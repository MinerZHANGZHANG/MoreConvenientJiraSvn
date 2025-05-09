﻿using MoreConvenientJiraSvn.App.ViewModels;
using MoreConvenientJiraSvn.Core.Models;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Windows;

/// <summary>
/// Jira2LocalDirWindow.xaml 的交互逻辑
/// </summary>
public partial class Jira2LocalDirWindow : Window
{
    private readonly JiraIssueBrowseViewModel _viewModel;
    public Jira2LocalDirWindow()
    {
        InitializeComponent();
        this._viewModel = ViewModelsManager.GetViewModel<JiraIssueBrowseViewModel>();

        this.DataContext = _viewModel;
        this.Loaded += Jira2LocalDirWindow_Loaded;
        this.Closed += Jira2LocalDirWindow_Closed;
    }

    private void Jira2LocalDirWindow_Closed(object? sender, EventArgs e)
    {
        WindowsManager.RemoveWindow(this);
    }

    private async void Jira2LocalDirWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await this._viewModel.InitViewModelAsync();
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this._viewModel.InvokeSelectedJiraIssueEvent(sender);
    }

    private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        this._viewModel.LoadSvnLogByFromLocal();
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {

    }
}
