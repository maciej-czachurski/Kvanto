using Kvanto.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Kvanto.Views;

public sealed partial class CompactOverlayWindow : Window
{
    private bool _isPinned = true;
    private int _sessionCount;

    public CompactOverlayWindow()
    {
        InitializeComponent();

        // Make it a small floating window (compact overlay)
        var appWindow = this.AppWindow;
        appWindow.Resize(new SizeInt32(280, 160));
        appWindow.SetIcon("Assets\\AppIcon.ico");
        appWindow.Title = "Kvanto";

        // Always on top by default for the compact view
        SetAlwaysOnTop(true);

        // Remove standard title bar – show custom drag handle
        ExtendsContentIntoTitleBar = true;

        // Subscribe to Pomodoro events
        if (App.PomodoroService != null)
        {
            App.PomodoroService.TimerTick += OnTimerTick;
            App.PomodoroService.SessionStateChanged += OnStateChanged;
            App.PomodoroService.SessionCompleted += OnSessionCompleted;
        }

        Closed += (_, _) =>
        {
            if (App.PomodoroService != null)
            {
                App.PomodoroService.TimerTick -= OnTimerTick;
                App.PomodoroService.SessionStateChanged -= OnStateChanged;
                App.PomodoroService.SessionCompleted -= OnSessionCompleted;
            }
        };
    }

    private void OnTimerTick(object? sender, PomodoroTimerEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            TimerDisplay.Text = e.TimeRemaining.ToString(@"mm\:ss");
            TimerProgress.Value = e.ProgressFraction;

            var brush = e.IsBreak
                ? (SolidColorBrush)Application.Current.Resources["PomodoroBreakBrush"]
                : (SolidColorBrush)Application.Current.Resources["PomodoroWorkBrush"];
            TimerProgress.Foreground = brush;
        });
    }

    private void OnStateChanged(object? sender, PomodoroStateEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (e.State == PomodoroState.Idle)
            {
                TaskNameLabel.Text = "No active task";
                StartStopBtn.Content = "▶";
                TimerDisplay.Text = $"{App.PomodoroService?.WorkMinutes ?? 25:D2}:00";
                TimerProgress.Value = 0;
            }
            else
            {
                TaskNameLabel.Text = e.State == PomodoroState.Work
                    ? e.TaskTitle ?? "Working…"
                    : e.State == PomodoroState.ShortBreak ? "Short Break" : "Long Break";
                StartStopBtn.Content = "⏸";
            }
        });
    }

    private void OnSessionCompleted(object? sender, PomodoroStateEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _sessionCount = e.CompletedSessionsCount;
            SessionCountLabel.Text = $"🍅 ×{_sessionCount}";
        });
    }

    private async void OnStartStopClick(object sender, RoutedEventArgs e)
    {
        if (App.PomodoroService == null) return;
        if (App.PomodoroService.IsRunning)
        {
            App.PomodoroService.TogglePause();
        }
        else
        {
            // Bring main window to let user pick a task
            App.MainWindow?.BringToFront();
        }
    }

    private async void OnStopClick(object sender, RoutedEventArgs e)
    {
        if (App.PomodoroService != null)
            await App.PomodoroService.StopAsync();
    }

    private void OnPinClick(object sender, RoutedEventArgs e)
    {
        _isPinned = !_isPinned;
        SetAlwaysOnTop(_isPinned);
    }

    private void OnOpenMainClick(object sender, RoutedEventArgs e)
    {
        App.MainWindow?.BringToFront();
    }

    private void SetAlwaysOnTop(bool value)
    {
        var presenter = this.AppWindow.Presenter as OverlappedPresenter;
        if (presenter != null)
            presenter.IsAlwaysOnTop = value;
    }
}
