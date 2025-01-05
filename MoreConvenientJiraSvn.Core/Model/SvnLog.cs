using LiteDB;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record SvnLog
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
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
    }

}
