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
        public RealmObject()
        {
            var modelName = GetType().Name;

            if (GetType().GetCustomAttributes(typeof(WovenAttribute), true).Length == 0)
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");

            Debug.WriteLine("Creating object from model: " + modelName);

            var properties = GetType().GetProperties();
            foreach (var prop in properties)
            {
                Debug.WriteLine("Property: " + prop.Name);
            }
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

    public class RealmList<T> : IQueryable where T : RealmObject
    {
        public Type ElementType
        {
            get { throw new NotImplementedException(); }
        }

        public System.Linq.Expressions.Expression Expression
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryProvider Provider
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class PrimaryKeyAttribute : Attribute
    {
    }

    public class IgnoreAttribute : Attribute
    {
    }

    public class WovenAttribute : Attribute
    {
    }
}
