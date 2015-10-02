using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace RealmNet
{
    public class Realm
    {
        private static List<Type> _realmObjectClasses;

        static Realm()
        {
            _realmObjectClasses = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => typeof(RealmObject).IsAssignableFrom(t))
                .ToList();

            _realmObjectClasses.ForEach(realmType =>
            {
                if (!realmType.GetCustomAttributes(typeof(WovenAttribute), true).Any())
                    Debug.WriteLine("WARNING! The type " + realmType.Name + " is a RealmObject but it has not been woven.");
            });
        }

        public static Realm GetInstance(string databasePath)
        {
            throw new NotImplementedException();
        }

        internal Dictionary<Type, TableHandle> _tableHandles = new Dictionary<Type, TableHandle>();

        public Realm()
        {
            _realmObjectClasses.ForEach((realmType) => _tableHandles[realmType] = GetTable(realmType));
        }

        public IGroupHandle TransactionGroupHandle { get { throw new NotImplementedException(); } }

        private static TableHandle GetTable(Type realmType)
        {
            throw new NotImplementedException();
        }

        public T CreateObject<T>() where T : RealmObject
        {
            return (T)CreateObject(typeof(T));
        }

        public object CreateObject(Type objectType)
        {
            var result = (RealmObject)Activator.CreateInstance(objectType);

            var tableHandle = _tableHandles[objectType];
            
            var rowPtr = NativeTable.add_empty_row(tableHandle);
            var rowHandle = CreateRowHandle	(rowPtr);

            result._Manage(this, rowHandle);

            return result;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        private static RowHandle CreateRowHandle(IntPtr rowPtr)
        {
            var rowHandle = new RowHandle();

            RuntimeHelpers.PrepareConstrainedRegions();
            try { /* Retain handle in a constrained execution region */ }
            finally
            {
                rowHandle.SetHandle(rowPtr);
            }

            return rowHandle;
        }

        public IDisposable BeginWrite()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> All<T>() where T: RealmObject
        {
            throw new NotImplementedException();
        }

        public void Remove(RealmObject p2)
        {
            throw new NotImplementedException();
        }
    }
}
