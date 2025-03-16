using MoreConvenientJiraSvn.Core.Models;
using AngleSharp.Html.Parser;
using MoreConvenientJiraSvn.Core.Interfaces;

namespace MoreConvenientJiraSvn.Infrastructure;

public class HtmlConvert : IHtmlConvert
{
    private HtmlParser? _htmlParser;

    public async Task<List<JiraField>> ConvertHtmlToJiraFieldsAsync(string htmlFormString, CancellationToken cancellationToken)
    {
        List<JiraField> jiraFields = [];

        var parser = _htmlParser ??= new();
        var document = await parser.ParseDocumentAsync(htmlFormString);

        var fieldDivs = document.QuerySelectorAll("div.field-group");
        foreach (var div in fieldDivs)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return [];
            }

            var label = div.QuerySelector("label");
            if (label == null)
            {
                continue;
            }

            var fieldId = label.GetAttribute("for");
            if (fieldId == null)
            {
                continue;
            }

            var input = div.QuerySelector("input.long-field,textarea.long-field,select.cf-select,fieldset.datepicker-params");
            if (input == null)
            {
                continue;
            }

            JiraField jiraField;
            switch (input.NodeName)
            {
                case "INPUT":
                    jiraField = new JiraTextField()
                    {
                        Id = fieldId,
                        Name = label.TextContent,
                        IsRequired = div.QuerySelector("span.icon-required") != null,
                        Description = div.QuerySelector("div.description")?.TextContent,

                        Value = input.GetAttribute("value") ?? string.Empty,
                    };
                    jiraFields.Add(jiraField);
                    break;
                case "TEXTAREA":
                    jiraField = new JiraTextField()
                    {
                        Id = fieldId,
                        Name = label.TextContent,
                        IsRequired = div.QuerySelector("span.icon-required") != null,
                        Description = div.QuerySelector("div.description")?.TextContent,

                        Value = input.TextContent,
                    };
                    jiraFields.Add(jiraField);
                    break;

                case "SELECT":
                    var options = input.QuerySelectorAll("option");
                    List<JiraFieldOption> jiraFieldOptions = [];
                    foreach (var option in options)
                    {
                        var optionId = option.GetAttribute("value");
                        var optionValue = option.TextContent;
                        var isSelected = option.GetAttribute("selected") != null;

                        if (optionId == null || optionValue == null)
                        {
                            continue;
                        }

                        JiraFieldOption jiraFieldOption = new()
                        {
                            OptionId = optionId,
                            OptionValue = optionValue,
                            IsSelected = isSelected
                        };

                        jiraFieldOptions.Add(jiraFieldOption);
                    }

                    jiraField = new JiraSelectField()
                    {
                        Id = fieldId,
                        Name = label.TextContent,
                        IsRequired = div.QuerySelector("span.icon-required") != null,
                        Description = div.QuerySelector("div.description")?.TextContent,

                        Options = jiraFieldOptions
                    };
                    jiraFields.Add(jiraField);
                    break;

                case "FIELDSET":
                    var dateInput = input.QuerySelector("input[title=\"date\"]");
                    if (dateInput == null)
                    {
                        continue;
                    }
                    var dateValueString = dateInput.GetAttribute("value");

                    jiraField = new JiraDateField()
                    {
                        Id = fieldId,
                        Name = label.TextContent,
                        IsRequired = div.QuerySelector("span.icon-required") != null,
                        Description = div.QuerySelector("div.description")?.TextContent,

                        Value = string.IsNullOrEmpty(dateValueString)
                                    ? DateTime.Now
                                    : DateTime.Parse(dateValueString)
                    };
                    jiraFields.Add(jiraField);
                    break;
            }
        }

        return jiraFields;
    }
}
