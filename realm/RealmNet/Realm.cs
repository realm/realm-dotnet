using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RealmNet
{
    public class Realm
    {
        public static ICoreProvider ActiveCoreProvider;

        public static Realm GetInstance()
        {
            return new Realm(ActiveCoreProvider);
        }

        private readonly ICoreProvider _coreProvider;

        private Realm(ICoreProvider coreProvider) 
        {
            this._coreProvider = coreProvider;
        }

        public T CreateObject<T>() where T : RealmObject
        {
            return (T)CreateObject(typeof(T));
        }

        public object CreateObject(Type objectType)
        {
            if (!_coreProvider.HasTable(objectType.Name))
                CreateTableFor(objectType);

            var result = (RealmObject)Activator.CreateInstance(objectType);
            var coreRow = _coreProvider.AddEmptyRow(objectType.Name);

            result._Manage(coreRow);

            return result;
        }

        private void CreateTableFor(Type objectType)
        {
            var tableName = objectType.Name;

            if (!objectType.GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The type " + tableName + " is a RealmObject but it has not been woven.");

            _coreProvider.AddTable(tableName);

            var propertiesToMap = objectType.GetTypeInfo().DeclaredProperties.Where(p => p.CustomAttributes.All(a => a.AttributeType != typeof (IgnoreAttribute)));
            foreach (var p in propertiesToMap)
            {
                var propertyName = p.Name;
                var mapToAttribute = p.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(MapToAttribute));
                if (mapToAttribute != null)
                    propertyName = ((string)mapToAttribute.ConstructorArguments[0].Value);
                
                var columnType = p.PropertyType;
                _coreProvider.AddColumnToTable(tableName, propertyName, columnType);
            }
        }

        public RealmQuery<T> All<T>()
        {
            return new RealmQuery<T>(_coreProvider);
        }
    }
}
