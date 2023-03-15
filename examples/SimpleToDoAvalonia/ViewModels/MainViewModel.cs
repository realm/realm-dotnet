using CommunityToolkit.Mvvm.ComponentModel;
using SimpleToDo.Models;

namespace SimpleToDo.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private ToDoListViewModel _content = null!;

    [ObservableProperty] private ToDoListCollectionViewModel _pane = null!;

    public MainViewModel()
    {
        Pane = new ToDoListCollectionViewModel();
        Content = new ToDoListViewModel();

        Pane.SelectedItemEvent += PaneOnSelectedItemEvent;
    }

    private void PaneOnSelectedItemEvent(object? sender, ToDoList list)
    {
        Content.GoToList(list);
    }
}