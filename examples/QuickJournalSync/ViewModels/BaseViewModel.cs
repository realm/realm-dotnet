using CommunityToolkit.Mvvm.ComponentModel;

namespace QuickJournalSync.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    protected bool _isBusy;

    protected Action? _currentDismissAction;

    partial void OnIsBusyChanged(bool value)
    {
        if (value)
        {
            _currentDismissAction = Services.DialogService.ShowActivityIndicator();
        }
        else
        {
            _currentDismissAction?.Invoke();
            _currentDismissAction = null;
        }
    }
}