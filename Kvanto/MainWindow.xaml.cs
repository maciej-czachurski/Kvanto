using Kvanto.Services;
using Kvanto.Views;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Graphics;

namespace Kvanto;

public sealed partial class MainWindow : Window
{
    public AppWindow AppWindow { get; private set; }
    private bool _isMinimizedToTray = false;

    public MainWindow()
    {
        InitializeComponent();

        // Set up AppWindow for title bar customization
        AppWindow = base.AppWindow;
        AppWindow.Title = "Kvanto";
        AppWindow.SetIcon("Assets\\AppIcon.ico");

        // Extend title bar
        ExtendsContentIntoTitleBar = true;

        // Subscribe to Pomodoro timer updates
        if (App.PomodoroService != null)
        {
            App.PomodoroService.TimerTick += OnPomodoroTimerTick;
            App.PomodoroService.SessionStateChanged += OnPomodoroStateChanged;
        }

        // Handle window close to minimize to tray
        AppWindow.Closing += OnWindowClosing;
    }

    private void NavView_Loaded(object sender, RoutedEventArgs e)
    {
        // Select Tasks page by default
        NavView.SelectedItem = NavView.MenuItems[0];
        ContentFrame.Navigate(typeof(TasksPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItem is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            switch (tag)
            {
                case "tasks":
                    ContentFrame.Navigate(typeof(TasksPage));
                    break;
                case "calendar":
                    ContentFrame.Navigate(typeof(CalendarPage));
                    break;
                case "reports":
                    ContentFrame.Navigate(typeof(ReportsPage));
                    break;
                case "compact":
                    OpenCompactOverlay();
                    break;
            }
        }
    }

    public void OpenCompactOverlay()
    {
        var compactWindow = new CompactOverlayWindow();
        compactWindow.Activate();
    }

    private void OnPomodoroTimerTick(object? sender, PomodoroTimerEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            PomodoroTimerLabel.Text = e.TimeRemaining.ToString(@"mm\:ss");
            PomodoroStatusBar.Visibility = Visibility.Visible;

            // Change status bar color based on session type
            PomodoroStatusBar.Background = e.IsBreak
                ? (SolidColorBrush)Application.Current.Resources["PomodoroBreakBrush"]
                : (SolidColorBrush)Application.Current.Resources["PomodoroWorkBrush"];
        });
    }

    private void OnPomodoroStateChanged(object? sender, PomodoroStateEventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (e.State == PomodoroState.Idle)
            {
                PomodoroStatusBar.Visibility = Visibility.Collapsed;
            }
        });
    }

    private void OnWindowClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        // Minimize to tray instead of closing
        args.Cancel = true;
        AppWindow.Hide();
        _isMinimizedToTray = true;
        App.TrayIconService?.ShowBalloonTip("Kvanto is still running in the system tray.");
    }

    public void BringToFront()
    {
        if (_isMinimizedToTray)
        {
            AppWindow.Show();
            _isMinimizedToTray = false;
        }
        AppWindow.MoveInZOrderAtTop();
        this.Activate();
    }
}
