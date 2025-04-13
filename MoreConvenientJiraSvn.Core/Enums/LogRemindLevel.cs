using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum LogRemindLevel
{
    [Description("存储")]
    Normal = 0,
    [Description("控制台")]
    Debug = 1,
}
