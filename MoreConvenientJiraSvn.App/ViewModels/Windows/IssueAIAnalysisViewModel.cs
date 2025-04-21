using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MoreConvenientJiraSvn.Service;
using MoreConvenientJiraSvn.Service.Plugins;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels
{
    public partial class IssueAIAnalysisViewModel(SemanticKernelService semanticKernelService, LogService logService) : ObservableObject, INotifyPropertyChanged
    {
        #region Model setting

        [ObservableProperty]
        private string? _aiServiceAddress;

        [ObservableProperty]
        private string? _apiKey;

        [ObservableProperty]
        private bool _isStreamResponse;

        [ObservableProperty]
        private string? _selectedModel;

        [ObservableProperty]
        private ObservableCollection<string> _models = [];

        [ObservableProperty]
        private bool _enableMultiModalInput;

        [ObservableProperty]
        private ObservableCollection<string> _aiDefaultAccessedDirectory = [];

        [ObservableProperty]
        private string? _promptText;

        public readonly IEnumerable<(string, string)> DefaultPromptList =
        [
            new (".NET代码分析","你是一位资深.NET开发人员，精通.NET6.0版本的WPF开发，并擅长分析代码，从中发现问题和提出优化建议"),
            new ("Issue总结","你是一位资深的产品经理，你了解软件开发和用户交互，你擅长分析总结代码和相关的文档，并产出为测试和实施人员可以理解的内容"),
        ];

        #endregion

        #region Current chat

        [ObservableProperty]
        private ObservableCollection<IssueFile> _issueFiles = [];

        [ObservableProperty]
        private string? _issueKey;

        [ObservableProperty]
        private string? _inputText;

        [ObservableProperty]
        private string? _outputText;

        #endregion

        #region History chat

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private ObservableCollection<ChatHistory> _chatHistories = [];

        #endregion

        #region Command

        [RelayCommand]
        public void AddAIDefaultAccessedDirectory()
        {

        }

        [RelayCommand]
        public void RemoveAIDefaultAccessedDirectory(string folderPath)
        {

        }

        [RelayCommand]
        public void AddAIAccessedFile()
        {

        }

        [RelayCommand]
        public void RemoveAIAccessedFile()
        {

        }

        [RelayCommand]
        public void ShowTipMessage(string message)
        {

        }

        [RelayCommand]
        public void LoadDefaultFilesByIssueKey()
        {

        }

        [RelayCommand]
        public void QueryChatHistories()
        {

        }

        [RelayCommand]
        public void SendQuestion()
        {

        }

        #endregion

        #region Help Function

        public void SaveModelSetting()
        {

        }


        #endregion
        public void StartNewChat()
        {
            // Initialize the chat history or any other necessary setup
            OutputText = string.Empty;
            InputText = string.Empty;
        }

        public void LoadHistoryChat()
        {
            // Load the chat history from the database or any other source
            // and populate the OutputText and InputText properties accordingly
        }

        [RelayCommand]
        public async Task AskQuestion()
        {
            if (string.IsNullOrEmpty(InputText) || string.IsNullOrEmpty(AiServiceAddress)
                || string.IsNullOrEmpty(SelectedModel) || string.IsNullOrEmpty(ApiKey))
            {
                MessageBox.Show("Please enter a necessary info.");
                return;
            }

            var kernel = semanticKernelService.BuildKernelWithChatCompletion(
                baseAddress: AiServiceAddress,
                modelId: SelectedModel,
                apiKey: ApiKey,
                replaceToEmptyStrings: ["\"strict\":false,"]
            );
            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                    options: new FunctionChoiceBehaviorOptions
                    {
                        AllowStrictSchemaAdherence = false
                    }),
            };

            // Todo: store the history
            var history = new ChatHistory();

            if (!string.IsNullOrEmpty(PromptText))
            {
                history.AddSystemMessage(PromptText);
            }

            history.AddUserMessage(InputText);

            try
            {
                if (IsStreamResponse)
                {
                    var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatHistory: history,
                        executionSettings: openAIPromptExecutionSettings,
                        kernel: kernel
                     );
                    OutputText = string.Empty;
                    await foreach (var chunk in response)
                    {
                        OutputText += chunk?.ToString() ?? string.Empty;
                    }
                }
                else
                {
                    var response = await chatCompletionService.GetChatMessageContentAsync(
                        chatHistory: history,
                        executionSettings: openAIPromptExecutionSettings,
                        kernel: kernel
                     );
                    OutputText = response.ToString() ?? string.Empty;
                }

                // Get the new messages added to the chat history object
                for (int i = 0; i < history.Count; i++)
                {
                    if (history[i] is null)
                    {
                        continue;
                    }
                    logService.LogDebug(history[i].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while processing the request\n{ex.Message}");
            }

        }
    }
}
