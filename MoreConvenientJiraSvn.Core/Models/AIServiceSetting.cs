using CommunityToolkit.Mvvm.ComponentModel;
using LiteDB;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.Core.Models;

public class AIServiceSetting : ObservableObject
{
    public ObjectId Id { get; set; } = ObjectId.Empty;

    private string _serviceAddress = string.Empty;
    public string ServiceAddress
    {
        get => _serviceAddress;
        set => SetProperty(ref _serviceAddress, value);
    }

    private string _apiKey = string.Empty;
    public string ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    private string _selectedModel = string.Empty;
    public string SelectedModel
    {
        get => _selectedModel;
        set => SetProperty(ref _selectedModel, value);
    }

    private ObservableCollection<string> _models = [];
    public ObservableCollection<string> Models
    {
        get => _models;
        set => SetProperty(ref _models, value);
    }

    private bool _enableStreamResponse = false;
    public bool EnableStreamResponse
    {
        get => _enableStreamResponse;
        set => SetProperty(ref _enableStreamResponse, value);
    }

    private bool _enableMultiModalInput = false;
    public bool EnableMultiModalInput
    {
        get => _enableMultiModalInput;
        set => SetProperty(ref _enableMultiModalInput, value);
    }

    private ObservableCollection<string> _defaultAccessedDirectories = [];
    public ObservableCollection<string> DefaultAccessedDirectories
    {
        get => _defaultAccessedDirectories;
        set => SetProperty(ref _defaultAccessedDirectories, value);
    }

    private string _promptText = string.Empty;
    public string PromptText
    {
        get => _promptText;
        set => SetProperty(ref _promptText, value);
    }
}
