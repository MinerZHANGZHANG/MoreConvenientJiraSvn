using MoreConvenientJiraSvn.Core.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Views.Controls
{
    /// <summary>
    /// ListBoxInput.xaml 的交互逻辑
    /// </summary>
    public partial class ListBoxInput : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable<SelectItem>), typeof(ListBoxInput));

        public static readonly DependencyProperty InputTextProperty =
            DependencyProperty.Register(nameof(InputText), typeof(string), typeof(ListBoxInput),
                new PropertyMetadata(string.Empty));

        public IEnumerable<SelectItem> ItemsSource
        {
            get => (IEnumerable<SelectItem>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public string InputText
        {
            get => (string)GetValue(InputTextProperty);
            set => SetValue(InputTextProperty, value);
        }

        public IEnumerable<SelectItem> SelectedItems
        {
            get { return ItemsSource.Where(i => i.IsSelected); }
        }

        public IEnumerable<string> SuggestionItems
        {
            get { return ItemsSource.Select(i => i.Name); }
        }

        public ListBoxInput()
        {
            InitializeComponent();

            Loaded += ListBoxInput_Loaded;
        }

        private void ListBoxInput_Loaded(object sender, RoutedEventArgs e)
        {
            SelectedItemsList.ItemsSource = SelectedItems;
            InputTextBox.Suggestions = SuggestionItems;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InputTextBox.Text))
            {
                return;
            }

            var item = ItemsSource.FirstOrDefault(i => i.Name == InputTextBox.Text);
            if (item != null)
            {
                item.IsSelected = true;
                SelectedItemsList.ItemsSource = SelectedItems;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is SelectItem item)
            {
                item.IsSelected = false;
                SelectedItemsList.ItemsSource = SelectedItems;
            }
        }
    }
}
