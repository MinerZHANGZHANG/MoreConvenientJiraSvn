using Microsoft.SemanticKernel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace MoreConvenientJiraSvn.Service.Plugins;

public class IssueInfoPlugin(IEnumerable<IssueFile> issueFiles)
{
    private readonly JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        WriteIndented = false,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
        }
    };

    [KernelFunction("get_file_list")]
    public IEnumerable<IssueFile> GetFileList()
    {
        return issueFiles;
    }

    [KernelFunction("get_file_content_by_id")]
    public async Task<string> GetFileContentById(int id)
    {
        var file = issueFiles.FirstOrDefault(x => x.Id == id);
        if (file == null)
        {
            return $"File with ID {id} not found.";
        }

        if (!File.Exists(file.Path))
        {
            return $"File with path {file.Path} not found.";
        }

        var fileContent = await File.ReadAllTextAsync(file.Path);

        return file.Type switch
        {
            FileType.Code => GetIssueCodeChange(fileContent),
            FileType.Sql => GetIssueSqlChange(fileContent),
            FileType.Doc => GetIssueDocChange(fileContent),
            _ => fileContent,
        };
    }

    public string GetIssueCodeChange(string content)
    {
        // Just for test
        var result = new ModifiedPatch
        {
            ModifiedFiles = [new ModifiedFile()
            {
                FilePath="main/App.xaml.cs",
                Operation= FileOperation.Change,
                Snippets=[new ModifiedCodeSnippet()
                {
                    DeletedPart="serviceProvider.GetRequiredService<ISqliteService>();",
                    AddPart="serviceProvider.GetRequiredService<ILiteDbService>();"
                }]
            }]
        };

        return JsonSerializer.Serialize(result, DefaultJsonSerializerOptions); ;
    }

    public string GetIssueSqlChange(string content)
    {
        return content;
    }

    public string GetIssueDocChange(string content)
    {
        // Todo: Remove sql prompt

        return content;
    }
}

public class IssueFile
{
    [JsonPropertyName("id")]
    public required int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("file_type")]
    public required FileType Type { get; set; }

    [JsonIgnore]
    public required string Path { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileOperation
{
    Add,
    Change,
    Delete
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum FileType
{
    Other,
    Code,
    Sql,
    Doc
}

public class ModifiedPatch
{
    public IEnumerable<ModifiedFile> ModifiedFiles { get; set; } = [];
}

public class ModifiedFile
{
    public required string FilePath { get; set; }
    public required FileOperation Operation { get; set; }
    public IEnumerable<ModifiedCodeSnippet> Snippets { get; set; } = [];
}

public class ModifiedCodeSnippet
{
    public string DeletedPart { get; set; } = string.Empty;
    public string AddPart { get; set; } = string.Empty;
}
