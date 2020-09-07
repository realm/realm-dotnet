////////////////////////////////////////////////////////////////////////////
//
// Copyright 2019 Realm Inc.
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
using Realms.Native;

namespace Realms.Server
{
    internal class ModificationDetails : IModificationDetails
    {
        private readonly Lazy<dynamic> _previous;
        private readonly Lazy<dynamic> _current;
        private readonly Lazy<ISet<string>> _changedProperties;

        public dynamic PreviousObject => _previous.Value;

        public dynamic CurrentObject => _current.Value;

        public ISet<string> ChangedProperties => _changedProperties.Value;

        internal ModificationDetails(Func<dynamic> previous, Func<dynamic> current, ColumnKey[] changedColumns, Func<ColumnKey[], ISet<string>> changedProperties)
        {
            _previous = new Lazy<dynamic>(previous);
            _current = new Lazy<dynamic>(current);
            _changedProperties = new Lazy<ISet<string>>(() => changedProperties(changedColumns));
        }
    }
}
