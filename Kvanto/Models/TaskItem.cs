using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Kvanto.Models;

public enum TaskPriority { Low, Medium, High, Critical }
public enum TaskStatus { Active, Paused, Completed, Archived }

public class TaskItem
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = "General";

    [MaxLength(7)]
    public string Color { get; set; } = "#6366F1";

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; set; } = TaskStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }

    /// <summary>Estimated number of 25-min Pomodoro work sessions.</summary>
    public int EstimatedPomodoros { get; set; } = 1;

    public ICollection<PomodoroSession> Sessions { get; set; } = new List<PomodoroSession>();

    // ── Computed helpers ──────────────────────────────────────────────────────

    /// <summary>Total minutes of completed work sessions for this task.</summary>
    public int TotalWorkedMinutes
    {
        get
        {
            int total = 0;
            foreach (var s in Sessions)
                if (s.Type == SessionType.Work && s.IsCompleted)
                    total += s.ActualDurationMinutes;
            return total;
        }
    }

    /// <summary>Number of distinct calendar days on which this task was worked.</summary>
    public int WorkedDaysCount
    {
        get
        {
            var days = new HashSet<DateTime>();
            foreach (var s in Sessions)
                if (s.Type == SessionType.Work && s.IsCompleted)
                    days.Add(s.StartTime.ToLocalTime().Date);
            return days.Count;
        }
    }

    /// <summary>Number of completed Pomodoro work sessions.</summary>
    public int CompletedPomodoros
    {
        get
        {
            int count = 0;
            foreach (var s in Sessions)
                if (s.Type == SessionType.Work && s.IsCompleted)
                    count++;
            return count;
        }
    }
}
