using MoreConvenientJiraSvn.Core.Enums;
using System.Reflection;

namespace MoreConvenientJiraSvn.Core.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class IssueJsonMappingAttribute(string key, string? childKey = null, JsonPositionType PositionType = JsonPositionType.Fields) : Attribute
{
    public string Key { get; } = key;
    public string? ChildKey { get; } = childKey;
    public JsonPositionType PositionType { get; } = PositionType;

    public PropertyInfo? PropertyInfo { get; set; }
}
