using System;
using System.IO;
using System.Windows.Forms;

namespace TorrentHandler
{
    internal static class Program
    { 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            GlobalVars.ChoiceForm = new ChoiceForm();

            GlobalVars.IsRelease = GlobalVars.IsReleaseVersion();
            GlobalVars.CurrentDirectory = GlobalVars.IsRelease
                ? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)
                : Directory.GetCurrentDirectory();

            GlobalVars.SetPaths();

            var tvTrackerPath = Path.Combine(GlobalVars.CurrentDirectory, "TV.txt");
            var movieTrackerPath = Path.Combine(GlobalVars.CurrentDirectory, "Movies.txt");
            var musicTrackerPath = Path.Combine(GlobalVars.CurrentDirectory, "Music.txt");
            var generalTrackerPath = Path.Combine(GlobalVars.CurrentDirectory, "General.txt");
            var gameTrackerPath = Path.Combine(GlobalVars.CurrentDirectory, "Games.txt");

            GlobalVars.MoviesPath = GlobalVars.GetSetting("Movies");
            GlobalVars.TvPath = GlobalVars.GetSetting("TV");
            GlobalVars.GeneralPath = GlobalVars.GetSetting("General");
            GlobalVars.GamesPath = GlobalVars.GetSetting("Games");
            GlobalVars.MusicPath = GlobalVars.GetSetting("Music");

            GlobalVars.TorrentFile = string.Join(" ", args);

            var trackers = new (string TrackerPath, string Focus, string HandlerPath)[]
            {
                (tvTrackerPath, GlobalVars.TvFocus, GlobalVars.TvPath),
                (movieTrackerPath, GlobalVars.MoviesFocus, GlobalVars.MoviesPath),
                (musicTrackerPath, GlobalVars.MusicFocus, GlobalVars.MusicPath),
                (gameTrackerPath, GlobalVars.GamesFocus, GlobalVars.GamesPath),
                (generalTrackerPath, GlobalVars.GeneralFocus, GlobalVars.GeneralPath)
            };

            foreach (var tracker in trackers)
            {
                if (GlobalVars.ScanTrackerFile(tracker.TrackerPath))
                {
                    GlobalVars.ChoiceForm.SendTorrent(tracker.Focus, tracker.HandlerPath);
                    return;
                }
            }

            Application.Run(GlobalVars.ChoiceForm);
        }
    }
}
