using LiteDB;

namespace MoreConvenientJiraSvn.Plugin.Jira2LocalDir
{
    public record LocalJiraInfo
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public string JiraId { get; set; } = string.Empty;
        public string LocalDir { get; set; } = string.Empty;
    }
}
