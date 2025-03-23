using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MoreConvenientJiraSvn.App.Views.Controls
{
    /// <summary>
    /// ListBoxInput.xaml 的交互逻辑
    /// </summary>
    public partial class ListBoxInput : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(ListBoxInput));

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<string>), typeof(ListBoxInput),
                new PropertyMetadata(new ObservableCollection<string>()));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ObservableCollection<string> SelectedItems
        {
            get => (ObservableCollection<string>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public ListBoxInput()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputTextBox.Text))
            {
                return;
            }
            SelectedItems.Add(InputTextBox.Text);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is string item)
            {
                SelectedItems.Remove(item);
            }
        }
    }
}
