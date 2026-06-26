using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kvanto.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Kvanto.ViewModels;

public class TaskAnalyticsItem
{
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Color { get; init; } = "#6366F1";
    public int TotalMinutes { get; init; }
    public int WorkedDays { get; init; }
    public int Sessions { get; init; }
    public string TotalTimeDisplay => TotalMinutes >= 60
        ? $"{TotalMinutes / 60}h {TotalMinutes % 60}m"
        : $"{TotalMinutes}m";
    public double BarWidthFraction { get; set; }
}

public class DailyTotalItem
{
    public DateTime Date { get; init; }
    public int Minutes { get; init; }
    public string DateLabel => Date.ToString("MMM d");
    public string MinutesLabel => $"{Minutes}m";
}

public partial class ReportsViewModel : BaseViewModel
{
    private readonly DatabaseService _db;

    [ObservableProperty]
    private DateTimeOffset _fromDate = DateTimeOffset.Now.AddDays(-30).Date;

    [ObservableProperty]
    private DateTimeOffset _toDate = DateTimeOffset.Now.Date;

    [ObservableProperty]
    private ObservableCollection<TaskAnalyticsItem> _taskAnalytics = new();

    [ObservableProperty]
    private ObservableCollection<DailyTotalItem> _dailyTotals = new();

    [ObservableProperty]
    private string _totalWorkedDisplay = "0m";

    [ObservableProperty]
    private int _totalSessions;

    [ObservableProperty]
    private int _totalDays;

    [ObservableProperty]
    private string _averageDailyDisplay = "0m";

    [ObservableProperty]
    private string _topTaskTitle = "—";

    [ObservableProperty]
    private int _currentStreak;

    public ReportsViewModel(DatabaseService db)
    {
        _db = db;
    }

    [RelayCommand]
    public async Task LoadReportAsync()
    {
        await RunSafeAsync(async () =>
        {
            var analytics = await _db.GetTaskAnalyticsAsync(FromDate.DateTime, ToDate.DateTime);
            var dailies = await _db.GetDailyTotalsAsync(FromDate.DateTime, ToDate.DateTime);

            // Summary stats
            int totalMin = analytics.Sum(x => x.TotalMinutes);
            TotalWorkedDisplay = totalMin >= 60
                ? $"{totalMin / 60}h {totalMin % 60}m"
                : $"{totalMin}m";

            TotalSessions = analytics.Sum(x => x.Sessions);
            TotalDays = dailies.Count;
            AverageDailyDisplay = TotalDays > 0
                ? FormatMinutes(totalMin / TotalDays)
                : "0m";

            TopTaskTitle = analytics.Count > 0 ? analytics[0].Task.Title : "—";

            // Calculate streak
            CurrentStreak = CalculateStreak(dailies.Select(d => d.Date).ToList());

            // Per-task analytics
            TaskAnalytics.Clear();
            int maxMin = analytics.Count > 0 ? analytics[0].TotalMinutes : 1;
            foreach (var (task, min, days, sessions) in analytics)
            {
                TaskAnalytics.Add(new TaskAnalyticsItem
                {
                    Title = task.Title,
                    Category = task.Category,
                    Color = task.Color,
                    TotalMinutes = min,
                    WorkedDays = days,
                    Sessions = sessions,
                    BarWidthFraction = maxMin > 0 ? (double)min / maxMin : 0
                });
            }

            // Daily totals for chart
            DailyTotals.Clear();
            foreach (var (date, min) in dailies)
                DailyTotals.Add(new DailyTotalItem { Date = date, Minutes = min });
        });
    }

    [RelayCommand]
    public Task SetRangeLastWeekAsync()
    {
        FromDate = DateTimeOffset.Now.AddDays(-7).Date;
        ToDate = DateTimeOffset.Now.Date;
        return LoadReportAsync();
    }

    [RelayCommand]
    public Task SetRangeLastMonthAsync()
    {
        FromDate = DateTimeOffset.Now.AddDays(-30).Date;
        ToDate = DateTimeOffset.Now.Date;
        return LoadReportAsync();
    }

    [RelayCommand]
    public Task SetRangeThisYearAsync()
    {
        var now = DateTimeOffset.Now;
        FromDate = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);
        ToDate = now.Date;
        return LoadReportAsync();
    }

    private static string FormatMinutes(int minutes) =>
        minutes >= 60 ? $"{minutes / 60}h {minutes % 60}m" : $"{minutes}m";

    private static int CalculateStreak(System.Collections.Generic.List<DateTime> workedDays)
    {
        if (workedDays.Count == 0) return 0;

        var days = new System.Collections.Generic.HashSet<DateTime>(
            workedDays.Select(d => d.Date));

        int streak = 0;
        var check = DateTime.Today;
        while (days.Contains(check))
        {
            streak++;
            check = check.AddDays(-1);
        }
        return streak;
    }
}
