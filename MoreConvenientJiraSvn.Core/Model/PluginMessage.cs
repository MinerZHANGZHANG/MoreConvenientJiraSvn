using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record PluginMessage
    {
        public string? SourceName { get; set; }
        public string? Info { get; set; }
        public InfoLevel Level { get; set; }
        public DateTime? Time { get; set; }
    }

    public enum InfoLevel
    {
        [Description("一般")]
        Normal,
        [Description("警告")]
        Warning,
        [Description("错误")]
        Error,
    }
}
