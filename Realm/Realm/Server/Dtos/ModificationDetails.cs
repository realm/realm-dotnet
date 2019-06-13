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

namespace Realms.Server
{
    internal class ModificationDetails : IModificationDetails
    {
        private readonly Func<int, dynamic> _previousGetter;
        private readonly Func<int, dynamic> _currentGetter;

        public int PreviousIndex { get; }

        public int CurrentIndex { get; }

        public dynamic PreviousObject => _previousGetter(PreviousIndex);

        public dynamic CurrentObject => _currentGetter(CurrentIndex);

        internal ModificationDetails(int previousIndex, int currentIndex, Func<int, dynamic> previousGetter, Func<int, dynamic> currentGetter)
        {
            PreviousIndex = previousIndex;
            CurrentIndex = currentIndex;
            _previousGetter = previousGetter;
            _currentGetter = currentGetter;
        }
    }
}
