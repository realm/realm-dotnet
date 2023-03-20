using Realms;

namespace SimpleToDo.Models;

public partial class Item : IEmbeddedObject
{
    public string? Description { get; set; }

    public bool IsDone { get; set; }
}
