using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Core.Models;

public record AIServiceSetting
{
    public string ServiceAddress { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string SelectedModel { get; set; } = string.Empty;
    public ObservableCollection<string> Models { get; set; } = [];
    public bool EnableStreamResponse { get; set; } = false;
    public bool EnableMultiModalInput { get; set; } = false;
    public ObservableCollection<string> DefaultAccessedDirectories { get; set; } = [];
    public string PromptText { get; set; } = string.Empty;
}
