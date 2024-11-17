using LiteDB;

namespace MoreConvenientJiraSvn.Core.Model
{
    public class JiraFilter
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.NewObjectId();
        public string? FilterId { get; set; }
        public string? Name { get; set; }
        public string? Jql { get; set; }
        public string? SearchUrl { get; set; }
    }
}
