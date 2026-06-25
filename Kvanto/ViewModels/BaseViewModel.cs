using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace Kvanto.ViewModels;

/// <summary>Base view model wiring CommunityToolkit.Mvvm.</summary>
public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    protected async Task RunSafeAsync(System.Func<Task> action)
    {
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            await action();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
