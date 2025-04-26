using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record class ChatRecord
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public string ModelName { get; set; } = string.Empty;
    public string StartText { get; set; } = string.Empty;
    public DateTime LatestChatTime { get; set; } = DateTime.Now;
    public string ChatHistoryJson { get; set; } = string.Empty;
    public IEnumerable<string> FilePaths { get; set; } = [];
}
