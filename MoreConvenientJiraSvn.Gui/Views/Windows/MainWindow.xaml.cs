using MoreConvenientJiraSvn.App.ViewModels;
using System.Windows;

namespace MoreConvenientJiraSvn.App
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