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
                fieldsNode[textField.Id] = textField.Value;
            }
            else if (field is JiraSelectField selectField)
            {
                fieldsNode[selectField.Id] = new JsonObject()
                {
                    ["name"] = selectField.Options.FirstOrDefault(o => o.IsSelected)?.Name ?? string.Empty
                };
            }
            else if (field is JiraMultiSelectField multipleSelectField)
            {
                fieldsNode[multipleSelectField.Id] = new JsonArray();
                foreach (var item in multipleSelectField.Options.Where(o => o.IsSelected))
                {
                    var parentNode = fieldsNode[multipleSelectField.Id];
                    if (parentNode is not JsonArray jsonArray)
                    {
                        continue;
                    }
                    jsonArray.Add(new JsonObject() { ["name"] = item.Name });
                }
            }
            else if (field is JiraDateField dateField)
            {
                fieldsNode[dateField.Id] = dateField.Value.ToString("yyyy-MM-dd");
            }

        }
        jsonResult = jsonObject.ToJsonString();
        return true;

    }
}
