using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Models;

public struct IssuePageInfo
{
    public int StartAt {  get; set; }
    public int MaxResults {  get; set; }
    public int Total {  get; set; }
    public List<IssueInfo> IssueInfos { get; set; }
    public string ErrorMessage { get; set; }
}
