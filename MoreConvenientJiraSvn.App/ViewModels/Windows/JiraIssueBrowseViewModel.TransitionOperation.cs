using CommunityToolkit.Mvvm.ComponentModel;
using MoreConvenientJiraSvn.Core.Models;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel
{
    [ObservableProperty]
    private ObservableCollection<JiraTransition> _transitions = [];

    [ObservableProperty]
    private JiraTransition? _selectedTransition;

    [ObservableProperty]
    private ObservableCollection<JiraField> _jiraFields = [];

    private CancellationTokenSource? _transitionCancellationTokenSource;

    partial void OnSelectedTransitionChanged(JiraTransition? value)
    {
       HandleTransitionChange(value);
    }

    private void InitJiraIssueTransitionOperation()
    {
        _selectedIssueChanged += JiraIssueTransitionOperation_selectedIssueChanged;
    }

    private async void JiraIssueTransitionOperation_selectedIssueChanged(object? sender, JiraIssue e)
    {
        if (SelectedJiraIssue == null)
        {
            return;
        }
        _transitionCancellationTokenSource?.Cancel();

        Transitions = [.. await _jiraService.GetTransitionsByIssueId(SelectedJiraIssue.IssueId)];
    }


    private async void HandleTransitionChange(JiraTransition? _)
    {
        if (SelectedTransition == null || SelectedJiraIssue == null)
        {
            return;
        }
        _transitionCancellationTokenSource?.Cancel();
        _transitionCancellationTokenSource = new();

        JiraFields = [.. await _jiraService.GetFieldInfoFromTransitionAndIssueId(SelectedJiraIssue.IssueId, SelectedTransition, _transitionCancellationTokenSource.Token)];

    }
}
