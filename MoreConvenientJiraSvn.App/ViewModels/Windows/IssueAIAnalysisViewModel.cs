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
        [ObservableProperty]
        private string? _promptText;

        [ObservableProperty]
        private string? _inputText;

        [ObservableProperty]
        private string? _outputText;

        [ObservableProperty]
        private string? _selectedModel;

        [ObservableProperty]
        private ObservableCollection<string> _models = [];

        [ObservableProperty]
        private string? _aiServiceAddress;

        [ObservableProperty]
        private string? _apiKey;

        [ObservableProperty]
        private bool _isStreamResponse;

        [ObservableProperty]
        private ObservableCollection<IssueFile> _issueFiles = [];

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
