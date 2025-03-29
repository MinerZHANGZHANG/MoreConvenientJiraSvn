using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.App.Properties;
using MoreConvenientJiraSvn.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class JiraIssueBrowseViewModel
{
    [ObservableProperty]
    private ObservableCollection<JiraTransition> _transitions = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitTransitionFormCommand))]
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
        JiraFields.Clear();
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

    [RelayCommand(CanExecute = nameof(HasTransitionBeSelected))]
    public async Task SubmitTransitionForm()
    {
        if (SelectedJiraIssue == null || SelectedTransition == null)
        {
            return;
        }

        if (!Settings.Default.IsEnableWriteOperation)
        {
            MessageBox.Show($"未启用Jira提交功能，若需要启用写操作请在【首页】-【应用设置】中打开开关.");
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

        JiraFields.Clear();
        SelectedTransition = null;
    }
}
