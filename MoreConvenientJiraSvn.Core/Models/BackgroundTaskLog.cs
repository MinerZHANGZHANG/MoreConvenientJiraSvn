﻿using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record BackgroundTaskLog
{
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    public string TaskName { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public bool IsSucccess { get; set; }
    public InfoLevel Level { get; set; }
    public DateTime StartTime { get; set; }

    [BsonIgnore]
    public IEnumerable<BackgroundTaskMessage> BackgroundTaskMessages { get; set; } = [];
    public string DisplayStartTime => StartTime.ToString("yyyy/MM/dd HH:mm");


}
