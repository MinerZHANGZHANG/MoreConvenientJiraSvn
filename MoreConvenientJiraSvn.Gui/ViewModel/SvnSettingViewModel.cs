using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Core.Model;
using MoreConvenientJiraSvn.Core.Service;
using System.Collections.ObjectModel;
using System.Windows;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
    public partial class SvnSettingViewModel : ObservableObject
    {
        private readonly SvnService _svnService;

        [ObservableProperty]
        private SvnConfig? _config;

        [ObservableProperty]
        private ObservableCollection<SvnPath> _paths;


        public SvnSettingViewModel(SvnService svnService)
        {
            _svnService = svnService;
            this.Config = _svnService.Config ?? new();
            this.Paths = new(_svnService.Paths);
        }

        [RelayCommand]
        public void UpdateConfig()
        {
            if (Config == null)
            {
                return;
            }
            _svnService.UpdateConfig(Config);
            OnPropertyChanged(nameof(Config));
        }


        [RelayCommand]
        public void UpdatePath(object parameter)
        {
            if (parameter is SvnPath path)
            {
                if (Paths.Any(p => p.Path == path.Path && p.Id != path.Id))
                {
                    MessageBox.Show($"已经有一个svn地址为{path.Path}了!");
                    return;
                }

                _svnService.InsertOrUpdateSinglePath(path);
                MessageBox.Show($"新增{path.Path}成功!");
            }

        }

        [RelayCommand]
        public void DeletePath(object parameter)
        {
            if (parameter is SvnPath path)
            {
                _svnService.DeleteSinglePath(path.Path);
                this.Paths.Remove(path);
                MessageBox.Show($"删除{path.Path}成功!");
            }
        }

    }
}
