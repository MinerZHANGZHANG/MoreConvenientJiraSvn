using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Core.Model;

public record OperationModel
{
    public required string OperationName { get; set; }
    public required string OperationId { get; set; }

    public ObservableCollection<FieldModel> Fields { get; set; } = new ObservableCollection<FieldModel>();
}

public class FieldModel
{
    public required string FieldId { get; set; }
    public required string FieldName { get; set; } = "Test";
    public required FieldType Type { get; set; }

    public string? FieldValue { get; set; } = "Test";
    public ObservableCollection<string>? SelectedValues { get; set; }
    public FieldOption[]? Options { get; set; }
}

public record FieldOption
{
    public required string OptionId { get; set; }
    public required string OptionName { get; set; }
}

public record Transition
{
    public required string TransitionId { get; set; }
    public required string TransitionName { get; set; }
    public override string ToString()
    {
        return TransitionName;
    }
}

public enum FieldType
{
    TextBox,
    BigTextBox,
    ComboBox,
    RadioButton,
    ListBox,
    DatePicker
}
