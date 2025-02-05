using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum InfoLevel
{
    [Description("一般")]
    Normal,
    [Description("警告")]
    Warning,
    [Description("错误")]
    Error,
}
