namespace TorrentHandler
{
    public sealed class AppConfig
    {
        public int Version { get; set; } = 1;
        public List<ClientConfig> Clients { get; set; } = new();
        public List<CategoryConfig> Categories { get; set; } = new();
    }

    public sealed class ClientConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public FocusConfig Focus { get; set; } = new();
    }

    public sealed class FocusConfig
    {
        public FocusMode Mode { get; set; } = FocusMode.ProcessPath;
        public string? WindowClass { get; set; }
        public string? WindowTitle { get; set; }
        public string? WindowTitleContains { get; set; }
    }

    public enum FocusMode
    {
        None,
        ProcessPath,
        Window
    }

    public sealed class CategoryConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public List<RuleConfig> Rules { get; set; } = new();
    }

    public sealed class RuleConfig
    {
        public List<string> TrackerDomains { get; set; } = new();
    }
}
