using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel
{
    [ObservableProperty]
    private List<JiraIssue> _childJiraIssues = [];

    private void InitJiraIssueDetailDisplay()
    {
        _selectedIssueChanged += JiraIssueBrowseViewModel_selectedIssueChanged;
    }

    private async void JiraIssueBrowseViewModel_selectedIssueChanged(object? sender, JiraIssue e)
    {
        if (string.IsNullOrEmpty(SelectedJiraIssue?.ChildrenIssuesJql))
        {
            return;
        }

        ChildJiraIssues = await _jiraService.GetIssuesByJqlAsync(SelectedJiraIssue.ChildrenIssuesJql);
    }

    [RelayCommand(CanExecute = nameof(HasIssueBeSelected))]
    public void OnlyDisplayCurrentJiraIssues()
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }

        List<JiraIssue> childWithParentJiraIssues = [.. ChildJiraIssues];
        childWithParentJiraIssues.Add(SelectedJiraIssue);

        RefreshCurrentJiraIssues(childWithParentJiraIssues);
    }

    [RelayCommand(CanExecute = nameof(HasIssueBeSelected))]
    public void OpenWebPage(string issueKey)
    {
        if (string.IsNullOrEmpty(issueKey))
        {
            return;
        }

        string? url = $"{_jiraService.JiraConfig.BaseUrl}browse/{issueKey}";
        if (string.IsNullOrEmpty(url))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageQueue.Enqueue($"无法打开网页: {ex.Message}");
        }
    }
}
