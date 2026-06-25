using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;

namespace Kvanto.Services;

/// <summary>
/// Sends Windows toast notifications for Pomodoro session events.
/// Uses the Windows App SDK AppNotifications API (requires app identity / MSIX packaging).
/// </summary>
public class NotificationService
{
    private bool _registered;

    public NotificationService()
    {
        try
        {
            AppNotificationManager.Default.NotificationInvoked += OnNotificationInvoked;
            AppNotificationManager.Default.Register();
            _registered = true;
        }
        catch
        {
            // Notification registration may fail if app lacks identity (e.g., unpackaged debug).
            _registered = false;
        }
    }

    /// <summary>Show a toast when a Pomodoro work session ends.</summary>
    public void SendPomodoroEndNotification(string taskTitle, int completedCount, bool isWork)
    {
        if (!_registered) return;

        var builder = new AppNotificationBuilder();

        if (isWork)
        {
            builder
                .AddText("🍅 Pomodoro Complete!", new AppNotificationTextProperties().SetMaxLines(1))
                .AddText($"Great work on \"{taskTitle}\"! Session #{completedCount} done.")
                .AddText("Time for a break – you've earned it.");

            builder.AddButton(new AppNotificationButton("Take Short Break")
                .AddArgument("action", "shortBreak"));
            builder.AddButton(new AppNotificationButton("Take Long Break")
                .AddArgument("action", "longBreak"));
        }
        else
        {
            builder
                .AddText("⏰ Break Over!", new AppNotificationTextProperties().SetMaxLines(1))
                .AddText("Ready to get back to work?")
                .AddButton(new AppNotificationButton("Start Next Pomodoro")
                    .AddArgument("action", "startWork")
                    .AddArgument("taskId", taskTitle));
        }

        var notification = builder.BuildNotification();
        AppNotificationManager.Default.Show(notification);
    }

    /// <summary>Show a generic informational toast.</summary>
    public void ShowInfoNotification(string title, string body)
    {
        if (!_registered) return;
        var notification = new AppNotificationBuilder()
            .AddText(title)
            .AddText(body)
            .BuildNotification();
        AppNotificationManager.Default.Show(notification);
    }

    private void OnNotificationInvoked(
        AppNotificationManager sender,
        AppNotificationActivatedEventArgs args)
    {
        var action = args.Arguments.TryGetValue("action", out var a) ? a : string.Empty;

        App.MainWindow?.DispatcherQueue.TryEnqueue(async () =>
        {
            App.MainWindow.BringToFront();

            if (App.PomodoroService == null) return;

            switch (action)
            {
                case "shortBreak":
                    await App.PomodoroService.StartBreakAsync(longBreak: false);
                    break;
                case "longBreak":
                    await App.PomodoroService.StartBreakAsync(longBreak: true);
                    break;
                case "startWork":
                    // Navigate to tasks page; user selects a task from there
                    break;
            }
        });
    }

    public void Unregister()
    {
        if (_registered)
            AppNotificationManager.Default.Unregister();
    }
}
