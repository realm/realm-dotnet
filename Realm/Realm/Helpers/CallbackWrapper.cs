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

using System;
using System.Diagnostics.CodeAnalysis;

namespace Realms
{
    internal class CallbackWrapper
    {
        public Exception ManagedException { get; set; }

        public static CallbackWrapper<T> Create<T>(T value) => new(value);
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "The top class is just a helper for the bottom one")]
    internal class CallbackWrapper<T> : CallbackWrapper
    {
        public T Value { get; }

        public CallbackWrapper(T value)
        {
            Value = value;
        }
    }
}
