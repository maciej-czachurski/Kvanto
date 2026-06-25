using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Windows.System;

namespace Kvanto.Services;

/// <summary>
/// Registers system-wide (global) hotkeys via the Win32 RegisterHotKey API.
/// Hotkeys work even when the Kvanto window is hidden in the system tray.
/// </summary>
public class HotkeyService : IDisposable
{
    // Win32 constants
    private const int WM_HOTKEY = 0x0312;
    private const int MOD_ALT = 0x0001;
    private const int MOD_CONTROL = 0x0002;
    private const int MOD_SHIFT = 0x0004;
    private const int MOD_WIN = 0x0008;
    private const int MOD_NOREPEAT = 0x4000;

    // Hotkey IDs
    public const int HOTKEY_START_STOP = 1;
    public const int HOTKEY_SKIP_SESSION = 2;
    public const int HOTKEY_OPEN_APP = 3;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private IntPtr _hwnd;
    private bool _initialized;
    private readonly List<int> _registeredIds = new();

    public event EventHandler<int>? HotkeyPressed;

    /// <summary>
    /// Initialize by hooking into the main window's message loop.
    /// Must be called after the window is created.
    /// </summary>
    public void Initialize(MainWindow window)
    {
        _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

        // Register default hotkeys
        // Ctrl+Alt+S → Start/Stop current Pomodoro
        RegisterHotkey(HOTKEY_START_STOP, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)VirtualKey.S);

        // Ctrl+Alt+N → Skip to next session
        RegisterHotkey(HOTKEY_SKIP_SESSION, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)VirtualKey.N);

        // Ctrl+Alt+K → Open/show Kvanto window
        RegisterHotkey(HOTKEY_OPEN_APP, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, (uint)VirtualKey.K);

        _initialized = true;
    }

    private void RegisterHotkey(int id, uint modifiers, uint vk)
    {
        if (RegisterHotKey(_hwnd, id, modifiers, vk))
            _registeredIds.Add(id);
    }

    /// <summary>
    /// Call this from the window's WndProc to handle WM_HOTKEY messages.
    /// Returns true if the message was a registered hotkey.
    /// </summary>
    public bool ProcessMessage(int msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg != WM_HOTKEY) return false;

        var id = wParam.ToInt32();
        HotkeyPressed?.Invoke(this, id);

        switch (id)
        {
            case HOTKEY_START_STOP:
                HandleStartStop();
                break;
            case HOTKEY_SKIP_SESSION:
                HandleSkip();
                break;
            case HOTKEY_OPEN_APP:
                App.MainWindow?.BringToFront();
                break;
        }
        return true;
    }

    private void HandleStartStop()
    {
        if (App.PomodoroService == null) return;

        if (App.PomodoroService.IsRunning)
            App.PomodoroService.TogglePause();
        else
            App.MainWindow?.BringToFront();
    }

    private async void HandleSkip()
    {
        if (App.PomodoroService == null) return;
        await App.PomodoroService.StopAsync();
    }

    public void Dispose()
    {
        if (!_initialized) return;
        foreach (var id in _registeredIds)
            UnregisterHotKey(_hwnd, id);
        _registeredIds.Clear();
    }
}
