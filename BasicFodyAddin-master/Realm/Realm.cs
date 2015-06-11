using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RealmIO
{
    public class Realm
    {
        private readonly ICoreProvider _coreProvider;

        public Realm(ICoreProvider coreProvider) 
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
            var rowIndex = _coreProvider.InsertEmptyRow(objectType.Name);

            result._Manage(this, rowIndex);

            return result;
        }

        private void CreateTableFor(Type objectType)
        {
            var tableName = objectType.Name;

            if (objectType.GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Count() == 0)
                Debug.WriteLine("WARNING! The type " + tableName + " is a RealmObject but it has not been woven.");

            _coreProvider.AddTable(tableName);

            var propertiesToMap = objectType.GetTypeInfo().DeclaredProperties.Where(p => !p.CustomAttributes.Any(a => a.AttributeType == typeof(RealmIO.IgnoreAttribute)));
            foreach (var p in propertiesToMap)
            {
                var propertyName = p.Name;
                var mapToAttribute = p.CustomAttributes.FirstOrDefault(a => a.AttributeType == typeof(MapToAttribute));
                if (mapToAttribute != null)
                    propertyName = ((string)mapToAttribute.ConstructorArguments[0].Value);
                
                var columnType = p.PropertyType;
                _coreProvider.AddColumnToTable(tableName, propertyName, columnType);
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

        public RealmQuery<T> All<T>()
        {
            return new RealmQuery<T>(_coreProvider);
        }

        internal T GetValue<T>(string tableName, int rowIndex, string propertyName)
        {
            return _coreProvider.GetValue<T>(tableName, rowIndex, propertyName);
        }

        internal void SetValue<T>(string tableName, int rowIndex, string propertyName, T value)
        {
            _coreProvider.SetValue(tableName, rowIndex, propertyName, value);
        }
    }

}
