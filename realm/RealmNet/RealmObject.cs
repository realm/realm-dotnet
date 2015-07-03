using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RealmNet
{
    public class RealmObject
    {
        private ICoreProvider _coreProvider;
        private long _rowIndex;

        protected RealmObject()
        {
            var modelName = GetType().Name;

            if (!GetType().GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");
        }

        public void _Manage(ICoreProvider coreProvider, long rowIndex)
        {
            _coreProvider = coreProvider;
            _rowIndex = rowIndex;
        }

        protected T GetValue<T>(string propertyName)
        {
            var tableName = GetType().Name;
            var isRealmObject = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmObject).GetTypeInfo());
            var isRealmList = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmList<>).GetTypeInfo());
            
            //Debug.WriteLine("Getting " + typeof(T).Name + " value for " + tableName + "[" + _rowIndex + "]." + propertyName);
            if (isRealmList) Debug.WriteLine("It's a realm list");
            if (isRealmObject) Debug.WriteLine("It's a realm object");

            if (_coreProvider != null)
            {
                return _coreProvider.GetValue<T>(tableName, propertyName, _rowIndex);
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

            if (_coreProvider != null)
            {
                _coreProvider.SetValue<T>(tableName, propertyName, _rowIndex, value);
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
