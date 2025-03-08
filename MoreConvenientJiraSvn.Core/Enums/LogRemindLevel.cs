using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum LogRemindLevel
{
    [Description("仅存储")]
    Normal = 0,
    [Description("弹窗提示")]
    Debug = 1,
}
