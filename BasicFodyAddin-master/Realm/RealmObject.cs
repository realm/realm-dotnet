using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Diagnostics;
using TightDbCSharp;

namespace RealmIO
{
    public class Realm
    {
        private ICoreProvider coreProvider;

        public Realm(ICoreProvider coreProvider) 
        {
            this.coreProvider = coreProvider;
        }

        public T CreateObject<T>() where T : class, new()
        {
            return (T)CreateObject(typeof(T));
        }

        public object CreateObject(Type objectType)
        {
            if (!coreProvider.HasTable(objectType.Name))
                CreateTableFor(objectType);

            var result = (RealmObject)Activator.CreateInstance(objectType);
            var rowIndex = coreProvider.InsertEmptyRow(objectType.Name);

            result._Manage(this, rowIndex);

            return result;
        }

        private void CreateTableFor(Type objectType)
        {
            var tableName = objectType.Name;

            if (objectType.GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Count() == 0)
                Debug.WriteLine("WARNING! The type " + tableName + " is a RealmObject but it has not been woven.");

            coreProvider.AddTable(tableName);

            var propertiesToMap = objectType.GetTypeInfo().DeclaredProperties.Where(p => !p.CustomAttributes.Any(a => a.AttributeType == typeof(RealmIO.IgnoreAttribute)));
            foreach (var p in propertiesToMap)
            {
                var columnName = p.Name;
                var columnType = p.PropertyType;
                coreProvider.AddColumnToTable(tableName, columnName, columnType);
            }

            //using (var people = new Table(
            //    new StringColumn("name"),
            //    new IntColumn("age"),
            //    new BoolColumn("hired"),
            //    new SubTableColumn("phones", //sub table specification
            //        new StringColumn("desc"),
            //        new StringColumn("number"))))
            //{
            //    people.Add("John", 20, true,  new[]{new[] {"home",   "555-1234-555"}});
            //    //Debug.WriteLine(people.Size); //=>6
            //}
        }

        internal T GetValue<T>(string tableName, int rowIndex, string propertyName)
        {
            return coreProvider.GetValue<T>(tableName, rowIndex, propertyName);
        }

        internal void SetValue<T>(string tableName, int rowIndex, string propertyName, T value)
        {
            coreProvider.SetValue(tableName, rowIndex, propertyName, value);
        }
    }

    public class RealmObject
    {
        private Realm managingRealm;
        private int rowIndex;

        public RealmObject()
        {
            var modelName = GetType().Name;

            if (GetType().GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Count() == 0)
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");
        }

        internal void _Manage(Realm managingRealm, int rowIndex)
        {
            this.managingRealm = managingRealm;
            this.rowIndex = rowIndex;
        }

        public T GetValue<T>(string propertyName)
        {
            var tableName = GetType().Name;
            var isRealmObject = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmObject).GetTypeInfo());
            var isRealmList = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmList<>).GetTypeInfo());
            
            Debug.WriteLine("Getting " + typeof(T).Name + " value for " + tableName + "[" + rowIndex + "]." + propertyName);
            if (isRealmList) Debug.WriteLine("It's a realm list");
            if (isRealmObject) Debug.WriteLine("It's a realm object");

            if (managingRealm != null)
            {
                return managingRealm.GetValue<T>(tableName, rowIndex, propertyName);
            }
            else
            {
                Debug.WriteLine("Pre-management. Storing in memory");
            }
            return default(T);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            var tableName = GetType().Name;
            Debug.WriteLine("Setting " + typeof(T).Name + " value for " + tableName + "[" + rowIndex + "]." + propertyName + " to " + value.ToString());

            if (managingRealm != null)
            {
                managingRealm.SetValue<T>(tableName, rowIndex, propertyName, value);
            }
            else
            {
                Debug.WriteLine("Pre-management. Storing in memory");
            }
        }

        public static bool IsAssignableFrom(TypeInfo extendType, TypeInfo baseType)
        {
            while (!baseType.IsAssignableFrom(extendType))
            {
                if (extendType.Equals(typeof(object)))
                {
                    return false;
                }
                if (extendType.IsGenericType && !extendType.IsGenericTypeDefinition)
                {
                    extendType = extendType.GetGenericTypeDefinition().GetTypeInfo();
                }
                else
                {
                    extendType = extendType.BaseType.GetTypeInfo();
                }
            }
            return true;
        }
    }
}
