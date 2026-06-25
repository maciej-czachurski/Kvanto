using Kvanto.Data;
using Kvanto.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

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
        UnhandledException += (_, e) => e.Handled = true;
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Initialize database
        DbContext = new KvantoDbContext();
        await DbContext.Database.MigrateAsync();

        // Initialize services
        NotificationService = new NotificationService();
        PomodoroService = new PomodoroService(NotificationService);
        HotkeyService = new HotkeyService();

        // Create and activate the main window
        MainWindow = new MainWindow();
        MainWindow.Activate();

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
}
