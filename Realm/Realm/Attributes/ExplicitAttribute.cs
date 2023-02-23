////////////////////////////////////////////////////////////////////////////
//
// Copyright 2020 Realm Inc.
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

namespace Realms
{
    /// <summary>
    /// An attribute that prevents the decorated class from being included in Realm's default schema.
    /// </summary>
    /// <remarks>
    /// If applied at the assembly level, then all classes in that assembly will be considered explicit and will not be added to
    /// the default schema. To include explicit classes in a Realm's schema, you should include them in
    /// <see cref="RealmConfigurationBase.Schema"/>:
    /// <code>
    /// var config = new RealmConfiguration
    /// {
    ///     Schema = new[] { typeof(MyExplicitClass) }
    /// };
    ///
    /// var realm = Realm.GetInstance(config);
    /// </code>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class ExplicitAttribute : Attribute
    {
    }
}
