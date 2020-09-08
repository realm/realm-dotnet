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
using System.IO;
using System.Linq;

namespace Realms.Tests
{
    [Preserve(AllMembers = true)]
    public abstract class RealmInstanceTest : RealmTest
    {
        protected RealmConfiguration _configuration;

        private Lazy<Realm> _lazyRealm;

        protected Realm _realm => _lazyRealm.Value;

        protected void FreezeInPlace(RealmObjectBase obj)
        {
            obj.FreezeInPlace();
            CleanupOnTearDown(obj.Realm);
        }

        protected virtual RealmConfiguration CreateConfiguration(string path) => new RealmConfiguration(path);

        protected T Freeze<T>(T obj)
            where T : RealmObjectBase
        {
            var result = obj.Freeze();
            CleanupOnTearDown(result.Realm);
            return result;
        }

        protected IRealmCollection<T> Freeze<T>(IRealmCollection<T> collection)
            where T : RealmObjectBase
        {
            var result = collection.Freeze();
            CleanupOnTearDown(result.Realm);
            return result;
        }

        protected IList<T> Freeze<T>(IList<T> list)
            where T : RealmObjectBase
        {
            var result = list.Freeze();
            CleanupOnTearDown(result.AsRealmCollection().Realm);
            return result;
        }

        protected IQueryable<T> Freeze<T>(IQueryable<T> query)
            where T : RealmObjectBase
        {
            var result = query.Freeze();
            CleanupOnTearDown(result.AsRealmCollection().Realm);
            return result;
        }

        protected override void CustomSetUp()
        {
            _configuration = CreateConfiguration(Path.GetTempFileName());
            _lazyRealm = new Lazy<Realm>(() => GetRealm(_configuration));
            base.CustomSetUp();
        }
    }
}
