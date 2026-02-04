using System.Text.Json;
using System.Text.Json.Serialization;

namespace TorrentHandler
{
    public static class ConfigService
    {
        private const string ConfigFileName = "config.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        public static AppConfig Load(string baseDirectory, out string configPath)
        {
            var appDataPath = GetAppDataConfigPath();
            var localPath = Path.Combine(baseDirectory, ConfigFileName);

            if (File.Exists(appDataPath))
            {
                configPath = appDataPath;
                return LoadFromPath(appDataPath);
            }

            if (File.Exists(localPath))
            {
                configPath = localPath;
                return LoadFromPath(localPath);
            }

            var legacyConfig = CreateFromLegacy(baseDirectory);
            configPath = appDataPath;
            Save(legacyConfig, configPath);
            return legacyConfig;
        }

        public static void Save(AppConfig config, string configPath)
        {
            var directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(config, JsonOptions);
            File.WriteAllText(configPath, json);
        }

        public static CategoryConfig? FindMatch(AppConfig config, IReadOnlyList<string> trackerDomains)
        {
            if (trackerDomains.Count == 0)
            {
                return null;
            }

            foreach (var category in config.Categories)
            {
                if (CategoryMatches(category, trackerDomains))
                {
                    return category;
                }
            }

            return null;
        }

        public static bool AddTrackersToCategory(AppConfig config, CategoryConfig category, IReadOnlyList<string> trackerDomains)
        {
            var normalized = NormalizeDomains(trackerDomains);
            if (normalized.Count == 0)
            {
                return false;
            }

            var existing = new HashSet<string>(
                category.Rules.SelectMany(rule => rule.TrackerDomains),
                StringComparer.OrdinalIgnoreCase);

            var targetRule = category.Rules.FirstOrDefault(rule => rule.TrackerDomains.Count > 0);
            if (targetRule == null)
            {
                targetRule = new RuleConfig();
                category.Rules.Add(targetRule);
            }

            var changed = false;
            foreach (var domain in normalized)
            {
                if (existing.Add(domain))
                {
                    targetRule.TrackerDomains.Add(domain);
                    changed = true;
                }
            }

            return changed;
        }

        private static string GetAppDataConfigPath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "TorrentHandler", ConfigFileName);
        }

        private static AppConfig LoadFromPath(string path)
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
            Normalize(config);
            return config;
        }

        private static bool CategoryMatches(CategoryConfig category, IReadOnlyList<string> trackerDomains)
        {
            foreach (var rule in category.Rules)
            {
                foreach (var ruleDomain in rule.TrackerDomains)
                {
                    foreach (var trackerDomain in trackerDomains)
                    {
                        if (DomainMatches(trackerDomain, ruleDomain))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool DomainMatches(string trackerDomain, string ruleDomain)
        {
            var tracker = trackerDomain.Trim();
            var rule = ruleDomain.Trim();

            if (tracker.Length == 0 || rule.Length == 0)
            {
                return false;
            }

            if (tracker.Equals(rule, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return tracker.EndsWith("." + rule, StringComparison.OrdinalIgnoreCase);
        }

        private static List<string> NormalizeDomains(IReadOnlyList<string> trackerDomains)
        {
            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var domain in trackerDomains)
            {
                var trimmed = domain.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (seen.Add(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }

        private static void Normalize(AppConfig config)
        {
            config.Clients ??= new List<ClientConfig>();
            config.Categories ??= new List<CategoryConfig>();

            foreach (var client in config.Clients)
            {
                client.Focus ??= new FocusConfig();
            }

            foreach (var category in config.Categories)
            {
                category.Rules ??= new List<RuleConfig>();
            }
        }

        private static AppConfig CreateFromLegacy(string baseDirectory)
        {
            var config = new AppConfig();
            var settings = ReadLegacySettings(baseDirectory);

            var mappings = new[]
            {
                new { Key = "TV", Label = "TV", Id = "tv", TrackerFile = "TV.txt" },
                new { Key = "Movies", Label = "Movies", Id = "movies", TrackerFile = "Movies.txt" },
                new { Key = "Music", Label = "Music", Id = "music", TrackerFile = "Music.txt" },
                new { Key = "Games", Label = "Games", Id = "games", TrackerFile = "Games.txt" },
                new { Key = "General", Label = "General", Id = "general", TrackerFile = "General.txt" }
            };

            foreach (var mapping in mappings)
            {
                settings.TryGetValue(mapping.Key, out var clientPath);
                var trackerDomains = ReadTrackerFile(baseDirectory, mapping.TrackerFile);

                if (string.IsNullOrWhiteSpace(clientPath) && trackerDomains.Count == 0)
                {
                    continue;
                }

                var clientId = mapping.Id;
                var client = new ClientConfig
                {
                    Id = clientId,
                    Label = mapping.Label,
                    Path = clientPath ?? string.Empty,
                    Focus = new FocusConfig
                    {
                        Mode = string.IsNullOrWhiteSpace(clientPath) ? FocusMode.None : FocusMode.ProcessPath
                    }
                };

                config.Clients.Add(client);

                var category = new CategoryConfig
                {
                    Id = mapping.Id,
                    Label = mapping.Label,
                    ClientId = clientId,
                    Rules = trackerDomains.Count > 0
                        ? new List<RuleConfig> { new RuleConfig { TrackerDomains = trackerDomains } }
                        : new List<RuleConfig>()
                };

                config.Categories.Add(category);
            }

            Normalize(config);
            return config;
        }

        private static Dictionary<string, string> ReadLegacySettings(string baseDirectory)
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var settingsPath = Path.Combine(baseDirectory, "Settings.ini");
            if (!File.Exists(settingsPath))
            {
                return settings;
            }

            foreach (var line in File.ReadLines(settingsPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                var parts = line.Split('=', 2, StringSplitOptions.TrimEntries);
                if (parts.Length != 2)
                {
                    continue;
                }

                var value = parts[1].Trim();
                if (value.Length >= 2 && value.StartsWith('"') && value.EndsWith('"'))
                {
                    value = value.Substring(1, value.Length - 2);
                }

                settings[parts[0]] = value;
            }

            return settings;
        }

        private static List<string> ReadTrackerFile(string baseDirectory, string fileName)
        {
            var trackerPath = Path.Combine(baseDirectory, fileName);
            var domains = new List<string>();
            if (!File.Exists(trackerPath))
            {
                return domains;
            }

            foreach (var line in File.ReadLines(trackerPath))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0)
                {
                    continue;
                }

                domains.Add(trimmed);
            }

            return domains;
        }
    }
}
