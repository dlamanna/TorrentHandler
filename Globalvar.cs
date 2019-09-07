using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TorrentHandler
{
    public class Globalvar
    {
        public static ChoiceForm choiceForm = null;
        public static Boolean isRelease = false;
        public static String currentDirectory = "";
        public static String MoviesPath = "";
        public static String TVPath = "";
        public static String GeneralPath = "";
        public static String MusicPath = "";
        public static String TVFocus = "";
        public static String MoviesFocus = "";
        public static String MusicFocus = "";
        public static String GeneralFocus = "";
        public static String torrentFile = "";

        public static void setPaths()
        {
            TVFocus = currentDirectory + "\\utorrentTV.exe";
            MoviesFocus = currentDirectory + "\\utorrentMovies.exe";
            MusicFocus = currentDirectory + "\\utorrentMusic.exe";
            GeneralFocus = currentDirectory + "\\utorrentGeneral.exe";
        }
        public static Boolean isReleaseVersion()
        {
            Assembly assembly = typeof(Program).Assembly;
            object[] attributes = assembly.GetCustomAttributes(typeof(DebuggableAttribute), true);
            if (attributes == null || attributes.Length == 0)
                return true;

            var d = (DebuggableAttribute)attributes[0];
            if ((d.DebuggingFlags & DebuggableAttribute.DebuggingModes.Default) == DebuggableAttribute.DebuggingModes.None)
                return true;

            return false;
        }
        public static String getSetting(String whichSetting)
        {
            String path = currentDirectory;
            String ret = "-1";
            String settingsPath = "";
            if (Directory.Exists(path))
            {
                settingsPath = path + "\\Settings.ini";
            }
            if (File.Exists(settingsPath))
            {
                String line;
                char delimiter = '=';
                System.IO.StreamReader settingsFile = new System.IO.StreamReader(settingsPath);
                while ((line = settingsFile.ReadLine()) != null)
                {
                    String[] substrings = line.Split(delimiter);
                    if (!substrings[0].Equals(null) && substrings[0].Equals(whichSetting))
                    {
                        ret = substrings[1];
                    }
                }

                settingsFile.Close();
            }
            else
                Console.WriteLine("### No settings file at: " + settingsPath);

            return ret;
        }
        public static Boolean scanTrackerFile(String trackerPath)
        {
            Boolean typeHit = false;
            if (File.Exists(torrentFile) && File.Exists(trackerPath))
            {
                String torrentFileContents = "";
                using (StreamReader sr = new StreamReader(torrentFile))
                {
                    torrentFileContents = sr.ReadToEnd();
                }

                String line;
                System.IO.StreamReader trackerFile = new System.IO.StreamReader(trackerPath);
                while ((line = trackerFile.ReadLine()) != null)
                {
                    if (torrentFileContents.Contains(line))
                    {
                        typeHit = true;
                    }
                }

                trackerFile.Close();
            }
            else
            {
                if (!File.Exists(torrentFile))
                    Console.WriteLine("### Torrent File not found: " + torrentFile);
                if (!File.Exists(trackerPath))
                    Console.WriteLine("### Tracker File not found: " + trackerPath);
            }
            return typeHit;
        }

    }
}
