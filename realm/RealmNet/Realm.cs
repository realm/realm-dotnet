using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using RealmNet.Interop;

namespace RealmNet
{
    public class Realm : IDisposable
    {
        public static ICoreProvider ActiveCoreProvider;

        public static Realm GetInstance(string path = null)
        {
            return new Realm(ActiveCoreProvider, path);
        }

        private readonly ICoreProvider _coreProvider;
        private ISharedGroupHandle _sharedGroupHandle;

        private Realm(ICoreProvider coreProvider, string path) 
        {
            _coreProvider = coreProvider;
            _sharedGroupHandle = coreProvider.CreateSharedGroup(path);
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
            var rowIndex = _coreProvider.AddEmptyRow(objectType.Name);

            result._Manage(_coreProvider, rowIndex);

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

        /// <summary>
        /// True if the c++ resources have been released
        /// True if dispose have been called one way or the other
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        public bool IsDisposed
        {
            get { return _sharedGroupHandle != null && _sharedGroupHandle.IsClosed; }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// Calling dispose will free any c++ structures created to keep track of the handled object.
        /// Dispose with no parametres is called by by the user indirectly via the using keyword, or directly by the user
        /// by him calling Displose()
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);//tell finalizer thread/GC it does not have to call finalize - we have already disposed of unmanaged resources
            //above is added in case someone inherits from a handled resource and implements a finalizer - the binding does not need the call, as the
            //binding does not introduce finalizers in any C# wrapper classes (only in the Handle classes via the finalizer in CriticalHandle
            //if we decide to make the user facing tightdb classes (table,tableview,group, sharedgroup etc. final, then we can save above call)
            //todo:Measure by test and by code inspection any performance gains from not calling SuppressFinalize(this) and making the classes final
        }

        //using a very simple dispose pattern as we will just call on to Handle.Dispose in both a finalizing and in a disposing situation
        //leaving this method in here so that classes derived from this one can implement a finalizer and have that finalizer call dispose(false)
        /// <summary>
        /// Override this if you have managed stuff that needs to be closed down when dispose is called
        /// </summary>
        /// <param name="disposing"></param>
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        protected virtual void Dispose(bool disposing)
        {
            if (_sharedGroupHandle != null && !IsDisposed)//handle could be null if we crashed in the constructor (group with filename to a OS protected area for instance)
            {
                //no matter if we are being called from a dispose in a user thread, or from a finalizer, we should
                //ask Handle to dispose of itself (unbind)
                _sharedGroupHandle.Dispose();
            }
        }
    }
}
