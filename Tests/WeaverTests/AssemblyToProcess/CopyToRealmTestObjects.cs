﻿////////////////////////////////////////////////////////////////////////////
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
using System.Diagnostics.CodeAnalysis;
using Realms;

namespace AssemblyToProcess
{
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class NonNullableProperties : RealmObject
    {
        public string String { get; set; }

        public char Char { get; set; }

        public byte Byte { get; set; }

        public short Int16 { get; set; }

        public int Int32 { get; set; }

        public long Int64 { get; set; }

        public float Single { get; set; }

        public double Double { get; set; }

        public DateTimeOffset DateTimeOffset { get; set; }

        public bool Boolean { get; set; }

        public byte[] ByteArray { get; set; }
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass")]
    public class NullableProperties : RealmObject
    {
        public char? Char { get; set; }

        public byte? Byte { get; set; }

        public short? Int16 { get; set; }

        public int? Int32 { get; set; }

        public long? Int64 { get; set; }

        public float? Single { get; set; }

        public double? Double { get; set; }

        public DateTimeOffset? DateTimeOffset { get; set; }

        public bool? Boolean { get; set; }
    }
}