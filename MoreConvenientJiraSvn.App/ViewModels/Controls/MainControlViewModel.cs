using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Enums;
using MoreConvenientJiraSvn.Core.Interfaces;
using MoreConvenientJiraSvn.Core.Models;
using System.Collections.ObjectModel;

namespace MoreConvenientJiraSvn.App.ViewModels;

public partial class MainControlViewModel : ObservableObject
{
    private readonly IRepository _repository;

    [ObservableProperty]
    private ObservableCollection<BackgroundTaskLog> _backgroundTaskLogs = [];
    [ObservableProperty]
    private BackgroundTaskLog? _selectedBackgroundTaskLog;

    [ObservableProperty]
    private ObservableCollection<BackgroundTaskMessage> _backgroundTaskMessages = [];

    public MainControlViewModel(IRepository repository)
    {
        _repository = repository;

        RefreshMessage();
    }

    [RelayCommand]
    public void OpenWindow(WindowType windowType)
    {
        WindowsManager.OpenOrFocusWindow(windowType);
    }

    [RelayCommand]
    public void RefreshMessage()
    {
        DateTime lastQueryTime = DateTime.Now.AddDays(-7);
        BackgroundTaskLogs = [.. _repository.Find<BackgroundTaskLog>(l => l.StartTime >= lastQueryTime).OrderByDescending(l => l.StartTime)];

        foreach (var log in BackgroundTaskLogs)
        {
            log.BackgroundTaskMessages = [.. _repository.Find<BackgroundTaskMessage>(m => m.LogId == log.Id)];
        }
    }

    [RelayCommand]
    public void ShowTaskLogMessages()
    {

    }
}

