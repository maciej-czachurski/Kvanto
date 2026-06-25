using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kvanto.Services;

// ── Event argument types ──────────────────────────────────────────────────────

public enum PomodoroState { Idle, Work, ShortBreak, LongBreak }

public class PomodoroTimerEventArgs : EventArgs
{
    public TimeSpan TimeRemaining { get; init; }
    public TimeSpan TotalDuration { get; init; }
    public bool IsBreak { get; init; }
    public double ProgressFraction => 1.0 - TimeRemaining.TotalSeconds / TotalDuration.TotalSeconds;
}

public class PomodoroStateEventArgs : EventArgs
{
    public PomodoroState State { get; init; }
    public int? CurrentTaskId { get; init; }
    public string? TaskTitle { get; init; }
    public int CompletedSessionsCount { get; init; }
}

// ── Service ───────────────────────────────────────────────────────────────────

/// <summary>
/// Core Pomodoro timer service.  Raises events on every second tick and
/// when the state (work / break / idle) changes.
/// </summary>
public class PomodoroService : IDisposable
{
    private readonly NotificationService _notifications;
    private Timer? _timer;
    private CancellationTokenSource? _cts;

    // State
    private PomodoroState _state = PomodoroState.Idle;
    private bool _isPaused;
    private int _currentTaskId;
    private string _currentTaskTitle = string.Empty;
    private int _completedSessions;
    private TimeSpan _timeRemaining;
    private TimeSpan _totalDuration;
    private int _activeSessionId;

    // Configurable durations (loaded from settings)
    public int WorkMinutes { get; set; } = 25;
    public int ShortBreakMinutes { get; set; } = 5;
    public int LongBreakMinutes { get; set; } = 15;
    public int LongBreakInterval { get; set; } = 4;

    // Events
    public event EventHandler<PomodoroTimerEventArgs>? TimerTick;
    public event EventHandler<PomodoroStateEventArgs>? SessionStateChanged;
    public event EventHandler<PomodoroStateEventArgs>? SessionCompleted;

    // Readonly state properties
    public PomodoroState State => _state;
    public int CurrentTaskId => _currentTaskId;
    public string CurrentTaskTitle => _currentTaskTitle;
    public bool IsRunning => _state != PomodoroState.Idle;
    public bool IsPaused => _isPaused;

    public PomodoroService(NotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>Start a new work session for the given task.</summary>
    public async Task StartWorkSessionAsync(int taskId, string taskTitle, int? sessionId = null)
    {
        await StopAsync(saveSession: false);

        _currentTaskId = taskId;
        _currentTaskTitle = taskTitle;
        _state = PomodoroState.Work;
        _totalDuration = TimeSpan.FromMinutes(WorkMinutes);
        _timeRemaining = _totalDuration;
        _activeSessionId = sessionId ?? 0;

        StartTimer();
        RaiseStateChanged();
    }

    /// <summary>Start a short or long break.</summary>
    public async Task StartBreakAsync(bool longBreak = false)
    {
        await StopAsync(saveSession: false);

        _state = longBreak ? PomodoroState.LongBreak : PomodoroState.ShortBreak;
        _totalDuration = TimeSpan.FromMinutes(longBreak ? LongBreakMinutes : ShortBreakMinutes);
        _timeRemaining = _totalDuration;

        StartTimer();
        RaiseStateChanged();
    }

    /// <summary>Pause/resume the current session.</summary>
    public void TogglePause()
    {
        if (_state == PomodoroState.Idle) return;

        if (_timer != null)
        {
            // Pause: stop the timer
            _cts?.Cancel();
            _cts = null;
            _timer.Dispose();
            _timer = null;
            _isPaused = true;
        }
        else
        {
            // Resume
            _isPaused = false;
            StartTimer();
        }
    }

    /// <summary>Stop the active session.</summary>
    public Task StopAsync(bool saveSession = true) => Task.Run(() =>
    {
        _cts?.Cancel();
        _cts = null;
        _timer?.Dispose();
        _timer = null;
        _isPaused = false;
        _state = PomodoroState.Idle;
        _currentTaskId = 0;
        _currentTaskTitle = string.Empty;
        RaiseStateChanged();
    });

    private void StartTimer()
    {
        _cts = new CancellationTokenSource();
        _timer = new Timer(_ =>
        {
            if (_cts == null || _cts.IsCancellationRequested) return;

            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));

            TimerTick?.Invoke(this, new PomodoroTimerEventArgs
            {
                TimeRemaining = _timeRemaining,
                TotalDuration = _totalDuration,
                IsBreak = _state != PomodoroState.Work
            });

            if (_timeRemaining <= TimeSpan.Zero)
            {
                OnSessionCompleted();
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    private void OnSessionCompleted()
    {
        _timer?.Dispose();
        _timer = null;
        _cts?.Cancel();

        var completedState = _state;
        var completedTask = _currentTaskTitle;

        if (_state == PomodoroState.Work)
        {
            _completedSessions++;
            _notifications.SendPomodoroEndNotification(
                _currentTaskTitle,
                _completedSessions,
                isWork: true);
        }
        else
        {
            _notifications.SendPomodoroEndNotification(
                _currentTaskTitle,
                _completedSessions,
                isWork: false);
        }

        SessionCompleted?.Invoke(this, new PomodoroStateEventArgs
        {
            State = completedState,
            CurrentTaskId = _currentTaskId,
            TaskTitle = _currentTaskTitle,
            CompletedSessionsCount = _completedSessions
        });

        _state = PomodoroState.Idle;
        _timeRemaining = TimeSpan.Zero;
        RaiseStateChanged();
    }

    private void RaiseStateChanged()
    {
        SessionStateChanged?.Invoke(this, new PomodoroStateEventArgs
        {
            State = _state,
            CurrentTaskId = _currentTaskId,
            TaskTitle = _currentTaskTitle,
            CompletedSessionsCount = _completedSessions
        });
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _timer?.Dispose();
    }
}
