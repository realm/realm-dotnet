using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RealmIO
{
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
