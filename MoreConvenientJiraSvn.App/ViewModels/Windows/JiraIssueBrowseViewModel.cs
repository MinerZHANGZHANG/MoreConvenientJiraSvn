using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using MoreConvenientJiraSvn.Service;
using System.Collections.ObjectModel;
using System.Windows;


namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel(JiraService jiraService, SvnService svnService, IRepository repository, SettingService settingService) : ObservableObject
{
    #region Service

    private readonly JiraService _jiraService = jiraService;
    private readonly SvnService _svnService = svnService;
    private readonly IRepository _repository = repository;
    private readonly SettingService _settingService = settingService;

    #endregion

    #region Field & Property

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUseTextToSearch))]
    private JiraIssueQueryType _selectedJiraIssueQueryType = JiraIssueQueryType.JiraId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(QueryJiraInfoCommand))]
    [NotifyPropertyChangedFor(nameof(HaveQueryText))]
    private string? _jiraIssueQueryText;

    [ObservableProperty]
    private JiraIssueFilter? _selectedJiraIssueFilter;
    [ObservableProperty]
    private List<JiraIssueFilter>? _jiraIssueFilters;

    [ObservableProperty]
    private ObservableCollection<JiraIssue>? _jiraIssueList;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasIssueBeSelected))]
    [NotifyPropertyChangedFor(nameof(HasTestCases))]
    [NotifyCanExecuteChangedFor(nameof(OpenOrCreateJiraIssueDirectoryCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyCommitTextCommand))]
    [NotifyCanExecuteChangedFor(nameof(CopyAnnotationTextCommand))]
    [NotifyCanExecuteChangedFor(nameof(OpenWebPageCommand))]
    [NotifyCanExecuteChangedFor(nameof(OnlyDisplayCurrentJiraIssuesCommand))]
    private JiraIssue? _selectedJiraIssue;

    public IReadOnlyList<JiraIssueQueryType> JiraIssueQueryTypes { get; } = Enum.GetValues<JiraIssueQueryType>();
    public bool IsUseTextToSearch => SelectedJiraIssueQueryType != JiraIssueQueryType.Filter;
    public bool HaveQueryText => !string.IsNullOrEmpty(JiraIssueQueryText);
    public bool HasIssueBeSelected => SelectedJiraIssue != null;

    private event EventHandler<JiraIssue>? _selectedIssueChanged;
    public SnackbarMessageQueue MessageQueue { get; } = new(TimeSpan.FromSeconds(2d));

    #endregion

    public async Task InitViewModelAsync()
    {
        JiraIssueFilters = await _jiraService.GetCurrentUserFavouriteFilterAsync();

        InitJiraIssueTransitionOperation();
        InitJiraIssueDetailDisplay();
        InitJiraIssueLocalOperations();
        InitJiraIssueSvnLogLink();
    }

    #region Command

    [RelayCommand]
    public async Task QueryJiraInfo()
    {
        // Todo: make page
        switch (SelectedJiraIssueQueryType)
        {
            case JiraIssueQueryType.JiraId:
                if (string.IsNullOrEmpty(JiraIssueQueryText))
                {
                    break;
                }
                var jiraIssue = await _jiraService.GetIssueAsync(JiraIssueQueryText);
                if (jiraIssue == null)
                {
                    break;
                }
                RefreshCurrentJiraIssues([jiraIssue]);
                break;
            case JiraIssueQueryType.Jql:
                if (string.IsNullOrEmpty(JiraIssueQueryText))
                {
                    break;
                }
                RefreshCurrentJiraIssues(await _jiraService.GetIssuesByJqlAsync(JiraIssueQueryText));
                break;
            case JiraIssueQueryType.Filter:
                if (SelectedJiraIssueFilter == null)
                {
                    break;
                }
                RefreshCurrentJiraIssues(await _jiraService.GetIssuesByFilterAsync(SelectedJiraIssueFilter));
                break;
            default:
                break;
        }
    }

    public void RefreshCurrentJiraIssues(IEnumerable<JiraIssue> issues)
    {
        JiraIssueList = [.. issues];
    }

    public void InvokeSelectedJiraIssueEvent(object? sender)
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }
        _selectedIssueChanged?.Invoke(sender, SelectedJiraIssue);
    }

    private void ShowMessageSnack(string message)
    {
        MessageQueue.Enqueue(message, "关闭", () => MessageQueue.Clear(), true);
    }

    #endregion
}






