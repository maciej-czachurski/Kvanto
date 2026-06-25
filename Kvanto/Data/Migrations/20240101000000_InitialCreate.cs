using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kvanto.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    ShortBreakMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LongBreakMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    LongBreakInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    HotkeyStartStop = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HotkeySkip = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ShowDesktopNotifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlaySoundOnEnd = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartMinimizedToTray = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartWithWindows = table.Column<bool>(type: "INTEGER", nullable: false),
                    AppTheme = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedPomodoros = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PlannedDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PomodoroSessions_Tasks_TaskItemId",
                        column: x => x.TaskItemId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "AppTheme", "HotkeySkip", "HotkeyStartStop", "LongBreakInterval", "LongBreakMinutes", "PlaySoundOnEnd", "ShortBreakMinutes", "ShowDesktopNotifications", "StartMinimizedToTray", "StartWithWindows", "WorkDurationMinutes" },
                values: new object[] { 1, "Dark", "Ctrl+Alt+N", "Ctrl+Alt+S", 4, 15, true, 5, true, false, false, 25 });

            migrationBuilder.CreateIndex(
                name: "IX_PomodoroSessions_TaskItemId",
                table: "PomodoroSessions",
                column: "TaskItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PomodoroSessions");
            migrationBuilder.DropTable(name: "Tasks");
            migrationBuilder.DropTable(name: "Settings");
        }
    }
}
