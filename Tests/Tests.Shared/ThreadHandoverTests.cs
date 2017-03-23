////////////////////////////////////////////////////////////////////////////
//
// Copyright 2017 Realm Inc.
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

using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Realms;

namespace IntegrationTests.Shared
{
    [TestFixture, Preserve(AllMembers = true)]
    public class ThreadHandoverTests : RealmInstanceTest
    {
        [Test]
        public void ObjectHandover_ShouldWork()
        {
            AsyncContext.Run(async () =>
            {
                var obj = new IntPropertyObject
                {
                    Int = 12
                };

                _realm.Write(() => _realm.Add(obj));

                var objReference = ThreadSafeReference.Create(obj);

                await Task.Run(() =>
                {
                    var otherRealm = Realm.GetInstance(_realm.Config);
                    var otherObj = otherRealm.ResolveReference(objReference);

                    Assert.That(otherObj.IsManaged);
                    Assert.That(otherObj.IsValid);
                    Assert.That(otherObj.Int, Is.EqualTo(12));
                });
            });
        }
    }
}
