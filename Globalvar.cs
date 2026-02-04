using System.Diagnostics;

namespace TorrentHandler
{
    public static class GlobalVars
    {
        public static bool IsRelease { get; set; }
        public static string CurrentDirectory { get; set; } = string.Empty;
        public static string ConfigPath { get; set; } = string.Empty;
        public static AppConfig Config { get; set; } = new();
        public static TorrentMetadata TorrentMetadata { get; set; } = new();

        public static bool IsReleaseVersion()
        {
#if DEBUG
            return false;
#else
            return !Debugger.IsAttached;
#endif
        }
    }
}
