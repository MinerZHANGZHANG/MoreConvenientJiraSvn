using LiteDB;

namespace MoreConvenientJiraSvn.Core.Model
{
    public record HostedServiceConfig
    {
        [BsonId]
        public ObjectId Id { get; set; } = ObjectId.Empty;

        public List<string> CheckSqlDirectoies { get; set; } = [];
    }
}

