using MoreConvenientJiraSvn.Core.Model;

namespace MoreConvenientJiraSvn.Core.Model;

public class EnumDescription
{
    public required Enum Value { get; set; }
    public string Description { get; set; } = string.Empty;
}