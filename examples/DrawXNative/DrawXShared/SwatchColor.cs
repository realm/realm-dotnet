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

using System.Collections.Generic;
using SkiaSharp;

namespace DrawXShared
{
    public struct SwatchColor
    {
        public string Name;
        public SKColor Color;
        private static Dictionary<string, SwatchColor> _colors;
        private static List<SwatchColor> _indexedColors;

        public static Dictionary<string, SwatchColor> ColorsByName
        {
            get
            {
                if (_colors == null)
                {
                    InitColors();
                }

                return _colors;
            }
        }

        // iterating this list gives you the colours in our desired order
        public static List<SwatchColor> Colors
        {
            get
            {
                if (_colors == null) 
                {
                    InitColors();
                }

                return _indexedColors;
            }
        }

        private static void InitColors()
        {
            _indexedColors = new List<SwatchColor> 
            {
                new SwatchColor { Name = "Charcoal", Color = new SKColor(28, 35, 63) },
                new SwatchColor { Name = "Elephant", Color = new SKColor(154, 155, 165) },
                new SwatchColor { Name = "Dove", Color = new SKColor(235, 235, 242) },
                new SwatchColor { Name = "Ultramarine", Color = new SKColor(57, 71, 127) },
                new SwatchColor { Name = "Indigo", Color = new SKColor(89, 86, 158) },
                new SwatchColor { Name = "GrapeJelly", Color = new SKColor(154, 80, 165) },
                new SwatchColor { Name = "Mulberry", Color = new SKColor(211, 76, 163) },
                new SwatchColor { Name = "Flamingo", Color = new SKColor(242, 81, 146) },
                new SwatchColor { Name = "SexySalmon", Color = new SKColor(247, 124, 136) },
                new SwatchColor { Name = "Peach", Color = new SKColor(252, 159, 149) },
                new SwatchColor { Name = "Melon", Color = new SKColor(252, 195, 151) }
            };
            _colors = new Dictionary<string, SwatchColor>();
            foreach (var color in _indexedColors)
            {
                _colors[color.Name] = color;
            }
        }
    }
}
