# Kvanto

A modern **WinUI 3** daily task tracker with Pomodoro timer, calendar view, and full reporting — built for Windows 11.

## Features

### ✅ Task Management
- Create, edit, and delete tasks with title, description, category, and priority
- Mark tasks complete; view active and archived tasks
- Per-task stats: total time worked, days worked, Pomodoro sessions completed vs estimated

### 🍅 Pomodoro Timer
- 25-minute work sessions followed by 5-minute (or 15-minute long) breaks
- Configurable durations via Settings
- Visual countdown with circular progress bar in the main window and status bar
- Auto-starts break after each completed session; long break every 4 sessions

### 📅 Calendar View
- Monthly calendar grid showing **which tasks** were worked on each day
- Colored task blocks per day with minutes spent
- Navigate between months; highlight today

### 📊 Reports & Analytics
- Selectable date range (Last 7/30 days, This Year, or custom)
- Summary cards: total time, sessions, active days, daily average, current streak
- Per-task bar chart showing relative time distribution
- Daily activity bar chart

### 📌 Compact Overlay (Pin to Desktop)
- Small always-on-top widget showing the active timer
- Can be pinned or unpinned; opens from main window or tray menu

### 🔔 System Tray Integration
- App minimizes to tray on close
- Tray icon changes colour during work (red) and break (green) sessions
- Context menu: open app, start/pause/stop Pomodoro, open compact view, exit
- Double-click tray icon to restore the window

### ⌨️ Global Keyboard Shortcuts
| Shortcut | Action |
|---|---|
| `Ctrl+Alt+S` | Start / Pause current Pomodoro |
| `Ctrl+Alt+N` | Skip to next session |
| `Ctrl+Alt+K` | Show Kvanto window |

### 🔔 Windows Notifications
- Toast notification at the end of every work or break session
- Action buttons in the notification: Start Break / Start Next Pomodoro
- Tray icon updates to reflect the current state

## Project Structure

```
Kvanto/
├── Kvanto.sln
└── Kvanto/
    ├── Kvanto.csproj              # WinUI 3 / Windows App SDK project
    ├── Package.appxmanifest       # MSIX manifest (tray, toast, startup)
    ├── App.xaml / App.xaml.cs     # Application entry point & DI
    ├── MainWindow.xaml/.cs        # Navigation shell
    ├── Models/
    │   ├── TaskItem.cs
    │   ├── PomodoroSession.cs
    │   └── AppSettings.cs
    ├── Data/
    │   ├── KvantoDbContext.cs      # EF Core + SQLite
    │   └── Migrations/
    ├── Services/
    │   ├── DatabaseService.cs
    │   ├── PomodoroService.cs      # Timer engine
    │   ├── NotificationService.cs  # Windows App SDK AppNotifications
    │   ├── TrayIconService.cs      # H.NotifyIcon.WinUI
    │   └── HotkeyService.cs        # Win32 RegisterHotKey P/Invoke
    ├── ViewModels/
    │   ├── BaseViewModel.cs
    │   ├── MainViewModel.cs
    │   ├── CalendarViewModel.cs
    │   └── ReportsViewModel.cs
    ├── Views/
    │   ├── TasksPage.xaml/.cs
    │   ├── CalendarPage.xaml/.cs
    │   ├── ReportsPage.xaml/.cs
    │   ├── SettingsPage.xaml/.cs
    │   ├── CompactOverlayWindow.xaml/.cs
    │   └── SummaryCard.xaml/.cs    # Reusable stats card
    ├── Converters/
    └── Styles/
        └── AppStyles.xaml
```

## Tech Stack

| Component | Library |
|---|---|
| UI Framework | WinUI 3 (Windows App SDK 1.5) |
| MVVM | CommunityToolkit.Mvvm 8.2 |
| Database | Entity Framework Core 8 + SQLite |
| System Tray | H.NotifyIcon.WinUI 2.1 |
| Notifications | Windows App SDK AppNotifications |
| Hotkeys | Win32 RegisterHotKey (P/Invoke) |
| Target OS | Windows 10 (1809+) / Windows 11 |

## Building

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **Windows App SDK** workload
2. Open `Kvanto.sln`
3. Restore NuGet packages (automatic on first build)
4. Build and run in **Debug|x64** configuration

> The app uses MSIX packaging for proper toast notification identity and system-tray startup registration.
