using System;
using System.ComponentModel.DataAnnotations;

namespace Kvanto.Models;

public enum SessionType { Work, ShortBreak, LongBreak }

public class PomodoroSession
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public SessionType Type { get; set; } = SessionType.Work;

    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }

    /// <summary>Planned duration in minutes (e.g. 25 for work, 5 for short break).</summary>
    public int PlannedDurationMinutes { get; set; } = 25;

    /// <summary>True when the session ran to completion (was not interrupted).</summary>
    public bool IsCompleted { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;

    // ── Computed helpers ──────────────────────────────────────────────────────

    /// <summary>Actual elapsed minutes (capped at planned duration).</summary>
    public int ActualDurationMinutes
    {
        get
        {
            if (EndTime == null) return 0;
            var elapsed = (int)(EndTime.Value - StartTime).TotalMinutes;
            return Math.Min(elapsed, PlannedDurationMinutes);
        }
    }
}
