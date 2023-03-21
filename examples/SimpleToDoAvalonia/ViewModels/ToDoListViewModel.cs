using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Realms;
using SimpleToDo.Models;

namespace SimpleToDo.ViewModels;

public partial class ToDoListViewModel : ViewModelBase
{
    private readonly Realm _realm = null!;

    [ObservableProperty]
    private IEnumerable<Item> _checkedItems = null!;

    [ObservableProperty]
    private ToDoList _currentList = null!;

    [ObservableProperty]
    private IEnumerable<Item> _uncheckedItems = null!;

    public ToDoListViewModel()
    {
        if (!Design.IsDesignMode)
        {
            _realm = Realm.GetInstance();
        }
        else
        {
            // This sets example data for the UI preview
            UncheckedItems = new[]
            {
                new Item { Description = "Uncheck1" },
                new Item { Description = "Uncheck2" }
            };
            CheckedItems = new[]
            {
                new Item { Description = "Check1", IsDone = true },
                new Item { Description = "Check2", IsDone = true }
            };
            CurrentList = new ToDoList
            {
                Name = "List1"
            };
        }
    }

    public void GoToList(ToDoList list)
    {
        CurrentList = list;
        UncheckedItems = list.Items.AsRealmQueryable().Where(l => l.IsDone == false);
        CheckedItems = list.Items.AsRealmQueryable().Where(l => l.IsDone == true);
    }

    public void DeleteItem(Item item)
    {
        _realm.Write(() => { CurrentList.Items.Remove(item); });
    }

    public void AddItem()
    {
        _realm.Write(() => { CurrentList.Items.Add(new Item()); });
    }
}