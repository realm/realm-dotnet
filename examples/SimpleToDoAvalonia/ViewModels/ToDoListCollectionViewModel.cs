using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Realms;
using SimpleToDo.Models;

namespace SimpleToDo.ViewModels;

public partial class ToDoListCollectionViewModel : ViewModelBase
{
    private readonly Realm _realm = null!;

    [ObservableProperty] private IEnumerable<ToDoList> _list = null!;
    [ObservableProperty] private ToDoList _selectedList = null!;

    public event EventHandler<ToDoList> SelectedItemEvent = delegate { };

    public ToDoListCollectionViewModel()
    {
        if (!Design.IsDesignMode)
        {
            _realm = Realm.GetInstance();
            List = _realm.All<ToDoList>().OrderBy(tdl => tdl.CreatedAt);
        }
        else
        {
            //This sets example data for the UI preview
            var list1 = new ToDoList
            {
                Name = "List1",
                CreatedAt = DateTimeOffset.Now.AddHours(-2),
                Items =
                {
                    new Item
                    {
                        Description = "Check1", IsDone = true
                    },

                    new Item { Description = "Check2", IsDone = true },
                    new Item { Description = "Uncheck1" },
                    new Item { Description = "Uncheck2" }
                }
            };
            var list2 = new ToDoList
            {
                Name = "List2",
                CreatedAt = DateTimeOffset.Now.AddHours(-1),
                Items =
                {
                    new Item { Description = "Check1", IsDone = true },
                    new Item { Description = "Check2", IsDone = true },
                    new Item { Description = "Uncheck1" },
                    new Item { Description = "Uncheck2" }
                }
            };

            List = new[] { list1, list2 };
        }
    }

    partial void OnSelectedListChanged(ToDoList value)
    {
        SelectedItemEvent(this, value);
    }

    public void AddList()
    {
        var newList = _realm.Write(() => _realm.Add(new ToDoList
        {
            CreatedAt = DateTimeOffset.Now
        }));

        SelectedList = newList;
    }

    public void DeleteList(ToDoList listToDelete)
    {
        _realm.Write(() => { _realm.Remove(listToDelete); });
    }
}