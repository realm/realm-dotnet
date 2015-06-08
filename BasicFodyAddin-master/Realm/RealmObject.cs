using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;

namespace Realm
{
    public class RealmObject
    {
        public RealmObject()
        {
            var modelName = GetType().Name;

            if (GetType().GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Count() == 0)
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");

            Debug.WriteLine("Creating object from model: " + modelName);

            //var properties = GetType().GetTypeInfo().GetProperties();
            //foreach (var prop in properties)
            //{
            //    Debug.WriteLine("Property: " + prop.Name);
            //}
        }

        public T GetValue<T>(string propertyName)
        {
            Debug.WriteLine("Getting " + typeof(T).Name + " value for " + propertyName);
            return default(T);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            Debug.WriteLine("Setting " + typeof(T).Name + " value for " + propertyName + " to " + value.ToString());
        }
    }
}
