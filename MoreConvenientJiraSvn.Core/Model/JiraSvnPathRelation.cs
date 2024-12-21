using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreConvenientJiraSvn.Core.Model
{
    /// <summary>
    /// In my surrounding，use FixVersions and JiraId link svnPath to jira
    /// </summary>
    public record JiraSvnPathRelation
    {
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public string SvnPath { get; set; } = string.Empty;

        public string FixVersion { get; set; } = string.Empty;
        public string JiraId { get; set; } = string.Empty;
    }
}
