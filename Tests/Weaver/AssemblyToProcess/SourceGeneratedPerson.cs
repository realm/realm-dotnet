////////////////////////////////////////////////////////////////////////////
//
// Copyright 2022 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License")
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
using Realms;
using AssemblyToProcess.Generated;

namespace AssemblyToProcess
{
    public partial class SourceGeneratedPerson : IRealmObject
    {
        public string Name { get; set; }

        public int Id { get; set; }

        [Ignored]
        public string Nickname { get; set; }
    }

    [Generated]
    public partial class SourceGeneratedPerson : IRealmObject
    {
        private ISourceGeneratedPersonAccessor _accessor;

        internal ISourceGeneratedPersonAccessor Accessor => _accessor = _accessor ?? new SourceGeneratedPersonAccessor();

        public List<string> LogList => (Accessor as SourceGeneratedPersonAccessor).LogList;
    }
}

namespace AssemblyToProcess.Generated
{
    internal interface ISourceGeneratedPersonAccessor
    {
        string Name { get; set; }

        int Id { get; set; }
    }

    internal class SourceGeneratedPersonAccessor : ISourceGeneratedPersonAccessor
    {
        private string _name;
        public string Name
        {
            get
            {
                LogString($"Get {nameof(Name)}");
                return _name;
            }
            set
            {
                LogString($"Set {nameof(Name)}");
                _name = value;
            }
        }

        private int _id;
        public int Id
        {
            get
            {
                LogString($"Get {nameof(Id)}");
                return _id;
            }
            set
            {
                LogString($"Set {nameof(Id)}");
                _id = value;
            }
        }

        public List<string> LogList = new List<string>();

        private void LogString(string s)
        {
            LogList.Add(s);
        }
    }
}
