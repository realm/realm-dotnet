using System;
using System.Collections.Generic;
using Realms;

namespace SimpleToDo.Models;

public partial class ToDoList : IRealmObject
{
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IList<Item> Items { get; } = null!;
}
