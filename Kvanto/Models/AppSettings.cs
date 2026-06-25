using System.ComponentModel.DataAnnotations;

namespace Kvanto.Models;

/// <summary>Application-wide settings stored in the local database.</summary>
public class AppSettings
{
    public int Id { get; set; } = 1;

    // Pomodoro durations (minutes)
    public int WorkDurationMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;

    /// <summary>Number of work sessions before a long break.</summary>
    public int LongBreakInterval { get; set; } = 4;

    // Hotkeys (virtual key codes, stored as strings)
    [MaxLength(50)]
    public string HotkeyStartStop { get; set; } = "Ctrl+Alt+S";

    [MaxLength(50)]
    public string HotkeySkip { get; set; } = "Ctrl+Alt+N";

    // Notifications
    public bool ShowDesktopNotifications { get; set; } = true;
    public bool PlaySoundOnEnd { get; set; } = true;

    // Startup
    public bool StartMinimizedToTray { get; set; } = false;
    public bool StartWithWindows { get; set; } = false;

    // Theme
    [MaxLength(20)]
    public string AppTheme { get; set; } = "Dark";
}
