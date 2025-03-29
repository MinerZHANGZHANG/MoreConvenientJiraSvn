using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private IReadOnlyCollection<JiraField> _originJiraFields = [];

    public bool HasTransitionBeSelected => SelectedTransition != null;

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

        var jiraFields = await _jiraService.GetFieldInfoFromTransitionAndIssueId(SelectedJiraIssue.IssueId, SelectedTransition, _transitionCancellationTokenSource.Token);
        JiraFields = [.. jiraFields.Item1];
        _originJiraFields = jiraFields.Item2;
    }

    // Todo: learn the changed event...
    [RelayCommand(CanExecute = nameof(HasTransitionBeSelected))]
    public async Task SubmitTransitionForm()
    {
        // Only changed field...
        if (SelectedJiraIssue == null || SelectedTransition == null)
        {
            return;
        }

        foreach (var oldField in _originJiraFields)
        {
            var newField = JiraFields.First(f => f.Id == oldField.Id);
            if (oldField.ValueIsEquals(newField))
            {
                JiraFields.Remove(newField);
            }
        }

        await _jiraService.TryPostTransitionsAsync(SelectedJiraIssue.IssueKey, SelectedTransition.TransitionId, JiraFields);
    }
}
