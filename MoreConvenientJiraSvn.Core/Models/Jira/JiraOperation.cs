using MoreConvenientJiraSvn.Core.Enums;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Core.Models;

public record JiraOperation
{
    public required string OperationName { get; set; }
    public required string OperationId { get; set; }

    public ObservableCollection<JiraField> Fields { get; set; } = [];
}

public class JiraField
{
    public required string FieldId { get; set; }
    public required string FieldName { get; set; }
    public required FieldType Type { get; set; }

    public string? FieldValue { get; set; }
    public ObservableCollection<string>? SelectedValues { get; set; }
    public JiraFieldOption[]? Options { get; set; }
}

public record JiraFieldOption
{
    public required string OptionId { get; set; }
    public required string OptionName { get; set; }
}

public record JiraTransition
{
    public required string TransitionId { get; set; }
    public required string TransitionName { get; set; }
    public override string ToString()
    {
        return TransitionName;
    }
}
