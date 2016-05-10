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

namespace Realms {

/// <summary>
/// Exception when you can't open an existing realm file because the format differs from your current class declarations.
/// </summary>
/// <remarks>
/// Typically triggered when you open the same Realm name, or use GetInstance() with no name, 
    /// and don't delete old files. <seealso href="https://realm.io/docs/xamarin/latest/#migrations">Read more at Migrations.</seealso>
/// </remarks>
public class RealmMigrationNeededException : RealmFileAccessErrorException {

    internal RealmMigrationNeededException(String message) : base(message)
    {
    }
}

} // namespace Realms
