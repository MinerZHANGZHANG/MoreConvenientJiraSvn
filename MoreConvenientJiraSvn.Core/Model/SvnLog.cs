using LiteDB;
using MoreConvenientJiraSvn.Core.Service;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record SvnLog
    {
        [BsonId]
        public required ObjectId Id { get; set; }
        public required string SvnPath { get; set; }
        public required long Revision { get; set; }
        public required string Operation { get; set; }
        public required string Author { get; set; }
        public required DateTime DateTime { get; set; }
        public string? Message { get; set; }
        public required List<string> ChangedUrls { get; set; }

        // special property
        public string? IssueJiraId { get; set; }
        public string? SubIssueJiraId { get; set; }

        public static ObjectId GetKey(string svnPath,long Revision)
        {
            return DataService.ConvertToObjectId($"{svnPath}-{Revision:16d}");
        }

        // rewrite equals?
    }

}
