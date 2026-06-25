using Kvanto.Services;
using Kvanto.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;

namespace Kvanto.Views;

public sealed partial class SettingsPage : Page
{
    private readonly DatabaseService _db;
    private AppSettings? _settings;

    public SettingsPage()
    {
        _db = new DatabaseService(App.DbContext!);
        InitializeComponent();
        _ = LoadSettingsAsync();
    }

    private async System.Threading.Tasks.Task LoadSettingsAsync()
    {
        _settings = await _db.GetSettingsAsync();

        WorkDurationBox.Value = _settings.WorkDurationMinutes;
        ShortBreakBox.Value = _settings.ShortBreakMinutes;
        LongBreakBox.Value = _settings.LongBreakMinutes;
        LongBreakIntervalBox.Value = _settings.LongBreakInterval;
        NotificationsToggle.IsOn = _settings.ShowDesktopNotifications;
        SoundToggle.IsOn = _settings.PlaySoundOnEnd;
        StartWithWindowsToggle.IsOn = _settings.StartWithWindows;
        StartMinimizedToggle.IsOn = _settings.StartMinimizedToTray;
    }

    private async void OnSaveSettingsClick(object sender, RoutedEventArgs e)
    {
        if (_settings == null) return;

        _settings.WorkDurationMinutes = (int)WorkDurationBox.Value;
        _settings.ShortBreakMinutes = (int)ShortBreakBox.Value;
        _settings.LongBreakMinutes = (int)LongBreakBox.Value;
        _settings.LongBreakInterval = (int)LongBreakIntervalBox.Value;
        _settings.ShowDesktopNotifications = NotificationsToggle.IsOn;
        _settings.PlaySoundOnEnd = SoundToggle.IsOn;
        _settings.StartWithWindows = StartWithWindowsToggle.IsOn;
        _settings.StartMinimizedToTray = StartMinimizedToggle.IsOn;

        await _db.SaveSettingsAsync(_settings);

        // Apply to running Pomodoro service
        if (App.PomodoroService != null)
        {
            App.PomodoroService.WorkMinutes = _settings.WorkDurationMinutes;
            App.PomodoroService.ShortBreakMinutes = _settings.ShortBreakMinutes;
            App.PomodoroService.LongBreakMinutes = _settings.LongBreakMinutes;
            App.PomodoroService.LongBreakInterval = _settings.LongBreakInterval;
        }

        // Show confirmation
        var dialog = new ContentDialog
        {
            Title = "Settings Saved",
            Content = "Your settings have been saved successfully.",
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async void OnStartWithWindowsToggled(object sender, RoutedEventArgs e)
    {
        try
        {
            var startupTask = await StartupTask.GetAsync("KvantoStartup");
            if (StartWithWindowsToggle.IsOn)
                await startupTask.RequestEnableAsync();
            else
                startupTask.Disable();
        }
        catch
        {
            // StartupTask not available outside MSIX package
        }
    }
}
