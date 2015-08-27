using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using RealmNet.Interop;

namespace RealmNet
{
    public class RealmObject
    {
        private Realm _realm;  // TODO - Andy thinks we can drop this member
        private ICoreProvider _coreProvider;
        private IRowHandle _rowHandle;

        // TODO - debate over isValid (Java) vs invalidated (Swift) and triple-state of standalone vs in realm vs formerly in realm and deleted
        public bool IsStandalone => _coreProvider is StandaloneCoreProvider;
        public bool InRealm => _rowHandle != null && !_rowHandle.IsInvalid && _rowHandle.IsAttached;

        internal IRowHandle RowHandle => _rowHandle;

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


        public void _Manage(Realm realm, ICoreProvider coreProvider, IRowHandle rowHandle)
        {
            _realm = realm;
            //TODO copies properties from object to core provider BEFORE replacing with incoming
            _coreProvider = coreProvider;
            _rowHandle = rowHandle;
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
            return _coreProvider.GetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowHandle);
        }

        protected void SetValue<T>(string propertyName, T value)
        {
            _coreProvider.SetValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowHandle, value);
        }



        protected RealmList<T> GetListValue<T>(string propertyName) where T : RealmObject
        {
            var ret = (RealmList <T>)_coreProvider.GetListValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowHandle);
            if (ret == null)
            {
                ret = new RealmList<T>();  // need an empty list so things like Add can be called on it
                SetListValue<T>(propertyName, ret);
            }
            return ret;
        }

        protected void SetListValue<T>(string propertyName, RealmList<T> value) where T : RealmObject
        {
            //ASD var dumpFor = typeof(T);
            _coreProvider.SetListValue<T>(_realm?.TransactionGroupHandle, GetType().Name, propertyName, _rowHandle, value);
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

        public override bool Equals(object p)
        {
            // If parameter is null, return false. 
            if (Object.ReferenceEquals(p, null))
            {
                return false;
            }

            // Optimization for a common success case. 
            if (Object.ReferenceEquals(this, p))
            {
                return true;
            }

            // If run-time types are not exactly the same, return false. 
            if (this.GetType() != p.GetType())
                return false;

            // Return true if the fields match. 
            // Note that the base class is not invoked because it is 
            // System.Object, which defines Equals as reference equality. 
            return RowHandle.Equals(((RealmObject)p).RowHandle);
        }
    }
}
