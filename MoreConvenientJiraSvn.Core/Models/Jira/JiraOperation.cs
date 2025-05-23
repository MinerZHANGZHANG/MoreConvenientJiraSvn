﻿

using System.ComponentModel;

namespace MoreConvenientJiraSvn.Core.Models;

public abstract class JiraField
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsRequired { get; set; }
    public string? Description { get; set; }
    //public required FieldType Type { get; set; } 

    public abstract bool ValueIsEquals(JiraField jiraField);
}

public class JiraSelectField : JiraField, INotifyPropertyChanged
{
    public List<JiraFieldOption> Options { get; set; } = [];

    public JiraFieldOption? SelectedOption 
    { 
        get
        {
            return _selectedOption;
        }
        set
        {
            if (_selectedOption != value)
            {
                _selectedOption = value;
                OnPropertyChanged(nameof(SelectedOption));
            }
        }
    }
    private JiraFieldOption? _selectedOption;

    public event PropertyChangedEventHandler? PropertyChanged;

    public override bool ValueIsEquals(JiraField jiraField)
    {
        if (jiraField is JiraSelectField selectField)
        {
            return selectField.SelectedOption?.OptionId == this.SelectedOption?.OptionId;
        }

        return false;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class JiraMultiSelectField : JiraField
{
    public List<JiraFieldOption> Options { get; set; } = [];
    public List<JiraFieldOption>? Value { get; set; }

    public override bool ValueIsEquals(JiraField jiraField)
    {
        if (jiraField is JiraMultiSelectField selectField)
        {
            foreach (var option in Options)
            {
                var anotherOption = selectField.Options.First(o => o.OptionId == option.OptionId);
                if (anotherOption.IsSelected != option.IsSelected)
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }
}

public class JiraTextField : JiraField
{
    public string Value { get; set; } = string.Empty;

    public override bool ValueIsEquals(JiraField jiraField)
    {
        if (jiraField is JiraTextField textField)
        {
            return Value == textField.Value;
        }
        return false;
    }
}

// TODO: Differ format
public class JiraDateField : JiraField
{
    public DateTime Value { get; set; }

    public override bool ValueIsEquals(JiraField jiraField)
    {
        if (jiraField is JiraDateField dateField)
        {
            return Value == dateField.Value;
        }
        return false;
    }
}

public record JiraFieldOption : SelectItem
{
    public required string OptionId { get; set; }
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
