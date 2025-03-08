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
    private JiraOperation? _selectedOperation;

    [ObservableProperty]
    private ObservableCollection<JiraField> _fieldModels = [];

    private void InitJiraIssueTransitionOperation()
    {

    }

    partial void OnSelectedTransitionChanged(JiraTransition? value)
    {
        HandleTransitionChange(value);
    }

    private void HandleTransitionChange(JiraTransition? transition)
    {
        if (transition == null)
        {
            return;
        }

        //SelectedOperation = _jiraService.Operations.FirstOrDefault(
        //    o => o.OperationId == transition.TransitionId
        //    && o.OperationName == transition.TransitionName);

        //FieldModels = SelectedOperation?.Fields ?? [];
    }
}
