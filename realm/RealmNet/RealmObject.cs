using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace RealmNet
{
    public class RealmObject
    {
        private Realm _realm;  // TODO - Andy thinks we can drop this member
        private ICoreProvider _coreProvider;
        private long _rowIndex;

        // TODO - debate over isValid (Java) vs invalidated (Swift) and triple-state of standalone vs in realm vs formerly in realm and deleted
        public bool IsStandalone { get { return _coreProvider != null && _coreProvider.GetType() == typeof(StandaloneCoreProvider); } }


        public bool InRealm { get { return _coreProvider != null && _coreProvider.GetType () != typeof(StandaloneCoreProvider); } }


        protected RealmObject()
        {
            var modelName = GetType().Name;

            if (!GetType().GetTypeInfo().GetCustomAttributes(typeof(WovenAttribute), true).Any())
                Debug.WriteLine("WARNING! The type " + modelName + " is a RealmObject but it has not been woven.");


            // TODO consider a more direct approach where we just grab tha active coreProvider for this thread
            // so creating objects gets lighter
            var realmInTransaction = Realm.RealmWithActiveTransactionThisTread();
            if (realmInTransaction == null)
                Realm.AdoptNewObject (this, null, StandaloneCoreProvider.GetInstance (), null);
            else if (realmInTransaction.State == RealmNet.Interop.TransactionState.Write)
                realmInTransaction.AdoptNewObject (this);
            else {
                // TODO bind a newly created read somehow to LINQ operation????
            }
        }


        public void _Manage(Realm realm, ICoreProvider coreProvider, long rowIndex)
        {
            _realm = realm;
            //TODO copies properties from object to core provider BEFORE replacing with incoming
            _coreProvider = coreProvider;
            _rowIndex = rowIndex;
        }


        protected T GetValue<T>(string propertyName)
        {
#if DEBUG
            var isRealmObject = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmObject).GetTypeInfo());
            //ASD remove soon var isRealmList = IsAssignableFrom(typeof(T).GetTypeInfo(), typeof(RealmList<>).GetTypeInfo());

            //Debug.WriteLine("Getting " + typeof(T).Name + " value for " + tableName + "[" + _rowIndex + "]." + propertyName);
            //ASD remove soon  if (isRealmList) Debug.WriteLine("It's a realm list");
            if (isRealmObject) Debug.WriteLine("It's a realm object");
#endif
            return _coreProvider.GetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            _coreProvider.SetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex, value);
        }



        protected IList<T> GetListValue<T>(string propertyName)
        {
            var ret = _coreProvider.GetListValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex);
            if (ret == null)
            {
                ret = new RealmRelatedList<T>(this);  // need an empty list so things like Add can be called on it
                SetListValue<T>(propertyName, ret);
            }
            return ret;
        }

        protected void SetListValue<T>(string propertyName, IList<T> value)
        {
            _coreProvider.SetListValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowIndex, value);
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
