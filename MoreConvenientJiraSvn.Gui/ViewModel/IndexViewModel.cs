using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MoreConvenientJiraSvn.Gui.View.Controls;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace MoreConvenientJiraSvn.Gui.ViewModel
{
    public partial class IndexViewModel : ObservableObject
    {

        public readonly MainControl mainControl = new();

        public readonly GetPluginControl getPluginControl = new();

        [ObservableProperty]
        private System.Windows.Controls.UserControl? _currentContent = null;

        [RelayCommand]
        public void SwitchContent(IndexContent indexContent)
        {
            switch (indexContent)
            {
                case IndexContent.Index:
                    CurrentContent = mainControl;
                    break;
                case IndexContent.Plugin:
                    CurrentContent = getPluginControl;
                    break;
                case IndexContent.Setting:
                case IndexContent.What:
                case IndexContent.How:
                case IndexContent.Expand:
                    MessageBox.Show("尚未支持");
                    break;
                default:
                    break;
            }
        }
    }

    public enum IndexContent
    {
        Index,
        Setting,
        Plugin,
        What,
        How,
        Expand
    }
}
