using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace TorrentHandler
{
    public static class TorrentLauncher
    {
        public static void Launch(CategoryConfig category, AppConfig config, TorrentMetadata metadata, bool isManualSelection)
        {
            var client = config.Clients.FirstOrDefault(c =>
                c.Id.Equals(category.ClientId, StringComparison.OrdinalIgnoreCase));

            if (client == null)
            {
                MessageBox.Show($"No client found for category '{category.Label}'.", "TorrentHandler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(client.Path))
            {
                MessageBox.Show($"Client path missing for '{category.Label}'.", "TorrentHandler",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FocusService.FocusClient(client);
            StartProgram(client.Path, QuoteArgument(metadata.TorrentFile));

            if (isManualSelection)
            {
                var changed = ConfigService.AddTrackersToCategory(config, category, metadata.TrackerDomains);
                if (changed)
                {
                    var configPath = string.IsNullOrWhiteSpace(GlobalVars.ConfigPath)
                        ? Path.Combine(GlobalVars.CurrentDirectory, "config.json")
                        : GlobalVars.ConfigPath;

                    ConfigService.Save(config, configPath);
                }
            }
        }

        private static void StartProgram(string fileName, string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
        }

        private static string QuoteArgument(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return value.Contains(' ') ? $"\"{value}\"" : value;
        }
    }
}
