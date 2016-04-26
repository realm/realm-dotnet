using System;
using System.Collections.Generic;
using Realms;

namespace Tasky.Shared 
{
    /// <summary>
    /// Manager classes are an abstraction on the data access layers
    /// </summary>
    public static class TodoItemManager 
    {
        static TodoItemManager ()
        {
        }
        
        public static TodoItem GetTask(string id)
        {
            return TodoItemRepositoryADO.GetTask(id);
        }
        
        public static IList<TodoItem> GetTasks ()
        {
            return new List<TodoItem>(TodoItemRepositoryADO.GetTasks());
        }
        
        public static Transaction BeginTransaction ()
        {
            return TodoItemRepositoryADO.BeginTransaction();
        }

        public static TodoItem CreateTodoItem ()
        {
            return TodoItemRepositoryADO.CreateTodoItem();
        }

        public static string SaveTask (TodoItem item)
        {
            return TodoItemRepositoryADO.SaveTask(item);
        }
        
        public static string DeleteTask(string id)
        {
            return TodoItemRepositoryADO.DeleteTask(id);
        }
    }
}
