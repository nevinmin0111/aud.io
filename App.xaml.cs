using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace SoundDesk;

public partial class App : System.Windows.Application
{
    private Forms.NotifyIcon? _trayIcon;
    private MainWindow? _window;
    private bool _reallyExit;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        _window = new MainWindow();
        _window.Closing += (_, args) =>
        {
            if (_reallyExit) return;
            args.Cancel = true;
            _window.Hide();
            _trayIcon?.ShowBalloonTip(1200, "aud.io", "Still running beside the clock.", Forms.ToolTipIcon.Info);
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open mixer", null, (_, _) => ShowMixer());
        menu.Items.Add("Exit", null, (_, _) => ExitApp());

        _trayIcon = new Forms.NotifyIcon
        {
            Text = "aud.io audio mixer",
            Icon = SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.MouseClick += (_, args) =>
        {
            if (args.Button == Forms.MouseButtons.Left) ShowMixer();
        };

        ShowMixer();
    }

    private void ShowMixer()
    {
        if (_window is null) return;
        _window.Show();
        if (_window.WindowState == WindowState.Minimized) _window.WindowState = WindowState.Normal;
        _window.Activate();
        _window.Topmost = true;
        _window.Topmost = false;
        _window.Focus();
    }

    private void ExitApp()
    {
        _reallyExit = true;
        _trayIcon?.Dispose();
        _window?.Close();
        Shutdown();
    }
}
