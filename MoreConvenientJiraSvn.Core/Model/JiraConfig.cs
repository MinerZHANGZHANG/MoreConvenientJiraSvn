using LiteDB;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record JiraConfig
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;
        public string ApiToken { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string TokenExpringAt { get; set; } = string.Empty;

    }
}
