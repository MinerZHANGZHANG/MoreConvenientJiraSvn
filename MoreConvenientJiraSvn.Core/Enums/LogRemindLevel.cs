using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum LogRemindLevel
{
    [Description("仅存储")]
    Normal = 0,
    [Description("弹窗提示")]
    Debug = 1,
}
