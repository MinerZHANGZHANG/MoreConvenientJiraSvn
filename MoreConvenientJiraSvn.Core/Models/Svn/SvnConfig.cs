﻿using LiteDB;
using MoreConvenientJiraSvn.Core.Enums;

namespace MoreConvenientJiraSvn.Core.Models;

public record SvnConfig
{
    public ObjectId Id { get; set; } = ObjectId.Empty;
    public string UserName { get; set; } = string.Empty;
    // Yes, use really value :), take care your data file
    public string UserPassword { get; set; } = string.Empty;
    public int MaxResultInSingleQuery { get; set; } = 65535;
    public List<ObjectId> PathIds { get; set; } = [];
}

public record SvnPath
{
    [BsonId]
    public string Path { get; set; } = string.Empty;
    public SvnPathType SvnPathType { get; set; } = SvnPathType.UnKnow;
    public string? LocalPath { get; set; }

    private string _pathName = string.Empty;
    public string PathName
    {
        get
        {
            if (string.IsNullOrEmpty(_pathName))
            {
                return Path;
            }
            return _pathName;
        }
        set
        {
            _pathName = value;
        }
    }

    public string FullPathName => string.IsNullOrWhiteSpace(_pathName) ? Path : $"({PathName}){Path}";

    public bool IsNeedExtractJiraId => SvnPathType == SvnPathType.Document || SvnPathType == SvnPathType.Code;

    public override string ToString()
    {
        return FullPathName;
    }
}

