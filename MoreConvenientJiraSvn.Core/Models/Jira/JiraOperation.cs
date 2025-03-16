

namespace MoreConvenientJiraSvn.Core.Models;

public abstract class JiraField
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsRequired { get; set; }
    public string? Description { get; set; }
    //public required FieldType Type { get; set; } 
}

public class JiraSelectField : JiraField
{
    public List<JiraFieldOption> Options { get; set; } = [];
    public JiraFieldOption? Value { get; set; }
}

public class JiraTextField : JiraField
{
    public string Value { get; set; } = string.Empty;
}

public class JiraDateField : JiraField
{
    public DateTime Value { get; set; }
}

public record JiraFieldOption
{
    public required string OptionId { get; set; }
    public required string OptionValue { get; set; }
    public required bool IsSelected { get; set; }
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
