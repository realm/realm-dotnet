using System;
using System.Linq;
using System.Collections.Generic;

using Realms;
using System.IO;

namespace Tasky.Shared
{
    /// <summary>
    /// TaskDatabase uses ADO.NET to create the [Items] table and create,read,update,delete data
    /// </summary>
    public class TodoDatabase 
    {
        private Realm realm;

        public TodoDatabase () 
        {
            realm = Realm.GetInstance();
        }

        public TodoItem CreateTodoItem()
        {
            var result = realm.CreateObject<TodoItem>();
            result.ID = Guid.NewGuid().ToString();
            return result;
        }

        public Transaction BeginTransaction()
        {
            return realm.BeginWrite();
        }

        public IEnumerable<TodoItem> GetItems ()
        {
            return realm.All<TodoItem>().ToList();
        }

        public TodoItem GetItem (string id) 
        {
            return realm.All<TodoItem>().single(i => i.ID == id);
        }

        public void SaveItem (TodoItem item) 
        {
            // Nothing to see here...
        }

        public string DeleteItem(string id) 
        {
            realm.Remove(GetItem(id));
            return id;
        }
    }
}
