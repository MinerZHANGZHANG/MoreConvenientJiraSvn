using MoreConvenientJiraSvn.App.Views.Windows;
using MoreConvenientJiraSvn.Core.Enums;
using System.Windows;

namespace MoreConvenientJiraSvn.App
{
    public static class WindowsManager
    {
        public static List<Window> Windows => _windows;
        private static List<Window> _windows = [];

        public static void OpenOrFocusWindow(WindowType windowType)
        {
            switch (windowType)
            {
                case WindowType.Main:
                    var mainWindow = _windows.Find(w => w is MainWindow);
                    if (mainWindow != null)
                    {
                        mainWindow.Focus();
                    }
                    else
                    {
                        mainWindow = new MainWindow();
                        mainWindow.Show();

                        _windows.Add(mainWindow);
                    }
                    break;
                case WindowType.Jira:
                    var jiraWindow = _windows.Find(w => w is Jira2LocalDirWindow);
                    if (jiraWindow != null)
                    {
                        jiraWindow.Focus();
                    }
                    else
                    {
                        jiraWindow = new Jira2LocalDirWindow();
                        jiraWindow.Show();

                        _windows.Add(jiraWindow);
                    }
                    break;
                case WindowType.Svn:
                    var svnWindow = _windows.Find(w => w is SvnJiraLinkWindow);
                    if (svnWindow != null)
                    {
                        svnWindow.Focus();
                    }
                    else
                    {
                        svnWindow = new SvnJiraLinkWindow();
                        svnWindow.Show();

                        _windows.Add(svnWindow);
                    }
                    break;
                case WindowType.Sql:
                    var sqlWindow = _windows.Find(w => w is SqlCheckWindow);
                    if (sqlWindow != null)
                    {
                        sqlWindow.Focus();
                    }
                    else
                    {
                        sqlWindow = new SqlCheckWindow();
                        sqlWindow.Show();

                        _windows.Add(sqlWindow);
                    }
                    break;
                default:
                    break;
            }
        }

        public static void RemoveWindow(Window window)
        {
            _windows.Remove(window);
        }
    }
}
