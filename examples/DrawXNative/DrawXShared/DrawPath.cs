////////////////////////////////////////////////////////////////////////////
//
// Copyright 2014 Realm Inc.
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

using Realms;
using SkiaSharp;
using System.Collections.Generic;

namespace DrawXShared
{
    public class DrawPath : RealmObject
    {
        public string drawerID {get; set;}
        public string color {get;set;}
        public IList<DrawPoint> points {get;}
        public SKPath path { get {
                return null;
            }}

        // raw field used just to count known points - see RealmDraw.cs explanation
        public int NumPointsDrawnLocally = 0;

    }
}
