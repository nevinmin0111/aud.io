using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using SoundDesk.Models;
using SoundDesk.Services;

namespace SoundDesk;

public partial class MainWindow : Window
{
    private readonly AudioService _audio = new();
    private readonly DispatcherTimer _refreshTimer;
    private bool _refreshQueued;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _audio;
        _audio.AudioChanged += Audio_AudioChanged;

        _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _refreshTimer.Tick += (_, _) => SafeRefresh();
        _refreshTimer.Start();
        UpdateCounts();
        Closed += (_, _) => _audio.Dispose();
    }

    private void Audio_AudioChanged(object? sender, EventArgs e)
    {
        if (_refreshQueued) return;
        _refreshQueued = true;
        Dispatcher.BeginInvoke(() =>
        {
            _refreshQueued = false;
            SafeRefresh();
        }, DispatcherPriority.Background);
    }

    private void SafeRefresh()
    {
        try { _audio.Refresh(); }
        catch { /* A device can vanish in the middle of enumeration; the next tick retries. */ }
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        DeviceCount.Text = $"{_audio.Devices.Count} connected";
        SessionCount.Text = $"{_audio.Sessions.Count} detected";
        NoAppsMessage.Visibility = _audio.Sessions.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => SafeRefresh();

    private void MakeDefault_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: OutputDevice device })
        {
            try { _audio.MakeDefault(device); }
            catch (Exception ex) { MessageBox.Show($"Windows could not switch that output.\n\n{ex.Message}", "aud.io"); }
        }
    }

    private void ThemePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized || ThemePicker.SelectedItem is not ComboBoxItem item) return;
        var theme = item.Content?.ToString();
        switch (theme)
        {
            case "DJ Neon": ApplyColors("#090512", "#160B25", "#211438", "#F8F2FF", "#A997BE", "#FF3DED", "#43245F"); break;
            case "Compact": ApplyColors("#F2F4F7", "#FFFFFF", "#FFFFFF", "#18202D", "#657086", "#3B73F1", "#D8DDE7"); break;
            default: ApplyColors("#10131A", "#191E29", "#222938", "#F5F7FF", "#96A0B5", "#45E0B7", "#323B50"); break;
        }
    }

    private void ApplyColors(params string[] colors)
    {
        var keys = new[] { "WindowBrush", "PanelBrush", "CardBrush", "TextBrush", "SubtleBrush", "AccentBrush", "BorderBrush" };
        for (var i = 0; i < keys.Length; i++) Resources[keys[i]] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));
    }
}
