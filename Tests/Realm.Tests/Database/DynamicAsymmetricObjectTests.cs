// ////////////////////////////////////////////////////////////////////////////
// //
// // Copyright 2022 Realm Inc.
// //
// // Licensed under the Apache License, Version 2.0 (the "License")
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// // http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.
// //
// ////////////////////////////////////////////////////////////////////////////

using System;
using NUnit.Framework;
using Realms.Dynamic;
using Realms.Tests.Sync;

namespace Realms.Tests.Database
{
    [TestFixture, Preserve(AllMembers = true)]
    public class DynamicAsymmetricObjectTests : SyncTestBase
    {
        // name format: Action_OptionalCondition_Expectation

        [TestCase(true)]
        [TestCase(false)]
        public void ReadWriteVarsInWriteTransaction(bool isDynamic)
        {
            SyncTestHelpers.RunBaasTestAsync(async () =>
            {
                var flxConfig = await GetFLXIntegrationConfigAsync();
                flxConfig.IsDynamic = isDynamic;
                flxConfig.Schema = new[] { typeof(AsymmetricObjectWithAllTypes) };
                using var realm = await GetFLXIntegrationRealmAsync(flxConfig: flxConfig);

                realm.Write(() =>
                {
                    var asymmetricObj = (AsymmetricObject)(object)realm.DynamicApi.CreateObject("AsymmetricObjectWithAllTypes", Guid.NewGuid());

                    if (isDynamic)
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<DynamicAsymmetricObject>());
                    }
                    else
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<AsymmetricObjectWithAllTypes>());
                    }

                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.CharProperty), 'F');
                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.NullableCharProperty), 'o');
                    asymmetricObj.DynamicApi.Set(nameof(AsymmetricObjectWithAllTypes.StringProperty), "o");

                    Assert.That(asymmetricObj.DynamicApi.Get<char>(nameof(AllTypesObject.CharProperty)), Is.EqualTo('F'));
                    Assert.That(asymmetricObj.DynamicApi.Get<char?>(nameof(AllTypesObject.NullableCharProperty)), Is.EqualTo('o'));
                    Assert.That(asymmetricObj.DynamicApi.Get<string>(nameof(AllTypesObject.StringProperty)), Is.EqualTo("o"));
                });

                if (TestHelpers.IsUnity)
                {
                    return;
                }

                realm.Write(() =>
                {
                    dynamic asymmetricObj = realm.DynamicApi.CreateObject("AsymmetricObjectWithAllTypes", Guid.NewGuid());
                    if (isDynamic)
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<DynamicAsymmetricObject>());
                    }
                    else
                    {
                        Assert.That(asymmetricObj, Is.InstanceOf<AsymmetricObjectWithAllTypes>());
                    }

                    asymmetricObj.CharProperty = 'F';
                    asymmetricObj.NullableCharProperty = 'o';
                    asymmetricObj.StringProperty = "o";

                    Assert.That((char)asymmetricObj.CharProperty, Is.EqualTo('F'));
                    Assert.That((char)asymmetricObj.NullableCharProperty, Is.EqualTo('o'));
                    Assert.That(asymmetricObj.StringProperty, Is.EqualTo("o"));
                });
            });
        }
    }
}
