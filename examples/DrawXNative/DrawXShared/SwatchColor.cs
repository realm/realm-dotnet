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
        private static SwatchColor[] _indexedColors;

        public static Dictionary<string, SwatchColor> colors
        {
            get
            {
                if (_colors == null)
                    InitColors();
                return _colors;
            }
        }

        public static SwatchColor Color(int i)
        {
            if (_colors == null)
                InitColors();
            return _indexedColors[i];
        }

        private static void InitColors()
        {
            _indexedColors = new SwatchColor[] {
                new SwatchColor() { name = "Charcoal", color = new SKColor(28, 35, 63) },
                new SwatchColor() { name = "Elephant", color = new SKColor(154, 155, 165) },
                new SwatchColor() { name = "Dove", color = new SKColor(235, 235, 242) },
                new SwatchColor() { name = "Ultramarine", color = new SKColor(57, 71, 127) },
                new SwatchColor() { name = "Indigo", color = new SKColor(89, 86, 158) },
                new SwatchColor() { name = "GrapeJelly", color = new SKColor(154, 80, 165) },
                new SwatchColor() { name = "Mulberry", color = new SKColor(211, 76, 163) },
                new SwatchColor() { name = "Flamingo", color = new SKColor(242, 81, 146) },
                new SwatchColor() { name = "SexySalmon", color = new SKColor(247, 124, 136) },
                new SwatchColor() { name = "Peach", color = new SKColor(252, 159, 149) },
                new SwatchColor() { name = "Melon", color = new SKColor(252, 195, 151) }
            };
            _colors = new Dictionary<string, SwatchColor> ();
            foreach (var color in _indexedColors)
            {
                _colors[color.name] = color;
            }
        }
    }
}
