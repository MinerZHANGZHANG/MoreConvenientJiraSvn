using System.Windows;
using System.Windows.Controls;

namespace MoreConvenientJiraSvn.App.Utils;

internal static class GenerateControl
{
    public static StackPanel GetConfrimDialog(string message)
    {
        var mainStackPanel = new StackPanel { Margin = new Thickness(16) };

        var textBlock = new TextBlock { Text = message };
        mainStackPanel.Children.Add(textBlock);

        var buttonPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            Orientation = Orientation.Horizontal
        };

        var confirmButton = new Button
        {
            Margin = new Thickness(0, 8, 8, 0),
            Content = "确认",
            IsDefault = true,
            Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
            Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand,
            CommandParameter = true
        };

        var cancelButton = new Button
        {
            Margin = new Thickness(0, 8, 8, 0),
            Content = "取消",
            IsCancel = true,
            Style = (Style)Application.Current.Resources["MaterialDesignFlatButton"],
            Command = MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand,
            CommandParameter = false
        };

        buttonPanel.Children.Add(confirmButton);
        buttonPanel.Children.Add(cancelButton);
        mainStackPanel.Children.Add(buttonPanel);

        return mainStackPanel;
    }
}
