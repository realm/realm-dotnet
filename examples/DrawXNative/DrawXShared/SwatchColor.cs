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

using SkiaSharp;
using System.Collections.Generic;
namespace DrawXShared
{
    public struct SwatchColor
    {
        public string name;
        public SKColor color;
        private static Dictionary<string, SwatchColor>  _colors;

        public static readonly SwatchColor Charcoal = new SwatchColor() { name = "Charcoal", color = new SKColor(28, 35, 63) };
        public static readonly SwatchColor Elephant = new SwatchColor() { name = "Elephant", color = new SKColor(154, 155, 165) };
        public static readonly SwatchColor Dove = new SwatchColor() { name = "Dove", color = new SKColor(235, 235, 242) };
        public static readonly SwatchColor Ultramarine = new SwatchColor() { name = "Ultramarine", color = new SKColor(57, 71, 127) };
        public static readonly SwatchColor Indigo = new SwatchColor() { name = "Indigo", color = new SKColor(89, 86, 158) };
        public static readonly SwatchColor GrapeJelly = new SwatchColor() { name = "GrapeJelly", color = new SKColor(154, 80, 165) };
        public static readonly SwatchColor Mulberry = new SwatchColor() { name = "Mulberry", color = new SKColor(211, 76, 163) };
        public static readonly SwatchColor Flamingo = new SwatchColor() { name = "Flamingo", color = new SKColor(242, 81, 146) };
        public static readonly SwatchColor SexySalmon = new SwatchColor() { name = "SexySalmon", color = new SKColor(247, 124, 136) };
        public static readonly SwatchColor Peach = new SwatchColor() { name = "Peach", color = new SKColor(252, 159, 149) };
        public static readonly SwatchColor Melon = new SwatchColor() { name = "Melon", color = new SKColor(252, 195, 151) };

        public static Dictionary<string, SwatchColor> colors
        {
            get
            {
                if (_colors == null)
                {
                    _colors = new Dictionary<string, SwatchColor> ();
                    _colors["Charcoal"] = Charcoal;
                    _colors["Elephant"] = Elephant;
                    _colors["Dove"] = Dove;
                    _colors["Ultramarine"] = Ultramarine;
                    _colors["Indigo"] = Indigo;
                    _colors["GrapeJelly"] = GrapeJelly;
                    _colors["Mulberry"] = Mulberry;
                    _colors["Flamingo"] = Flamingo;
                    _colors["SexySalmon"] = SexySalmon;
                    _colors["Peach"] = Peach;
                    _colors["Melon"] = Melon;
                }
                return _colors;
            }
        }
    }
}
