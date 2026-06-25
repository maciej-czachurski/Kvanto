using H.NotifyIcon;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace Kvanto.Services;

/// <summary>
/// Manages the system-tray icon using H.NotifyIcon.WinUI.
/// Provides a context menu and balloon tip for Pomodoro state feedback.
/// </summary>
public class TrayIconService : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private readonly MainWindow _mainWindow;
    private readonly PomodoroService _pomodoroService;

    public TrayIconService(MainWindow mainWindow, PomodoroService pomodoroService)
    {
        _mainWindow = mainWindow;
        _pomodoroService = pomodoroService;
    }

    public Task InitializeAsync()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Kvanto – Task Tracker"
        };

        // Context menu
        var menu = new MenuFlyout();

        var openItem = new MenuFlyoutItem { Text = "Open Kvanto" };
        openItem.Click += (_, _) => _mainWindow.BringToFront();

        var startPomodoroItem = new MenuFlyoutItem { Text = "▶  Start Pomodoro" };
        startPomodoroItem.Click += (_, _) =>
        {
            _mainWindow.BringToFront();
            // Navigate to tasks page and start the first active task
        };

        var pauseItem = new MenuFlyoutItem { Text = "⏸  Pause" };
        pauseItem.Click += (_, _) => _pomodoroService.TogglePause();

        var stopItem = new MenuFlyoutItem { Text = "⏹  Stop" };
        stopItem.Click += async (_, _) => await _pomodoroService.StopAsync();

        var compactItem = new MenuFlyoutItem { Text = "📌  Compact View" };
        compactItem.Click += (_, _) => _mainWindow.DispatcherQueue.TryEnqueue(
            () => _mainWindow.OpenCompactOverlay());

        var separator = new MenuFlyoutSeparator();

        var exitItem = new MenuFlyoutItem { Text = "Exit" };
        exitItem.Click += (_, _) => Application.Current.Exit();

        menu.Items.Add(openItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(startPomodoroItem);
        menu.Items.Add(pauseItem);
        menu.Items.Add(stopItem);
        menu.Items.Add(separator);
        menu.Items.Add(compactItem);
        menu.Items.Add(new MenuFlyoutSeparator());
        menu.Items.Add(exitItem);

        _trayIcon.ContextMenuMode = ContextMenuMode.PopupMenu;
        _trayIcon.ContextFlyout = menu;

        // Double-click restores the window
        _trayIcon.TrayMouseDoubleClick += (_, _) => _mainWindow.BringToFront();

        // Update tray tooltip when Pomodoro state changes
        _pomodoroService.TimerTick += OnTimerTick;
        _pomodoroService.SessionStateChanged += OnStateChanged;

        return Task.CompletedTask;
    }

    private void OnTimerTick(object? sender, PomodoroTimerEventArgs e)
    {
        if (_trayIcon == null) return;
        var label = e.IsBreak ? "Break" : "Work";
        _trayIcon.ToolTipText =
            $"Kvanto – {label}: {e.TimeRemaining:mm\\:ss} remaining";
    }

    private void OnStateChanged(object? sender, PomodoroStateEventArgs e)
    {
        if (_trayIcon == null) return;

        _trayIcon.ToolTipText = e.State == PomodoroState.Idle
            ? "Kvanto – Task Tracker"
            : $"Kvanto – {e.State}: {e.TaskTitle}";
    }

    /// <summary>Show a balloon tip / tooltip message from the tray icon.</summary>
    public void ShowBalloonTip(string message)
    {
        App.NotificationService?.ShowInfoNotification("Kvanto", message);
    }

    public void Dispose()
    {
        _pomodoroService.TimerTick -= OnTimerTick;
        _pomodoroService.SessionStateChanged -= OnStateChanged;
        _trayIcon?.Dispose();
    }
}
