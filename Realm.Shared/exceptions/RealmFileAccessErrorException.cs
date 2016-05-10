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

///<summary>
/// Base for catching exceptions with Realm files, typically problems from which an app would recover.</summary>
///<remarks>
///You can catch any of the subclasses independently but any File-level 
///error which could be handled by an application descends from these.
///</remarks>
public class RealmFileAccessErrorException :  RealmException {
    internal RealmFileAccessErrorException(String message) : base(message)
    {
    }
}

} // namespace Realms
