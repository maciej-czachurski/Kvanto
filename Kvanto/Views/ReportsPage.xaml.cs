using Kvanto.Services;
using Kvanto.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Kvanto.Views;

public sealed partial class ReportsPage : Page
{
    public ReportsViewModel ViewModel { get; }

    public ReportsPage()
    {
        var db = new DatabaseService(App.DbContext!);
        ViewModel = new ReportsViewModel(db);
        InitializeComponent();
        _ = ViewModel.LoadReportAsync();
    }
}
