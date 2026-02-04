using System.IO;
using System.Windows.Forms;

namespace TorrentHandler
{
    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            GlobalVars.IsRelease = GlobalVars.IsReleaseVersion();
            GlobalVars.CurrentDirectory = GlobalVars.IsRelease
                ? AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)
                : Directory.GetCurrentDirectory();

            GlobalVars.Config = ConfigService.Load(GlobalVars.CurrentDirectory, out var configPath);
            GlobalVars.ConfigPath = configPath;

            var torrentFile = string.Join(" ", args).Trim();
            if (string.IsNullOrWhiteSpace(torrentFile))
            {
                MessageBox.Show("No torrent file provided.", "TorrentHandler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(torrentFile))
            {
                MessageBox.Show($"Torrent file not found: {torrentFile}", "TorrentHandler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            GlobalVars.TorrentMetadata = TorrentParser.Parse(torrentFile);

            var match = ConfigService.FindMatch(GlobalVars.Config, GlobalVars.TorrentMetadata.TrackerDomains);
            if (match != null)
            {
                TorrentLauncher.Launch(match, GlobalVars.Config, GlobalVars.TorrentMetadata, isManualSelection: false);
                return;
            }

            if (GlobalVars.Config.Categories.Count < 2)
            {
                MessageBox.Show("Config must define at least two categories.", "TorrentHandler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var form = new ChoiceForm(GlobalVars.Config, GlobalVars.TorrentMetadata);
            Application.Run(form);
        }
    }
}
