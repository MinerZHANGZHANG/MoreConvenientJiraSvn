﻿using LiteDB;

namespace MoreConvenientJiraSvn.Core.Models;

public record SqlCheckSetting
{
    /// <summary>
    /// Global setting
    /// </summary>
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string DefaultDir { get; set; } = string.Empty;
}
