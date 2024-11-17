using MoreConvenientJiraSvn.Gui.ViewModel;
using System.Windows;

namespace MoreConvenientJiraSvn.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = ViewModelsManager.GetViewModel<MainWindowViewModel>();
            InitializeComponent();
        }
    }
}