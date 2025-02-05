namespace MoreConvenientJiraSvn.Core.Models;

public class EnumDescription
{
    public required Enum Value { get; set; }
    public string Description { get; set; } = string.Empty;
}