// ***********************************************************************
// Copyright (c) 2017 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using NUnit.Framework.Interfaces;
using Xamarin.Forms;

namespace NUnit.Runner.Extensions
{
    internal static class XamarinExtensions
    {
        /// <summary>
        /// Gets the color to display for the test status
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Color Color(this ResultState result)
        {
            switch (result.Status)
            {
                case TestStatus.Passed:
                    return Xamarin.Forms.Color.Green;
                case TestStatus.Skipped:
                    return Xamarin.Forms.Color.FromRgb(206, 172, 0);    // Dark Yellow
                case TestStatus.Failed:
                    if (result == ResultState.Failure)
                        return Xamarin.Forms.Color.Red;
                    if (result == ResultState.NotRunnable)
                        return Xamarin.Forms.Color.FromRgb(255, 106, 0);  // Orange

                    return Xamarin.Forms.Color.FromRgb(170, 0, 0); // Dark Red
                    
                case TestStatus.Inconclusive:
                default:
                    return Xamarin.Forms.Color.Gray;
            }
        }
    }
}