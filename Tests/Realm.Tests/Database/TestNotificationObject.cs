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

#if TEST_WEAVER
using TestAsymmetricObject = Realms.AsymmetricObject;
using TestEmbeddedObject = Realms.EmbeddedObject;
using TestRealmObject = Realms.RealmObject;
#else
using TestAsymmetricObject = Realms.IAsymmetricObject;
using TestEmbeddedObject = Realms.IEmbeddedObject;
using TestRealmObject = Realms.IRealmObject;
#endif
using System.Collections;
using System.Collections.Generic;

namespace Realms.Tests.Database
{
    public partial class TestNotificationObject : TestRealmObject
    {
        // Automatically implemented (overridden) properties
        public string StringProperty { get; set; }

        public IList<TestNotificationObject> ListProperty { get; }

        public ISet<TestNotificationObject> SetProperty { get; }

        public IDictionary<string, TestNotificationObject> DictionaryProperty { get; }

        public TestNotificationObject SameTypeLinkProperty { get; set; }

        public Person DifferentTypeLinkProperty { get; set; }
    }
}
