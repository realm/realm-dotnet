using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RealmIO
{
    public class RealmObject
    {
        private ICoreRow _coreRow;

        protected RealmObject()
        {
            var modelName = GetType().Name;

            if (!GetType().GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");
        }

        internal void _Manage(ICoreRow coreRow)
        {
            _coreRow = coreRow;
        }

        protected T GetValue<T>(string propertyName)
        {
            var tableName = GetType().Name;
            var isRealmObject = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmObject).GetTypeInfo());
            var isRealmList = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmList<>).GetTypeInfo());
            
            //Debug.WriteLine("Getting " + typeof(T).Name + " value for " + tableName + "[" + _rowIndex + "]." + propertyName);
            if (isRealmList) Debug.WriteLine("It's a realm list");
            if (isRealmObject) Debug.WriteLine("It's a realm object");

            if (_coreRow != null)
            {
                return _coreRow.GetValue<T>(propertyName);
                //return _managingRealm.GetValue<T>(tableName, _rowIndex, propertyName);
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
            //Debug.WriteLine("Setting " + typeof(T).Name + " value for " + tableName + "[" + _rowIndex + "]." + propertyName + " to " + value.ToString());

            if (_coreRow != null)
            {
                _coreRow.SetValue<T>(propertyName, value);
                //_managingRealm.SetValue<T>(tableName, _rowIndex, propertyName, value);
            }
            else
            {
                Debug.WriteLine("Pre-management. Storing in memory");
            }
        }

        private static bool IsAssignableFrom(TypeInfo extendType, TypeInfo baseType)
        {
            while (!baseType.IsAssignableFrom(extendType))
            {
                if (extendType.Equals(typeof(object).GetTypeInfo()))
                    return false;

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
