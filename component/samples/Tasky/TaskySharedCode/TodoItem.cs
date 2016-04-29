using System;
using Realms;

namespace Tasky.Shared 
{
    /// <summary>
    /// Todo Item business object
    /// </summary>
    public class TodoItem : RealmObject
    {
        public TodoItem ()
        {
        }

        [ObjectId]
        public string ID { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }
        public bool Done { get; set; }
    }
}
