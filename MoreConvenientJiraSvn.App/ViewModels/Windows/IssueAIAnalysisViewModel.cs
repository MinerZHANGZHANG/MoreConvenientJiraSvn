using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiteDB;
using MaterialDesignThemes.Wpf;
using MdXaml;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using Microsoft.Win32;
using MoreConvenientJiraSvn.App.Utils;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using MoreConvenientJiraSvn.Service.Plugins;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MoreConvenientJiraSvn.App.ViewModels
{
    public partial class IssueAIAnalysisViewModel(SemanticKernelService semanticKernelService, SettingService settingService, LogService logService, IRepository repository) : ObservableObject, INotifyPropertyChanged
    {
        private const int RefreshIntervalSecond = 1;

        [ObservableProperty]
        private AIServiceSetting _AIServiceSetting = new();

        public IEnumerable<KeyValuePair<string, string>> DefaultPromptList { get; } =
        [
            new (".NET代码分析","你是一位资深.NET开发人员，精通.NET6.0版本的WPF开发，并擅长分析代码，从中发现问题和提出优化建议"),
            new ("Issue总结","你是一位资深的产品经理，你了解软件开发和用户交互，你擅长分析总结代码和相关的文档，并产出为测试和实施人员可以理解的内容"),
        ];

        [ObservableProperty]
        private string _tipMessage = string.Empty;

        [ObservableProperty]
        private Visibility _dialogCanelButtonVisibility = Visibility.Visible;

        private Markdown _markDownEngine = new();

        #region Current chat

        [ObservableProperty]
        private ObservableCollection<IssueFile> _issueFiles = [];

        [ObservableProperty]
        private string? _issueKey;

        [ObservableProperty]
        private string? _inputText;

        [ObservableProperty]
        private FlowDocument _chatFlowDocument = new();

        private ChatContext? _chatContext;

        [ObservableProperty]
        private bool _isWaitResponse = false;

        #endregion

        #region History chat

        [ObservableProperty]
        private DateTime _startDate;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private ObservableCollection<ChatRecord> _chatHistories = [];

        #endregion

        public async Task Initialize()
        {
            AIServiceSetting = settingService.FindSetting<AIServiceSetting>() ?? new();

            StartDate = DateTime.Now.AddDays(-7);
            EndDate = DateTime.Now.AddDays(1);
            await QueryChatHistories();
        }

        #region Command

        [RelayCommand]
        public void AddAIDefaultAccessedDirectory()
        {
            OpenFolderDialog openFolderDialog = new()
            {
                Multiselect = true,
                Title = "选择默认访问的目录",
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                string[] folderPaths = openFolderDialog.FolderNames;
                foreach (var folderPath in folderPaths)
                {
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        return;
                    }
                    if (AIServiceSetting.DefaultAccessedDirectories.Contains(folderPath))
                    {
                        return;
                    }
                    AIServiceSetting.DefaultAccessedDirectories.Add(folderPath);
                }
            }
        }

        [RelayCommand]
        public void RemoveAIDefaultAccessedDirectory(string folderPath)
        {
            AIServiceSetting.DefaultAccessedDirectories.Remove(folderPath);
        }

        [RelayCommand]
        public void AddAIAccessedFile()
        {
            string filter = "支持的文件类型|*.patch;*.pdf;*.txt;*.xlsx;*.xls;*.csv;*.doc;*.docx;*.png;*.jpg;*.sql|" +
            "补丁文件 (*.patch)|*.patch|" +
            "PDF文档 (*.pdf)|*.pdf|" +
            "文本文件 (*.txt)|*.txt|" +
            "Excel文件 (*.xls;*.xlsx)|*.xls;*.xlsx|" +
            "CSV数据表 (*.csv)|*.csv|" +
            "Word文档 (*.doc;*.docx)|*.doc;*.docx|" +
            "图片文件 (*.png;*.jpg)|*.png;*.jpg|" +
            "SQL脚本 (*.sql)|*.sql|" +
            "所有文件 (*.*)|*.*";

            var openFileDialog = new OpenFileDialog
            {
                Filter = filter,  // 文件类型过滤
                Multiselect = true  // 允许多选文件
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string[] selectedFiles = openFileDialog.FileNames;

                foreach (var file in selectedFiles)
                {
                    var fileType = Path.GetExtension(file).ToLowerInvariant() switch
                    {
                        ".patch" => FileType.Code,
                        ".sql" => FileType.Sql,
                        _ => FileType.Doc,
                    };
                    var fileName = Path.GetFileName(file);
                    var filePath = Path.GetFullPath(file);

                    if (IssueFiles.Any(x => x.Path == filePath))
                    {
                        continue;
                    }

                    IssueFiles.Add(new IssueFile
                    {
                        Id = IssueFiles.Count + 1,
                        Name = fileName,
                        Path = filePath,
                        Type = fileType
                    });
                }
            }
        }

        [RelayCommand]
        public void RemoveAIAccessedFile(int fileId)
        {
            var issueFile = IssueFiles.FirstOrDefault(x => x.Id == fileId);
            if (issueFile != null)
            {
                IssueFiles.Remove(issueFile);
            }
        }

        [RelayCommand]
        public async Task OpenFileOrDirectory(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                await ShowTipMessage($"Error occurred while opening the file or directory: {ex.Message}");
            }
        }

        [RelayCommand]
        public void LoadDefaultFilesByIssueKey()
        {
            if (AIServiceSetting.DefaultAccessedDirectories.Count == 0)
            {
                //ShowMessage("Please add a default directory.");
                return;
            }

            if (string.IsNullOrEmpty(IssueKey))
            {
                //ShowMessage("Please enter a issue key.");
                return;
            }

            foreach (var folderPath in AIServiceSetting.DefaultAccessedDirectories)
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    continue;
                }
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => file.EndsWith(".patch") || file.EndsWith(".sql") || file.EndsWith(".txt"));
                foreach (var file in files)
                {
                    var fileType = Path.GetExtension(file).ToLowerInvariant() switch
                    {
                        ".patch" => FileType.Code,
                        ".sql" => FileType.Sql,
                        _ => FileType.Doc,
                    };
                    var fileName = Path.GetFileName(file);
                    var filePath = Path.GetFullPath(file);
                    if (IssueFiles.Any(x => x.Path == filePath))
                    {
                        continue;
                    }
                    IssueFiles.Add(new IssueFile
                    {
                        Id = IssueFiles.Count + 1,
                        Name = fileName,
                        Path = filePath,
                        Type = fileType
                    });
                }
            }
        }

        [RelayCommand]
        public async Task QueryChatHistories()
        {
            if (StartDate > EndDate)
            {
                await ShowTipMessage("Start date should be less than end date.");
                return;
            }
            var chatHistories = repository.Find<ChatRecord>(r => r.LatestChatTime > StartDate && r.LatestChatTime < EndDate);
            if (chatHistories != null)
            {
                ChatHistories = [.. chatHistories];
            }
            else
            {
                await ShowTipMessage("No chat history found.");
            }
        }

        [RelayCommand]
        public async Task SendQuestion()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                await ShowTipMessage("Please enter a question.");
                return;
            }

            if (_chatContext == null || _chatContext.ChatHistory.Count <= 1)
            {
                _chatContext = await CreateChatContext();
                if (_chatContext == null)
                {
                    return;
                }
            }

            _chatContext.ChatHistory.AddUserMessage(InputText);
            await AskQuestion();
        }

        [RelayCommand]
        public void SaveAIServiceSetting()
        {
            if (!string.IsNullOrEmpty(AIServiceSetting.SelectedModel)
                && !AIServiceSetting.Models.Contains(AIServiceSetting.SelectedModel))
            {
                AIServiceSetting.Models.Add(AIServiceSetting.SelectedModel);
            }

            settingService.UpsertSetting(AIServiceSetting);
        }

        [RelayCommand]
        public async Task LoadHistoryChat(ChatRecord record)
        {
            _chatContext = await CreateChatContext();
            if (_chatContext == null)
            {
                return;
            }

            _chatContext.ChatHistory = JsonSerializer.Deserialize<ChatHistory>(record.ChatHistoryJson) ?? [];
            _chatContext.Id = record.Id;

            IssueFiles.Clear();
            foreach (var filePath in record.FilePaths)
            {
                var fileType = Path.GetExtension(filePath).ToLowerInvariant() switch
                {
                    ".patch" => FileType.Code,
                    ".sql" => FileType.Sql,
                    _ => FileType.Doc,
                };
                var fileName = Path.GetFileName(filePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }
                IssueFiles.Add(new IssueFile
                {
                    Id = IssueFiles.Count + 1,
                    Name = fileName,
                    Path = filePath,
                    Type = fileType
                });
            }

            RefreshChatFlowDocument();
        }

        [RelayCommand]
        public void ClearCurrentChat()
        {
            ChatFlowDocument = new();
            _chatContext = null;
            IssueFiles.Clear();
        }

        #endregion

        #region Help Function

        private async Task<object?> ShowTipMessage(string message)
        {
            return await DialogHost.Show(GenerateControl.GetConfrimDialog(message), "ConfirmDialog");
        }

        private void RefreshChatFlowDocument()
        {
            if (_chatContext == null)
            {
                return;
            }

            StringBuilder chatBuilder = new();
            foreach (var chatContent in _chatContext.ChatHistory)
            {
                chatBuilder.AppendLine($"###{chatContent.Role.ToString()} \n {chatContent.Content}");
            }
            ChatFlowDocument = _markDownEngine.Transform(chatBuilder.ToString());
        }

        private void RecordChatHistory()
        {
            if (_chatContext == null)
            {
                return;
            }

            var chatRecord = new ChatRecord
            {
                Id = _chatContext.Id,
                ChatHistoryJson = JsonSerializer.Serialize(_chatContext.ChatHistory),
                LatestChatTime = DateTime.Now,
                ModelName = _chatContext.ChatCompletionService.GetModelId() ?? "UnKnowModel",
                FilePaths = IssueFiles.Select(x => x.Path),
                StartText = InputText?[..Math.Min(32, InputText.Length - 1)] ?? string.Empty
            };
            repository.Upsert(chatRecord);
        }

        private async Task<ChatContext?> CreateChatContext()
        {
            if (AIServiceSetting == null
                || string.IsNullOrEmpty(AIServiceSetting.ServiceAddress)
                || string.IsNullOrEmpty(AIServiceSetting.SelectedModel)
                || string.IsNullOrEmpty(AIServiceSetting.ApiKey))
            {
                await ShowTipMessage("Please enter a necessary info.");
                return null;
            }

            var kernel = semanticKernelService.BuildKernelWithChatCompletion(
                            baseAddress: AIServiceSetting.ServiceAddress,
                            modelId: AIServiceSetting.SelectedModel,
                            apiKey: AIServiceSetting.ApiKey,
                            replaceToEmptyStrings: ["\"strict\":false,"]
                        );

            var openAIPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(
                    options: new FunctionChoiceBehaviorOptions
                    {
                        AllowStrictSchemaAdherence = false
                    }),
            };
            if (!string.IsNullOrEmpty(AIServiceSetting.PromptText))
            {
                openAIPromptExecutionSettings.ChatSystemPrompt = AIServiceSetting.PromptText;
            }

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

            var chatContext = new ChatContext
            {
                Id = ObjectId.NewObjectId(),
                Kernel = kernel,
                ChatCompletionService = chatCompletionService,
                ChatHistory = [],
                PromptExecutionSettings = openAIPromptExecutionSettings

            };

            return chatContext;
        }

        public async Task AskQuestion()
        {
            if (_chatContext == null)
            {
                return;
            }

            try
            {
                IsWaitResponse = true;
                if (this.AIServiceSetting.EnableStreamResponse)
                {
                    var response = _chatContext.ChatCompletionService.GetStreamingChatMessageContentsAsync(
                        chatHistory: _chatContext.ChatHistory,
                        executionSettings: _chatContext.PromptExecutionSettings,
                        kernel: _chatContext.Kernel
                     );

                    DateTime startTime = DateTime.Now;
                    await foreach (var chunk in response)
                    {
                        if (DateTime.Now - startTime > TimeSpan.FromSeconds(RefreshIntervalSecond))
                        {
                            RefreshChatFlowDocument();
                            startTime = DateTime.Now;
                        }
                    }

                    RecordChatHistory();
                    InputText = string.Empty;
                }
                else
                {
                    var response = await _chatContext.ChatCompletionService.GetChatMessageContentAsync(
                        chatHistory: _chatContext.ChatHistory,
                        executionSettings: _chatContext.PromptExecutionSettings,
                        kernel: _chatContext.Kernel
                     );

                    if (_chatContext == null)
                    {
                        throw new Exception("chat already be clear.");
                    }

                    _chatContext.ChatHistory.AddAssistantMessage(response.Content ?? string.Empty);

                    RefreshChatFlowDocument();
                    RecordChatHistory();
                    InputText = string.Empty;
                }
            }
            catch (Exception ex)
            {
                await ShowTipMessage($"Error occurred while processing the request\n{ex.Message}");
            }
            finally
            {
                IsWaitResponse = false;
            }

        }

        #endregion
    }

    public record ChatContext
    {
        public required ObjectId Id { get; set; }
        public required Kernel Kernel { get; set; }
        public required IChatCompletionService ChatCompletionService { get; set; }
        public required ChatHistory ChatHistory { get; set; }
        public required OpenAIPromptExecutionSettings PromptExecutionSettings { get; set; }
    }
}
