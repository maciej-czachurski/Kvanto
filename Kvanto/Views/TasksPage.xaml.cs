using Kvanto.Models;
using Kvanto.Services;
using Kvanto.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Kvanto.Views;

public sealed partial class TasksPage : Page
{
    public MainViewModel ViewModel { get; }

    public TasksPage()
    {
        var db = new DatabaseService(App.DbContext!);
        ViewModel = new MainViewModel(db, App.PomodoroService!, DispatcherQueue);
        InitializeComponent();
        _ = ViewModel.LoadTasksAsync();
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        Unloaded += (_, _) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private async void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsAddTaskDialogOpen) && ViewModel.IsAddTaskDialogOpen)
        {
            try
            {
                AddTaskDialog.XamlRoot = XamlRoot;
                await AddTaskDialog.ShowAsync();
            }
            finally
            {
                ViewModel.IsAddTaskDialogOpen = false;
            }
        }
    }

    private async void OnCompleteTaskClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
            await ViewModel.CompleteTaskAsync(task);
    }

    private async void OnDeleteTaskClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TaskItem task)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Task",
                Content = $"Are you sure you want to delete \"{task.Title}\"?\nAll Pomodoro history will also be deleted.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                XamlRoot = XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                await ViewModel.DeleteTaskAsync(task);
        }
    }
}
