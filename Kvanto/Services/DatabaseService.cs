using Kvanto.Data;
using Kvanto.Models;
using Microsoft.EntityFrameworkCore;
using TaskStatus = Kvanto.Models.TaskStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kvanto.Services;

/// <summary>Provides high-level task and session data operations.</summary>
public class DatabaseService
{
    private readonly KvantoDbContext _db;

    public DatabaseService(KvantoDbContext db) => _db = db;

    // ── Tasks ─────────────────────────────────────────────────────────────────

    public Task<List<TaskItem>> GetActiveTasksAsync() =>
        _db.Tasks
           .Include(t => t.Sessions)
           .Where(t => t.Status == TaskStatus.Active || t.Status == TaskStatus.Paused)
           .OrderByDescending(t => t.Priority)
           .ThenBy(t => t.CreatedAt)
           .ToListAsync();

    public Task<List<TaskItem>> GetAllTasksAsync() =>
        _db.Tasks
           .Include(t => t.Sessions)
           .OrderByDescending(t => t.CreatedAt)
           .ToListAsync();

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await _db.Tasks.FindAsync(taskId);
        if (task != null)
        {
            _db.Tasks.Remove(task);
            await _db.SaveChangesAsync();
        }
    }

    public Task<TaskItem?> GetTaskWithSessionsAsync(int taskId) =>
        _db.Tasks
           .Include(t => t.Sessions)
           .FirstOrDefaultAsync(t => t.Id == taskId);

    // ── Sessions ──────────────────────────────────────────────────────────────

    public async Task<PomodoroSession> StartSessionAsync(int taskId, SessionType type, int durationMinutes)
    {
        var session = new PomodoroSession
        {
            TaskItemId = taskId,
            Type = type,
            StartTime = DateTime.UtcNow,
            PlannedDurationMinutes = durationMinutes
        };
        _db.PomodoroSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task CompleteSessionAsync(int sessionId, bool completed)
    {
        var session = await _db.PomodoroSessions.FindAsync(sessionId);
        if (session == null) return;
        session.EndTime = DateTime.UtcNow;
        session.IsCompleted = completed;
        await _db.SaveChangesAsync();
    }

    // ── Calendar data ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all completed work sessions grouped by local date for the given month.
    /// Key = local date, Value = list of (task, totalMinutes).
    /// </summary>
    public async Task<Dictionary<DateTime, List<(TaskItem Task, int Minutes)>>> GetMonthSessionsAsync(
        int year, int month)
    {
        var from = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddMonths(1);

        var sessions = await _db.PomodoroSessions
            .Include(s => s.TaskItem)
            .Where(s => s.Type == SessionType.Work
                     && s.IsCompleted
                     && s.StartTime >= from
                     && s.StartTime < to)
            .ToListAsync();

        var result = new Dictionary<DateTime, List<(TaskItem, int)>>();
        foreach (var s in sessions)
        {
            var date = s.StartTime.ToLocalTime().Date;
            if (!result.TryGetValue(date, out var dayList))
            {
                dayList = new List<(TaskItem, int)>();
                result[date] = dayList;
            }

            var existing = dayList.FindIndex(x => x.Item1.Id == s.TaskItem.Id);
            if (existing >= 0)
            {
                var (t, m) = dayList[existing];
                dayList[existing] = (t, m + s.ActualDurationMinutes);
            }
            else
            {
                dayList.Add((s.TaskItem, s.ActualDurationMinutes));
            }
        }
        return result;
    }

    // ── Analytics ─────────────────────────────────────────────────────────────

    /// <summary>Total worked minutes per task in the given date range.</summary>
    public async Task<List<(TaskItem Task, int TotalMinutes, int Days, int Sessions)>>
        GetTaskAnalyticsAsync(DateTime from, DateTime to)
    {
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime().AddDays(1);

        var sessions = await _db.PomodoroSessions
            .Include(s => s.TaskItem)
            .Where(s => s.Type == SessionType.Work
                     && s.IsCompleted
                     && s.StartTime >= fromUtc
                     && s.StartTime < toUtc)
            .ToListAsync();

        return sessions
            .GroupBy(s => s.TaskItem)
            .Select(g =>
            {
                var days = g.Select(s => s.StartTime.ToLocalTime().Date).Distinct().Count();
                var minutes = g.Sum(s => s.ActualDurationMinutes);
                return (g.Key, minutes, days, g.Count());
            })
            .OrderByDescending(x => x.minutes)
            .ToList();
    }

    /// <summary>Daily totals (sum of all work minutes) for the given date range.</summary>
    public async Task<List<(DateTime Date, int TotalMinutes)>> GetDailyTotalsAsync(
        DateTime from, DateTime to)
    {
        var fromUtc = from.ToUniversalTime();
        var toUtc = to.ToUniversalTime().AddDays(1);

        var sessions = await _db.PomodoroSessions
            .Where(s => s.Type == SessionType.Work
                     && s.IsCompleted
                     && s.StartTime >= fromUtc
                     && s.StartTime < toUtc)
            .ToListAsync();

        return sessions
            .GroupBy(s => s.StartTime.ToLocalTime().Date)
            .Select(g => (g.Key, g.Sum(s => s.ActualDurationMinutes)))
            .OrderBy(x => x.Item1)
            .ToList();
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public async Task<AppSettings> GetSettingsAsync()
    {
        var settings = await _db.Settings.FindAsync(1);
        if (settings == null)
        {
            settings = new AppSettings { Id = 1 };
            _db.Settings.Add(settings);
            await _db.SaveChangesAsync();
        }
        return settings;
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        _db.Settings.Update(settings);
        await _db.SaveChangesAsync();
    }
}
