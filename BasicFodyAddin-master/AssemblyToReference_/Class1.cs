using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
//using System.Reflecton;

namespace Realm
{
    public class RealmObject
    {
        public RealmObject(string modelName)
        {
            Debug.WriteLine("Creating object from model: " + modelName);

            var properties = GetType().GetTypeInfo();

        }

        public T GetValue<T>(string propertyName)
        {
            Debug.WriteLine("Getting value for " + propertyName);
            return default(T);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            Debug.WriteLine("Setting value for " + propertyName + " to " + value.ToString());
        }
    }

    public class PrimaryKeyAttribute : Attribute
    {
    }

    public class IgnoreAttribute : Attribute
    {
    }
}
