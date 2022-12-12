////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Realms
{
    /*
    public interface ISettableManagedAccessor
    {
        void SetManagedAccessor(IRealmAccessor accessor, IRealmObjectHelper helper = null, bool update = false, bool skipDefaults = false);
    }

    public interface IRealmObjectHelper
    { }

   */
    public class RealmObject : RealmObjectBase
    {
    }

    public class EmbeddedObject : RealmObjectBase
    {
    }

    public class AsymmetricObject : RealmObjectBase
    {
    }


    public interface IRealmObjectBase { } /*: ISettableManagedAccessor
    {
        IRealmAccessor Accessor { get; }

        bool IsManaged { get; }

        bool IsValid { get; }

        bool IsFrozen { get; }

        Realm Realm { get; }

        Schema.ObjectSchema ObjectSchema { get; }

        DynamicObjectApi DynamicApi { get; }

        int BacklinksCount { get; }

    }
*/
    public interface IRealmObject: IRealmObjectBase
    { 
    }

    public interface IEmbeddedObject: IRealmObjectBase
    {
        IRealmObjectBase Parent { get; }
    }

    public interface IAsymmetricObject : IRealmObjectBase
    {
    }
    
    public abstract class RealmObjectBase : IRealmObjectBase, INotifyPropertyChanged
    {
        //void ISettableManagedAccessor.SetManagedAccessor(IRealmAccessor accessor, IRealmObjectHelper helper, bool update, bool skipDefaults)
        //{ }

        public List<string> LogList = new List<string>();

        public event PropertyChangedEventHandler PropertyChanged;

        //IRealmAccessor IRealmObjectBase.Accessor => default;

        public bool Accessor { get; }

        public bool IsValid { get; }

        public bool IsFrozen { get; }

        public Schema.ObjectSchema ObjectSchema { get; }

        public DynamicObjectApi DynamicApi { get; }

        public int BacklinksCount { get; }

        private void LogString(string s)
        {
            LogList.Add(s);
        }

        private void LogCall(string parameters = "", [CallerMemberName] string caller = "")
        {
            LogString("RealmObject." + caller + "(" + parameters + ")");
        }

        private bool _isManaged;

        public bool IsManaged
        {
            get
            {
                LogString("IsManaged");
                return _isManaged;
            }

            set
            {
                _isManaged = value;
            }
        }

        private readonly Realm _realm = new Realm();

        public Realm Realm
        {
            get
            {
                LogString("Realm");
                return _realm;
            }
        }

        protected void SetValue(string propertyName, RealmValue value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected void SetValueUnique(string propertyName, RealmValue value)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected RealmValue GetValue(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new RealmValue();
        }

        protected IList<T> GetListValue<T>(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new RealmList<T>();
        }

        protected ISet<T> GetSetValue<T>(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return new RealmSet<T>();
        }

        protected void SetListValue<T>(string propertyName, IList<T> value) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected T GetObjectValue<T>(string propertyName) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return default(T);
        }

        protected void SetObjectValue<T>(string propertyName, T value) where T : RealmObject
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\", {nameof(value)} = {value}");
        }

        protected IQueryable<T> GetBacklinks<T>(string propertyName)
        {
            LogCall($"{nameof(propertyName)} = \"{propertyName}\"");
            return Enumerable.Empty<T>().AsQueryable();
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //private void SubscribeForNotifications()
        //{ }

        //private void UnsubscribeFromNotifications()
        //{ }
    }


    
    public abstract class ManagedAccessor
    {
        public TypeInfo GetTypeInfo(IRealmObjectBase obj)
        { return null; }
    }

  /*  public abstract class UnmanagedAccessor : IRealmAccessor
    {
        public bool IsManaged => default;

        public bool IsValid => default;

        public bool IsFrozen => default;

        public Realm Realm => default;

        public virtual Schema.ObjectSchema ObjectSchema => default;

        public int BacklinksCount => default;

        public DynamicObjectApi DynamicApi => throw new NotSupportedException("Using the dynamic API to access a RealmObject is only possible for managed (persisted) objects.");

        public IRealmObjectBase GetParent() => null;

        public IQueryable<T> GetBacklinks<T>(string propertyName)
            where T : IRealmObjectBase
            => throw new NotSupportedException("Using the GetBacklinks is only possible for managed (persisted) objects.");

        public abstract IDictionary<string, TValue> GetDictionaryValue<TValue>(string propertyName);

        public abstract IList<T> GetListValue<T>(string propertyName);

        public abstract ISet<T> GetSetValue<T>(string propertyName);

        public abstract RealmValue GetValue(string propertyName);

        public abstract void SetValue(string propertyName, RealmValue val);

        public abstract void SetValueUnique(string propertyName, RealmValue val);

        public void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate)
        { }

        public void UnsubscribeFromNotifications()
        { }

        public UnmanagedAccessor(Type objectType)
        { }

        public TypeInfo GetTypeInfo(IRealmObjectBase obj) => default;
    }

    public interface IRealmAccessor
    {
        bool IsManaged { get; }

        bool IsValid { get; }

        bool IsFrozen { get; }

        Realm Realm { get; }

        Schema.ObjectSchema ObjectSchema { get; }

        int BacklinksCount { get; }

        IRealmObjectBase GetParent();

        DynamicObjectApi DynamicApi { get; }

        void SubscribeForNotifications(Action<string> notifyPropertyChangedDelegate);

        void UnsubscribeFromNotifications();
    }

  
    */
    public struct DynamicObjectApi
    {
    }

    namespace Schema
    {
        public class ObjectSchema
        {

            public enum ObjectType : byte
            {
                RealmObject = 0,
                EmbeddedObject = 1,
                AsymmetricObject = 2,
            }

            public class Builder
            {
                public Builder(string name, ObjectType schemaType)
                { }
            }
        }
    }

    public class InvalidObject
    {

    }
}
