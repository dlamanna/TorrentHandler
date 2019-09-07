using System;
using System.IO;
using System.Windows.Forms;

namespace TorrentHandler
{
    static class Program
    { 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Globalvar.choiceForm = new ChoiceForm();

            /// Initial Definitions
            Globalvar.isRelease = Globalvar.isReleaseVersion();
            if (Globalvar.isRelease)
                Globalvar.currentDirectory = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            else
                Globalvar.currentDirectory = Directory.GetCurrentDirectory();

            Globalvar.setPaths();

            String TVTrackerPath = Globalvar.currentDirectory + "\\TV.txt";
            String MovieTrackerPath = Globalvar.currentDirectory + "\\Movies.txt";
            String MusicTrackerPath = Globalvar.currentDirectory + "\\Music.txt";
            String GeneralTrackerPath = Globalvar.currentDirectory + "\\General.txt";
            Globalvar.MoviesPath = Globalvar.getSetting("Movies");
            Globalvar.TVPath = Globalvar.getSetting("TV");
            Globalvar.GeneralPath = Globalvar.getSetting("General");
            Globalvar.MusicPath = Globalvar.getSetting("Music");
            
            Globalvar.torrentFile = String.Join(" ", args);
            Boolean typeHit = false;
            typeHit = Globalvar.scanTrackerFile(TVTrackerPath);
            if (!typeHit) typeHit = Globalvar.scanTrackerFile(MovieTrackerPath);
            else
            {
                Globalvar.choiceForm.sendTorrent(Globalvar.TVFocus, Globalvar.TVPath);
                return;
            }

            if (!typeHit) typeHit = Globalvar.scanTrackerFile(MusicTrackerPath);
            else
            {
                Globalvar.choiceForm.sendTorrent(Globalvar.MoviesFocus, Globalvar.MoviesPath);
                return;
            }

            if (!typeHit) typeHit = Globalvar.scanTrackerFile(GeneralTrackerPath);
            else
            {
                Globalvar.choiceForm.sendTorrent(Globalvar.MusicFocus, Globalvar.MusicPath);
                return;
            }

            if (!typeHit) Application.Run(Globalvar.choiceForm);
            else Globalvar.choiceForm.sendTorrent(Globalvar.GeneralFocus, Globalvar.GeneralPath);
        }
    }
}
