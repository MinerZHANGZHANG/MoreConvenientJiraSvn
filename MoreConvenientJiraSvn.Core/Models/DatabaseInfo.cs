using System;

namespace MoreConvenientJiraSvn.Core.Models;

public record DatabaseInfo
{
    public required int Version { get; set; }
    public DateTime UpdateTime { get; set; } = DateTime.Now;
}
