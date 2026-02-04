using System.Text;

namespace TorrentHandler
{
    public sealed class TorrentMetadata
    {
        public string TorrentFile { get; init; } = string.Empty;
        public List<string> TrackerUrls { get; } = new();
        public List<string> TrackerDomains { get; } = new();
    }

    public static class TorrentParser
    {
        public static TorrentMetadata Parse(string torrentFile)
        {
            var metadata = new TorrentMetadata { TorrentFile = torrentFile };
            if (!File.Exists(torrentFile))
            {
                return metadata;
            }

            try
            {
                var data = File.ReadAllBytes(torrentFile);
                var index = 0;
                var root = ParseValue(data, ref index);

                if (root is Dictionary<string, object> dict)
                {
                    ExtractTrackers(dict, metadata);
                }
            }
            catch
            {
                // Intentionally ignore parse errors and fall back to manual selection.
            }

            return metadata;
        }

        private static void ExtractTrackers(Dictionary<string, object> dict, TorrentMetadata metadata)
        {
            var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (dict.TryGetValue("announce", out var announce) && announce is string announceUrl)
            {
                AddUrl(urls, announceUrl);
            }

            if (dict.TryGetValue("announce-list", out var announceList) && announceList is List<object> tiers)
            {
                foreach (var tier in tiers)
                {
                    if (tier is not List<object> tierList)
                    {
                        continue;
                    }

                    foreach (var entry in tierList)
                    {
                        if (entry is string url)
                        {
                            AddUrl(urls, url);
                        }
                    }
                }
            }

            foreach (var url in urls)
            {
                metadata.TrackerUrls.Add(url);
            }

            var domains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var url in urls)
            {
                var domain = TryGetDomain(url);
                if (!string.IsNullOrWhiteSpace(domain) && domains.Add(domain))
                {
                    metadata.TrackerDomains.Add(domain);
                }
            }
        }

        private static void AddUrl(HashSet<string> urls, string url)
        {
            var trimmed = url.Trim();
            if (trimmed.Length == 0)
            {
                return;
            }

            urls.Add(trimmed);
        }

        private static string? TryGetDomain(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            {
                return uri.Host.TrimEnd('.');
            }

            var working = url;
            var schemeIndex = working.IndexOf("://", StringComparison.Ordinal);
            if (schemeIndex >= 0)
            {
                working = working.Substring(schemeIndex + 3);
            }

            var slashIndex = working.IndexOf('/');
            if (slashIndex >= 0)
            {
                working = working.Substring(0, slashIndex);
            }

            var colonIndex = working.IndexOf(':');
            if (colonIndex >= 0)
            {
                working = working.Substring(0, colonIndex);
            }

            working = working.Trim();
            return working.Length == 0 ? null : working;
        }

        private static object ParseValue(byte[] data, ref int index)
        {
            if (index >= data.Length)
            {
                throw new FormatException("Unexpected end of data.");
            }

            var prefix = (char)data[index];
            return prefix switch
            {
                'i' => ParseInteger(data, ref index),
                'l' => ParseList(data, ref index),
                'd' => ParseDictionary(data, ref index),
                _ when char.IsDigit(prefix) => ParseString(data, ref index),
                _ => throw new FormatException($"Unexpected token '{prefix}'.")
            };
        }

        private static long ParseInteger(byte[] data, ref int index)
        {
            index++;
            var negative = false;

            if (data[index] == '-')
            {
                negative = true;
                index++;
            }

            long value = 0;
            while (index < data.Length && data[index] != 'e')
            {
                value = (value * 10) + (data[index] - '0');
                index++;
            }

            if (index >= data.Length)
            {
                throw new FormatException("Unterminated integer value.");
            }

            index++;
            return negative ? -value : value;
        }

        private static List<object> ParseList(byte[] data, ref int index)
        {
            index++;
            var list = new List<object>();

            while (index < data.Length && data[index] != 'e')
            {
                list.Add(ParseValue(data, ref index));
            }

            if (index >= data.Length)
            {
                throw new FormatException("Unterminated list.");
            }

            index++;
            return list;
        }

        private static Dictionary<string, object> ParseDictionary(byte[] data, ref int index)
        {
            index++;
            var dict = new Dictionary<string, object>(StringComparer.Ordinal);

            while (index < data.Length && data[index] != 'e')
            {
                var key = ParseString(data, ref index);
                var value = ParseValue(data, ref index);
                dict[key] = value;
            }

            if (index >= data.Length)
            {
                throw new FormatException("Unterminated dictionary.");
            }

            index++;
            return dict;
        }

        private static string ParseString(byte[] data, ref int index)
        {
            var length = 0;
            while (index < data.Length && data[index] != ':')
            {
                length = (length * 10) + (data[index] - '0');
                index++;
            }

            if (index >= data.Length)
            {
                throw new FormatException("Invalid string length.");
            }

            index++;
            if (index + length > data.Length)
            {
                throw new FormatException("String length exceeds data size.");
            }

            var value = Encoding.UTF8.GetString(data, index, length);
            index += length;
            return value;
        }
    }
}
