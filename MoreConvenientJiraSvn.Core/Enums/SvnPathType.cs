using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum SvnPathType
{
    [Description("未知")]
    UnKnow,
    [Description("文档")]
    Document,
    [Description("代码")]
    Code,
}
