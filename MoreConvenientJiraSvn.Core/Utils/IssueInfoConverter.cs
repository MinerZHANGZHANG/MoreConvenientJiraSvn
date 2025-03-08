using MoreConvenientJiraSvn.Core.Attributes;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Models;
using System.Text.Json;

namespace MoreConvenientJiraSvn.Core.Utils;

public static class IssueInfoConverter
{
    private const string FieldsNodeName = "fields";
    private const string ParentNodeName = "parent";
    private static readonly Lazy<Dictionary<string, IssueJsonMappingAttribute>> _propertyMapDict;
    public static Dictionary<string, IssueJsonMappingAttribute> PropertyMapDict => _propertyMapDict.Value;

    static IssueInfoConverter()
    {
        _propertyMapDict = new(InitPropertyMapDict);
    }

    private static Dictionary<string, IssueJsonMappingAttribute> InitPropertyMapDict()
    {
        var propertyMapDict = new Dictionary<string, IssueJsonMappingAttribute>();
        foreach (var property in typeof(JiraIssue).GetProperties())
        {
            var attributes = property.GetCustomAttributes(typeof(IssueJsonMappingAttribute), false);
            if (attributes.Length > 0)
            {
                var attribute = (IssueJsonMappingAttribute)attributes[0];
                attribute.PropertyInfo = property;
                propertyMapDict[attribute.Key] = attribute;
            }
        }
        return propertyMapDict;
    }

    public static bool TryGetIssueInfoFromJson(string jsonString, out JiraIssue? result, out string errorMsg)
    {
        errorMsg = string.Empty;
        try
        {
            result = GetIssueInfoFromJson(jsonString);
            if (result == null)
            {
                errorMsg = "trans result is null";
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            errorMsg = ex.Message;
            result = null;
        }
        return false;
    }


    public static JiraIssue? GetIssueInfoFromJson(string jsonString)
    {
        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return null;
        }

        using JsonDocument doc = JsonDocument.Parse(jsonString);
        if (doc.RootElement.TryGetProperty(FieldsNodeName, out var fieldsElement))
        {
            return null;
        }
        fieldsElement.TryGetProperty(ParentNodeName, out var parentElement);

        var IssueInfo = new JiraIssue();
        foreach (var propertyMap in PropertyMapDict)
        {
            if (propertyMap.Value.PropertyInfo == null)
            {
                continue;
            }

            JsonElement? targetElement = null;
            switch (propertyMap.Value.PositionType)
            {
                case JsonPositionType.Main:
                    targetElement = doc.RootElement;
                    break;
                case JsonPositionType.Fields:
                    targetElement = fieldsElement;
                    break;
                case JsonPositionType.Parent:
                    targetElement = parentElement;
                    break;
                default:
                    break;
            }

            if (propertyMap.Value.ChildKey != null
                && targetElement != null
                && propertyMap.Value.PropertyInfo.GetType() != typeof(List<string>))
            {
                targetElement.Value.TryGetProperty(propertyMap.Value.ChildKey, out var childElement);
                targetElement = childElement;
            }

            if (targetElement == null)
            {
                continue;
            }

            if (propertyMap.Value.PropertyInfo == typeof(List<string>) && targetElement?.ValueKind == JsonValueKind.Array)
            {
                List<string?> listResult = [];
                foreach (var item in targetElement.Value.EnumerateArray())
                {
                    if (propertyMap.Value.ChildKey == null)
                    {
                        listResult.Add(item.GetString());
                    }
                    else if (item.TryGetProperty(propertyMap.Value.ChildKey, out var arrayChildElement))
                    {
                        listResult.Add(arrayChildElement.GetString());
                    }
                }
                propertyMap.Value.PropertyInfo.SetValue(IssueInfo, listResult);
            }
            else if (propertyMap.Value.PropertyInfo == typeof(string))
            {
                propertyMap.Value.PropertyInfo.SetValue(IssueInfo, targetElement?.GetString());
            }
            else if (propertyMap.Value.PropertyInfo == typeof(double) && targetElement?.TryGetDouble(out var doubleResult) == true)
            {
                propertyMap.Value.PropertyInfo.SetValue(IssueInfo, doubleResult);
            }
            else if (propertyMap.Value.PropertyInfo == typeof(DateTime) && targetElement?.TryGetDateTime(out var dateTiemResult) == true)
            {
                propertyMap.Value.PropertyInfo.SetValue(IssueInfo, dateTiemResult);
            }
        }

        return IssueInfo;
    }
}

#region Todo: use reader instead
//JsonPositionType currentPositionType = JsonPositionType.Main;
//int parentStartTokenCount = 0;
//while (reader.Read())
//{
//    if (reader.TokenType == JsonTokenType.StartObject && currentPositionType == JsonPositionType.Parent)
//    {
//        parentStartTokenCount++;
//    }
//    else if (reader.TokenType == JsonTokenType.EndObject && currentPositionType == JsonPositionType.Parent)
//    {
//        parentStartTokenCount--;
//        if(parentStartTokenCount == 0)
//        {
//            currentPositionType = JsonPositionType.Fields;
//        }
//    }
//    else if (reader.TokenType == JsonTokenType.PropertyName)
//    {
//        string? propertyName = reader.GetString();
//        if (propertyName == FieldsNodeName)
//        {
//            currentPositionType = JsonPositionType.Fields;
//        }
//        else if (propertyName == ParentNodeName && currentPositionType == JsonPositionType.Fields)
//        {
//            currentPositionType = JsonPositionType.Parent;
//        }
//        else if (propertyName != null
//            && _propertyMapDict.TryGetValue(propertyName, out var propertyMap)
//            && propertyMap.PositionType == currentPositionType)
//        {
//            if (propertyMap.ChildKey == null)
//            {
//                reader.Read();
//                switch (reader.TokenType)
//                {
//                    case JsonTokenType.None:
//                        break;
//                    case JsonTokenType.StartObject:
//                        break;
//                    case JsonTokenType.EndObject:
//                        break;
//                    case JsonTokenType.StartArray:
//                        break;
//                    case JsonTokenType.EndArray:
//                        break;
//                    case JsonTokenType.PropertyName:
//                        break;
//                    case JsonTokenType.Comment:
//                        break;
//                    case JsonTokenType.String:
//                        break;
//                    case JsonTokenType.Number:
//                        break;
//                    case JsonTokenType.True:
//                        break;
//                    case JsonTokenType.False:
//                        break;
//                    case JsonTokenType.Null:
//                        break;
//                    default:
//                        break;
//                }
//            }
//        }
//    }
//}
#endregion
