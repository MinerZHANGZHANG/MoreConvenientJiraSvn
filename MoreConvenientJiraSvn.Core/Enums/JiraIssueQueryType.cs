using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Enums;

public enum JiraIssueQueryType
{
    [Description("Jira编号")]
    JiraId,
    [Description("JQL")]
    Jql,
    [Description("筛选器")]
    Filter,
}
