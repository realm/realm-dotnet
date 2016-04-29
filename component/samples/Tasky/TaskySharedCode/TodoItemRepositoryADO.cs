using System;
using System.Collections.Generic;
using System.IO;
using Realms;

namespace Tasky.Shared 
{
    public class TodoItemRepositoryADO 
    {
        TodoDatabase db = null;
        protected static TodoItemRepositoryADO me;		

        static TodoItemRepositoryADO ()
        {
            me = new TodoItemRepositoryADO();
        }

        protected TodoItemRepositoryADO ()
        {
            // instantiate the database	
            db = new TodoDatabase();
        }

        public static Transaction BeginTransaction()
        {
            return me.db.BeginTransaction();
        }

        public static TodoItem CreateTodoItem()
        {
            return me.db.CreateTodoItem();
        }

        public static TodoItem GetTask(string id)
        {
            return me.db.GetItem(id);
        }

        public static IEnumerable<TodoItem> GetTasks ()
        {
            return me.db.GetItems();
        }

        public static string SaveTask (TodoItem item)
        {
            me.db.SaveItem(item);
            return item.ID;
        }

        public static string DeleteTask(string id)
        {
            return me.db.DeleteItem(id);
        }
    }
}

