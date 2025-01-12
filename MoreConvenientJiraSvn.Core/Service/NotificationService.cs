using System.Windows.Forms;

namespace MoreConvenientJiraSvn.Core.Service;
public class NotificationService
{
    private readonly NotifyIcon _notifyIcon;

    public NotificationService(string iconUrl)
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = new(iconUrl, 40, 40),
            Visible = true
        };
        _notifyIcon.Click += NotifyIcon_Click;
    }

    private void NotifyIcon_Click(object? sender, EventArgs e)
    {
        this.ShowNotification("TODO", "或许应该在这个小按钮上加点功能...");
    }

    public void ShowNotification(string title, string message, ToolTipIcon toolTipIcon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, message, toolTipIcon);
    }

    public void DebugMessage(string message)
    {
        _notifyIcon.ShowBalloonTip(3000, "Debug", message, ToolTipIcon.None);
    }

    public void Dispose()
    {
        _notifyIcon?.Dispose();
    }
}