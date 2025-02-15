using LiteDB;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Core.Models;

public record class SqlCreateInfo
{
    [BsonId]
    public required ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public required string Name { get; set; }
    public required string Template { get; set; }
    public Dictionary<string, string> Paramters { get; set; } = [];
    public required string Category { get; set; }
}

public class CategoryGroup
{
    public required string Category { get; set; }
    public ObservableCollection<SqlCreateInfo> Items { get; set; } = [];
}
