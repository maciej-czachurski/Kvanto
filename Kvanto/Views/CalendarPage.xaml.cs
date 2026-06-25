using Kvanto.Services;
using Kvanto.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace Kvanto.Views;

public sealed partial class CalendarPage : Page
{
    public CalendarViewModel ViewModel { get; }

    public CalendarPage()
    {
        var db = new DatabaseService(App.DbContext!);
        ViewModel = new CalendarViewModel(db);
        InitializeComponent();
        _ = ViewModel.LoadMonthAsync();
    }

    private void OnCalendarDayTapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is CalendarDayViewModel day)
        {
            ViewModel.SelectedDay = day;
        }
    }
}
