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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Realms
{
    public abstract class ManagedAccessor
    {
    }

    public class RealmObject : RealmObjectBase
    {
    }

    public class EmbeddedObject : RealmObjectBase
    {
    }

    public class AsymmetricObject : RealmObjectBase
    {
    }

    public interface IRealmObjectBase
    {
    }

    public interface IRealmObject : IRealmObjectBase
    {
    }

    public interface IEmbeddedObject : IRealmObjectBase
    {
    }

    public abstract class RealmObjectBase : IRealmObjectBase, INotifyPropertyChanged
    {
        public List<string> LogList = new List<string>();

        public event PropertyChangedEventHandler? PropertyChanged;

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
            return default!;
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

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
