using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public class BackgroundTaskLog
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public IEnumerable<ObjectId> MessageIds { get; set; } = [];
    public string TaskName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsSucccess { get; set; }
    public InfoLevel Level { get; set; }
    public DateTime StartTime { get; set; }
}
