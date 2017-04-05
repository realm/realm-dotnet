// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
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

using NUnit.Runner.Messages;
using NUnit.Runner.ViewModel;
using Xamarin.Forms;

namespace NUnit.Runner.View
{
    /// <summary>
    /// The main Xamarin.Forms view of the application
    /// </summary>
	public partial class SummaryView : ContentPage
	{
        SummaryViewModel _model;

        internal SummaryView (SummaryViewModel model)
		{
            _model = model;
		    _model.Navigation = Navigation;
		    BindingContext = _model;
			InitializeComponent();

            MessagingCenter.Subscribe<ErrorMessage>(this, ErrorMessage.Name, error => {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert("Error", error.Message, "OK"));
            });
        }

        /// <summary>
        /// Called when the view is appearing
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _model.OnAppearing();
        }
    }
}
