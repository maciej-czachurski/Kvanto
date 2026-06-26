using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvanto.Models;
using Kvanto.Services;
using Microsoft.UI.Dispatching;
using TaskStatus = Kvanto.Models.TaskStatus;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kvanto.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly DatabaseService _db;
    private readonly PomodoroService _pomodoro;
    private readonly DispatcherQueue? _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private TaskItem? _selectedTask;

    [ObservableProperty]
    private TaskItem? _activeTask;

    [ObservableProperty]
    private string _pomodoroTimeDisplay = "25:00";

    [ObservableProperty]
    private double _pomodoroProgress;

    [ObservableProperty]
    private bool _isPomodoroRunning;

    [ObservableProperty]
    private bool _isBreak;

    [ObservableProperty]
    private string _pomodoroLabel = "Start Pomodoro";

    [ObservableProperty]
    private string _sessionCountLabel = string.Empty;

    // Dialog / form state
    [ObservableProperty]
    private bool _isAddTaskDialogOpen;

    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    [ObservableProperty]
    private string _newTaskDescription = string.Empty;

    [ObservableProperty]
    private string _newTaskCategory = "General";

    [ObservableProperty]
    private int _newTaskEstimatedPomodoros = 1;

    // Stores the enum as an index so ComboBox.SelectedIndex can bind TwoWay correctly.
    // Matches TaskPriority enum order: Low=0, Medium=1, High=2, Critical=3.
    [ObservableProperty]
    private int _newTaskPriorityIndex = (int)TaskPriority.Medium;

    public TaskPriority NewTaskPriority => (TaskPriority)NewTaskPriorityIndex;

    private int _completedSessions;
    private int _currentSessionDbId;

    public MainViewModel(DatabaseService db, PomodoroService pomodoro, DispatcherQueue? dispatcherQueue = null)
    {
        _db = db;
        _pomodoro = pomodoro;
        _dispatcherQueue = dispatcherQueue;

        _pomodoro.TimerTick += OnTimerTick;
        _pomodoro.SessionStateChanged += OnStateChanged;
        _pomodoro.SessionCompleted += OnSessionCompleted;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    public async Task LoadTasksAsync()
    {
        await RunSafeAsync(async () =>
        {
            var list = await _db.GetActiveTasksAsync();
            Tasks.Clear();
            foreach (var t in list) Tasks.Add(t);
        });
    }

    [RelayCommand]
    public async Task StartPomodoroAsync()
    {
        if (SelectedTask == null) return;

        if (_pomodoro.IsRunning && _pomodoro.CurrentTaskId == SelectedTask.Id)
        {
            _pomodoro.TogglePause();
            PomodoroLabel = _pomodoro.IsPaused ? "Resume Pomodoro" : "Pause";
            return;
        }

        // Persist session start
        _currentSessionDbId = 0;
        var session = await _db.StartSessionAsync(
            SelectedTask.Id, SessionType.Work, _pomodoro.WorkMinutes);
        _currentSessionDbId = session.Id;

        ActiveTask = SelectedTask;
        await _pomodoro.StartWorkSessionAsync(
            SelectedTask.Id, SelectedTask.Title, _currentSessionDbId);

        IsPomodoroRunning = true;
        PomodoroLabel = "Pause";
    }

    [RelayCommand]
    public async Task StopPomodoroAsync()
    {
        if (_currentSessionDbId > 0)
            await _db.CompleteSessionAsync(_currentSessionDbId, completed: false);

        await _pomodoro.StopAsync();
        IsPomodoroRunning = false;
        PomodoroLabel = "Start Pomodoro";
        ActiveTask = null;
        PomodoroTimeDisplay = $"{_pomodoro.WorkMinutes:D2}:00";
        PomodoroProgress = 0;
    }

    [RelayCommand]
    public void OpenAddTaskDialog()
    {
        NewTaskTitle = string.Empty;
        NewTaskDescription = string.Empty;
        NewTaskCategory = "General";
        NewTaskEstimatedPomodoros = 1;
        NewTaskPriorityIndex = (int)TaskPriority.Medium;
        IsAddTaskDialogOpen = true;
    }

    [RelayCommand]
    public async Task AddTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        await RunSafeAsync(async () =>
        {
            var task = new TaskItem
            {
                Title = NewTaskTitle.Trim(),
                Description = NewTaskDescription.Trim(),
                Category = NewTaskCategory.Trim(),
                EstimatedPomodoros = NewTaskEstimatedPomodoros,
                Priority = NewTaskPriority
            };
            await _db.CreateTaskAsync(task);
            Tasks.Insert(0, task);
        });

        IsAddTaskDialogOpen = false;
    }

    [RelayCommand]
    public async Task CompleteTaskAsync(TaskItem? task)
    {
        if (task == null) return;
        task.Status = TaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;
        await _db.UpdateTaskAsync(task);
        Tasks.Remove(task);
    }

    [RelayCommand]
    public async Task DeleteTaskAsync(TaskItem? task)
    {
        if (task == null) return;
        await _db.DeleteTaskAsync(task.Id);
        Tasks.Remove(task);
    }

    // ── Pomodoro event handlers ───────────────────────────────────────────────

    private void OnTimerTick(object? sender, PomodoroTimerEventArgs e)
    {
        void Update()
        {
            PomodoroTimeDisplay = e.TimeRemaining.ToString(@"mm\:ss");
            PomodoroProgress = e.ProgressFraction;
            IsBreak = e.IsBreak;
        }

        if (_dispatcherQueue != null)
            _dispatcherQueue.TryEnqueue(Update);
        else
            Update();
    }

    private void OnStateChanged(object? sender, PomodoroStateEventArgs e)
    {
        void Update()
        {
            IsPomodoroRunning = e.State != PomodoroState.Idle;
            if (e.State == PomodoroState.Idle)
            {
                PomodoroLabel = "Start Pomodoro";
                ActiveTask = null;
            }
        }

        if (_dispatcherQueue != null)
            _dispatcherQueue.TryEnqueue(Update);
        else
            Update();
    }

    private async void OnSessionCompleted(object? sender, PomodoroStateEventArgs e)
    {
        _completedSessions = e.CompletedSessionsCount;

        if (_dispatcherQueue != null)
            _dispatcherQueue.TryEnqueue(() => SessionCountLabel = $"🍅 ×{_completedSessions}");
        else
            SessionCountLabel = $"🍅 ×{_completedSessions}";

        if (_currentSessionDbId > 0)
            await _db.CompleteSessionAsync(_currentSessionDbId, completed: true);

        // Auto-start break
        bool longBreak = _completedSessions % _pomodoro.LongBreakInterval == 0;
        await _pomodoro.StartBreakAsync(longBreak);
    }
}
