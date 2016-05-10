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
using System.ComponentModel;

// Implement equivalents of extensions only available in .NET 4.6
[EditorBrowsable(EditorBrowsableState.Never)]
internal static class DateTimeOffsetExtensions
{
    private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

    internal static long ToRealmUnixTimeMilliseconds(this DateTimeOffset @this)
    {
        return Convert.ToInt64((@this.ToUniversalTime() - UnixEpoch).TotalMilliseconds);
    }

    internal static DateTimeOffset FromRealmUnixTimeMilliseconds( Int64 ms)
    {
        return UnixEpoch.AddMilliseconds(ms);
    }
}

