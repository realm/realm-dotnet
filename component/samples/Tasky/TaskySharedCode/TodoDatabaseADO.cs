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
        public Realm Realm;

        public TodoDatabase () 
		{
            Realm = Realm.GetInstance();
		}

		public IEnumerable<TodoItem> GetItems ()
		{
            return Realm.All<TodoItem>().ToList();
		}

		public TodoItem GetItem (string id) 
		{
            return Realm.All<TodoItem>().First(i => i.ID == id);
		}

		public void SaveItem (TodoItem item) 
		{
            // Nothing to see here...
		}

		public string DeleteItem(string id) 
		{
            Realm.Remove(GetItem(id));
            return id;
		}
	}
}
