using MoreConvenientJiraSvn.Core.Models;
using System.Text.Json.Nodes;

namespace MoreConvenientJiraSvn.Core.Utils;

public static class JsonBuilder
{
    public static bool TryConvertJiraFieldsToJson(string transitionId, IEnumerable<JiraField> jiraFields, out string jsonResult)
    {
        jsonResult = string.Empty;

        JsonObject jsonObject = new()
        {
            ["transition"] = new JsonObject
            {
                ["id"] = transitionId
            },
            ["fields"] = new JsonObject()
        };
        foreach (var field in jiraFields)
        {
            var fieldsNode = jsonObject["fields"];
            if (fieldsNode == null)
            {
                continue;
            }

            if (field is JiraTextField textField)
            {
                fieldsNode[textField.Name] = textField.Value;
            }
            else if (field is JiraSelectField selectField)
            {
                fieldsNode[selectField.Name] = new JsonObject()
                {
                    ["name"] = selectField.Options.FirstOrDefault(o => o.IsSelected)?.OptionValue ?? string.Empty
                };
            }
            else if (field is JiraSelectField multipleSelectField)
            {
                fieldsNode[multipleSelectField.Name] = new JsonObject();
                foreach (var item in multipleSelectField.Options.Where(o => o.IsSelected))
                {
                    var parentNode = fieldsNode[multipleSelectField.Name];
                    if (parentNode == null)
                    {
                        continue;
                    }
                    parentNode["name"] = item.OptionValue;
                }
            }
            else if (field is JiraDateField dateField)
            {
                fieldsNode[dateField.Name] = dateField.Value.ToString("yyyy-MM-dd");
            }

        }
        jsonResult = jsonObject.ToJsonString();
        return false;
    }
}
