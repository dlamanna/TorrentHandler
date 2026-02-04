using System;
using System.Diagnostics;
using System.IO;

namespace TorrentHandler
{
    public static class GlobalVars
    {
        public static ChoiceForm ChoiceForm { get; set; } = null!;
        public static bool IsRelease { get; set; }
        public static string CurrentDirectory { get; set; } = string.Empty;

        public static string MoviesPath { get; set; } = string.Empty;
        public static string TvPath { get; set; } = string.Empty;
        public static string GeneralPath { get; set; } = string.Empty;
        public static string GamesPath { get; set; } = string.Empty;
        public static string MusicPath { get; set; } = string.Empty;

        public static string TvFocus { get; private set; } = string.Empty;
        public static string MoviesFocus { get; private set; } = string.Empty;
        public static string MusicFocus { get; private set; } = string.Empty;
        public static string GeneralFocus { get; private set; } = string.Empty;
        public static string GamesFocus { get; private set; } = string.Empty;

        public static string TorrentFile { get; set; } = string.Empty;

        public static void SetPaths()
        {
            TvFocus = Path.Combine(CurrentDirectory, "utorrentTV.exe");
            MoviesFocus = Path.Combine(CurrentDirectory, "utorrentMovies.exe");
            MusicFocus = Path.Combine(CurrentDirectory, "utorrentMusic.exe");
            GeneralFocus = Path.Combine(CurrentDirectory, "utorrentGeneral.exe");
            GamesFocus = Path.Combine(CurrentDirectory, "utorrentGames.exe");
        }

        public static bool IsReleaseVersion()
        {
#if DEBUG
            return false;
#else
            return !Debugger.IsAttached;
#endif
        }

        public static string GetSetting(string settingName)
        {
            if (!Directory.Exists(CurrentDirectory))
            {
                Console.WriteLine("### Current directory not found: " + CurrentDirectory);
                return "-1";
            }

            var settingsPath = Path.Combine(CurrentDirectory, "Settings.ini");
            if (!File.Exists(settingsPath))
            {
                Console.WriteLine("### No settings file at: " + settingsPath);
                return "-1";
            }

            foreach (var line in File.ReadLines(settingsPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
                if (parts.Length == 2 && parts[0].Equals(settingName, StringComparison.OrdinalIgnoreCase))
                {
                    return parts[1];
                }
            }

            return "-1";
        }

        public static bool ScanTrackerFile(string trackerPath)
        {
            if (!File.Exists(TorrentFile) || !File.Exists(trackerPath))
            {
                if (!File.Exists(TorrentFile))
                {
                    Console.WriteLine("### Torrent File not found: " + TorrentFile);
                }

                if (!File.Exists(trackerPath))
                {
                    Console.WriteLine("### Tracker File not found: " + trackerPath);
                }

                return false;
            }

            var torrentFileContents = File.ReadAllText(TorrentFile);
            foreach (var line in File.ReadLines(trackerPath))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (torrentFileContents.Contains(line, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
