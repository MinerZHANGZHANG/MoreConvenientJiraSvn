using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MoreConvenientJiraSvn.App.ViewModels
{
    public partial class IssueAIAnalysisViewModel : ObservableObject, INotifyPropertyChanged
    {
        [ObservableProperty]
        private string _promptText;

        [ObservableProperty]
        private string _selectedModel;

        public ObservableCollection<string> Models { get; } = new ObservableCollection<string>
        {
            "Model 1",
            "Model 2",
            "Model 3"
        };

        // New properties for multi-file selection in code问答.
        public ObservableCollection<string> CodeFiles { get; } = new ObservableCollection<string>();

        // New properties for multi-file selection in Sql问答.
        public ObservableCollection<string> SqlFiles { get; } = new ObservableCollection<string>();

        // New properties for folder file selection.
        [ObservableProperty]
        private string _selectedFolderPath;

        public ObservableCollection<string> FolderFiles { get; } = new ObservableCollection<string>();

        [ObservableProperty]
        private string _selectedFolderFile;
    }
}
