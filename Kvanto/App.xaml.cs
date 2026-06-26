using Kvanto.Data;
using Kvanto.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;

namespace Kvanto;

public partial class App : Application
{
    public static MainWindow? MainWindow { get; private set; }
    public static KvantoDbContext? DbContext { get; private set; }
    public static PomodoroService? PomodoroService { get; private set; }
    public static NotificationService? NotificationService { get; private set; }
    public static TrayIconService? TrayIconService { get; private set; }
    public static HotkeyService? HotkeyService { get; private set; }

    public App()
    {
        InitializeComponent();
        UnhandledException += (_, e) =>
        {
            Debug.WriteLine($"[Kvanto] Unhandled exception: {e.Exception}");
            e.Handled = true;
        };
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Create and activate the main window first so the app always has a window.
        // Without a visible window, WinUI 3 will silently exit if any exception
        // is swallowed before Activate() is ever called.
        MainWindow = new MainWindow();
        MainWindow.Activate();

        try
        {
            // Initialize database
            DbContext = new KvantoDbContext();
            await DbContext.Database.MigrateAsync();

            // Initialize services
            NotificationService = new NotificationService();
            PomodoroService = new PomodoroService(NotificationService);
            HotkeyService = new HotkeyService();

            // Initialize tray icon AFTER the window handle is available
            TrayIconService = new TrayIconService(MainWindow, PomodoroService);
            await TrayIconService.InitializeAsync();

            // Register global hotkeys
            HotkeyService.Initialize(MainWindow);

            // Handle activation from toast notification
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs?.Kind == ExtendedActivationKind.ToastNotification)
            {
                MainWindow.BringToFront();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Kvanto] Startup initialization failed: {ex}");

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Startup Error",
                Content = $"Kvanto could not finish initializing:\n\n{ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }
    }
}
