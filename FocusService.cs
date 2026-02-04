using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace TorrentHandler
{
    public static class FocusService
    {
        private const int SwRestore = 9;
        private const int SwShow = 5;

        private static readonly IntPtr HwndTopMost = new IntPtr(-1);
        private static readonly IntPtr HwndNoTopMost = new IntPtr(-2);
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpShowWindow = 0x0040;

        private const int GwlExStyle = -20;
        private const int WsExAppWindow = 0x00040000;
        private const int WsExToolWindow = 0x00000080;

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public static bool FocusClient(ClientConfig client)
        {
            if (client.Focus == null || client.Focus.Mode == FocusMode.None)
            {
                return true;
            }

            return client.Focus.Mode switch
            {
                FocusMode.Window => FocusWindow(client),
                FocusMode.ProcessPath => FocusProcess(client),
                _ => false
            };
        }

        private static bool FocusProcess(ClientConfig client)
        {
            if (string.IsNullOrWhiteSpace(client.Path))
            {
                return false;
            }

            var process = FindProcess(client) ?? TryStartProcess(client.Path);
            if (process == null)
            {
                return false;
            }

            var handle = process.MainWindowHandle;
            if (handle == IntPtr.Zero)
            {
                handle = WaitForMainWindow(process, TimeSpan.FromSeconds(3));
            }

            if (handle == IntPtr.Zero)
            {
                handle = WaitForWindow(() => FindWindowForProcess(process), TimeSpan.FromSeconds(3));
            }

            if (handle == IntPtr.Zero)
            {
                return false;
            }

            ForceForegroundWindow(handle);
            return true;
        }

        private static bool FocusWindow(ClientConfig client)
        {
            var focus = client.Focus ?? new FocusConfig();
            var handle = FindWindowByCriteria(focus);

            if (handle == IntPtr.Zero && !string.IsNullOrWhiteSpace(client.Path))
            {
                TryStartProcess(client.Path);
                handle = WaitForWindow(() => FindWindowByCriteria(focus), TimeSpan.FromSeconds(3));
            }

            if (handle == IntPtr.Zero)
            {
                return false;
            }

            ForceForegroundWindow(handle);
            return true;
        }

        private static Process? FindProcess(ClientConfig client)
        {
            foreach (var process in Process.GetProcesses())
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(client.Focus?.WindowTitleContains))
                    {
                        var title = process.MainWindowTitle;
                        if (title.IndexOf(client.Focus.WindowTitleContains, StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            continue;
                        }
                    }

                    var path = process.MainModule?.FileName;
                    if (path == null)
                    {
                        continue;
                    }

                    if (string.Equals(path, client.Path, StringComparison.OrdinalIgnoreCase))
                    {
                        return process;
                    }
                }
                catch
                {
                    // Ignore processes we cannot inspect.
                }
            }

            return null;
        }

        private static IntPtr FindWindowForProcess(Process process)
        {
            if (process.HasExited)
            {
                return IntPtr.Zero;
            }

            var processId = (uint)process.Id;
            IntPtr best = IntPtr.Zero;
            int bestScore = int.MinValue;
            long bestArea = -1;

            EnumWindows((hWnd, _) =>
            {
                GetWindowThreadProcessId(hWnd, out var windowProcessId);
                if (windowProcessId != processId)
                {
                    return true;
                }

                var score = 0;
                var title = GetWindowTitle(hWnd);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    score += 50;
                }

                var className = GetWindowClass(hWnd);
                if (!string.IsNullOrWhiteSpace(className) &&
                    className.IndexOf("torrent", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 20;
                }

                if (IsWindowVisible(hWnd))
                {
                    score += 10;
                }

                if (!IsIconic(hWnd))
                {
                    score += 5;
                }

                var exStyle = GetWindowExStyle(hWnd);
                if ((exStyle & WsExAppWindow) != 0)
                {
                    score += 10;
                }

                if ((exStyle & WsExToolWindow) != 0)
                {
                    score -= 10;
                }

                var area = GetWindowArea(hWnd);
                if (area > 0)
                {
                    score += (int)Math.Min(area / 10000, 20);
                }

                if (score > bestScore || (score == bestScore && area > bestArea))
                {
                    bestScore = score;
                    bestArea = area;
                    best = hWnd;
                }

                return true;
            }, IntPtr.Zero);

            return best;
        }

        private static int GetWindowExStyle(IntPtr hWnd)
        {
            if (IntPtr.Size == 8)
            {
                return unchecked((int)GetWindowLongPtr64(hWnd, GwlExStyle).ToInt64());
            }

            return GetWindowLong32(hWnd, GwlExStyle);
        }

        private static long GetWindowArea(IntPtr hWnd)
        {
            if (!GetWindowRect(hWnd, out var rect))
            {
                return 0;
            }

            var width = Math.Max(0, rect.Right - rect.Left);
            var height = Math.Max(0, rect.Bottom - rect.Top);
            return (long)width * height;
        }

        private static void ForceForegroundWindow(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            if (IsIconic(handle))
            {
                ShowWindow(handle, SwRestore);
            }
            else
            {
                ShowWindow(handle, SwShow);
            }

            BringWindowToTop(handle);

            var foreground = GetForegroundWindow();
            var currentThread = GetCurrentThreadId();
            var foregroundThread = GetWindowThreadProcessId(foreground, out _);

            try
            {
                if (foregroundThread != currentThread)
                {
                    AttachThreadInput(foregroundThread, currentThread, true);
                }

                SetForegroundWindow(handle);
                SetWindowPos(handle, HwndTopMost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpShowWindow);
                SetWindowPos(handle, HwndNoTopMost, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpShowWindow);
            }
            finally
            {
                if (foregroundThread != currentThread)
                {
                    AttachThreadInput(foregroundThread, currentThread, false);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private static Process? TryStartProcess(string path)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process == null)
                {
                    return null;
                }

                process.WaitForInputIdle(2000);
                return process;
            }
            catch
            {
                return null;
            }
        }

        private static IntPtr FindWindowByCriteria(FocusConfig focus)
        {
            var targetClass = focus.WindowClass?.Trim();
            var targetTitle = focus.WindowTitle?.Trim();
            var targetTitleContains = focus.WindowTitleContains?.Trim();
            var hasExactTitle = !string.IsNullOrWhiteSpace(targetTitle);
            var hasContainsTitle = !hasExactTitle && !string.IsNullOrWhiteSpace(targetTitleContains);

            IntPtr found = IntPtr.Zero;

            EnumWindows((hWnd, _) =>
            {
                if (!string.IsNullOrWhiteSpace(targetClass))
                {
                    var className = GetWindowClass(hWnd);
                    if (!string.Equals(className, targetClass, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                var title = GetWindowTitle(hWnd);
                if (hasExactTitle)
                {
                    if (!string.Equals(title, targetTitle, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                else if (hasContainsTitle)
                {
                    if (title.IndexOf(targetTitleContains!, StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        return true;
                    }
                }

                if ((hasExactTitle || hasContainsTitle) && string.IsNullOrWhiteSpace(title))
                {
                    return true;
                }

                found = hWnd;
                return false;
            }, IntPtr.Zero);

            return found;
        }

        private static string GetWindowClass(IntPtr hWnd)
        {
            var buffer = new StringBuilder(256);
            return GetClassName(hWnd, buffer, buffer.Capacity) > 0 ? buffer.ToString() : string.Empty;
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            var buffer = new StringBuilder(512);
            return GetWindowText(hWnd, buffer, buffer.Capacity) > 0 ? buffer.ToString() : string.Empty;
        }

        private static IntPtr WaitForWindow(Func<IntPtr> finder, TimeSpan timeout)
        {
            var stopAt = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < stopAt)
            {
                var handle = finder();
                if (handle != IntPtr.Zero)
                {
                    return handle;
                }

                Thread.Sleep(100);
            }

            return IntPtr.Zero;
        }

        private static IntPtr WaitForMainWindow(Process process, TimeSpan timeout)
        {
            var stopAt = DateTime.UtcNow + timeout;
            while (DateTime.UtcNow < stopAt)
            {
                process.Refresh();
                if (process.MainWindowHandle != IntPtr.Zero)
                {
                    return process.MainWindowHandle;
                }

                Thread.Sleep(100);
            }

            return IntPtr.Zero;
        }
    }
}
