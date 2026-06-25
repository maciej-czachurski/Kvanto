using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvanto.Models;
using Kvanto.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Kvanto.ViewModels;

/// <summary>Data for a single calendar cell.</summary>
public class CalendarDayViewModel : ObservableObject
{
    public DateTime Date { get; init; }
    public bool IsCurrentMonth { get; init; }
    public bool IsToday { get; init; }
    public ObservableCollection<CalendarTaskEntry> TaskEntries { get; } = new();
    public int TotalMinutes => GetTotalMinutes();

    private int GetTotalMinutes()
    {
        int sum = 0;
        foreach (var e in TaskEntries) sum += e.Minutes;
        return sum;
    }
}

public class CalendarTaskEntry
{
    public string TaskTitle { get; init; } = string.Empty;
    public int Minutes { get; init; }
    public string Color { get; init; } = "#6366F1";
    public string DisplayText => $"{TaskTitle} – {Minutes}m";
}

public partial class CalendarViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    [ObservableProperty]
    private int _currentYear = DateTime.Today.Year;

    [ObservableProperty]
    private int _currentMonth = DateTime.Today.Month;

    [ObservableProperty]
    private string _monthLabel = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CalendarDayViewModel> _calendarDays = new();

    [ObservableProperty]
    private CalendarDayViewModel? _selectedDay;

    public CalendarViewModel(DatabaseService db)
    {
        _db = db;
        UpdateMonthLabel();
    }

    [RelayCommand]
    public async Task LoadMonthAsync()
    {
        await RunSafeAsync(async () =>
        {
            var data = await _db.GetMonthSessionsAsync(CurrentYear, CurrentMonth);
            BuildCalendarGrid(data);
        });
    }

    [RelayCommand]
    public async Task NavigatePreviousMonthAsync()
    {
        var d = new DateTime(CurrentYear, CurrentMonth, 1).AddMonths(-1);
        CurrentYear = d.Year;
        CurrentMonth = d.Month;
        UpdateMonthLabel();
        await LoadMonthAsync();
    }

    [RelayCommand]
    public async Task NavigateNextMonthAsync()
    {
        var d = new DateTime(CurrentYear, CurrentMonth, 1).AddMonths(1);
        CurrentYear = d.Year;
        CurrentMonth = d.Month;
        UpdateMonthLabel();
        await LoadMonthAsync();
    }

    private void UpdateMonthLabel() =>
        MonthLabel = new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM yyyy");

    private void BuildCalendarGrid(
        Dictionary<DateTime, List<(TaskItem Task, int Minutes)>> sessionData)
    {
        CalendarDays.Clear();

        var firstDay = new DateTime(CurrentYear, CurrentMonth, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        var today = DateTime.Today;

        // Start from Monday (or Sunday, depending on culture) before the 1st
        int startOffset = ((int)firstDay.DayOfWeek + 6) % 7; // Monday = 0
        var gridStart = firstDay.AddDays(-startOffset);

        // Always show 6 weeks × 7 days = 42 cells
        for (int i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);
            var cell = new CalendarDayViewModel
            {
                Date = date,
                IsCurrentMonth = date.Month == CurrentMonth,
                IsToday = date.Date == today.Date
            };

            if (sessionData.TryGetValue(date.Date, out var entries))
            {
                foreach (var (task, minutes) in entries)
                {
                    cell.TaskEntries.Add(new CalendarTaskEntry
                    {
                        TaskTitle = task.Title,
                        Minutes = minutes,
                        Color = task.Color
                    });
                }
            }

            CalendarDays.Add(cell);
        }
    }
}
