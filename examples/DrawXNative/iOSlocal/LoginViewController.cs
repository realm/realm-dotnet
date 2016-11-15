using Foundation;
using System;
using UIKit;
//using Realms;
using DrawXShared;

namespace DrawX.iOS
{
    public partial class LoginViewController : UIViewController
    {

        public UIViewController Invoker {get;set;}  // caller should set so can use to dismiss

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // quick fake editor just supplying hardcoded
            var s = DrawXSettingsManager.Settings;
            ServerEntry.Text = s.ServerIP;
            UsernameEntry.Text = s.Username;
            PasswordEntry.Text = s.Password;

            LoginButton.TouchUpInside += (sender, e) =>
            {
                DrawXSettingsManager.Write(() =>
                {
                    s.ServerIP = ServerEntry.Text;
                    s.Username = UsernameEntry.Text;
                    s.Password = PasswordEntry.Text;
                });
                Invoker?.DismissModalViewController(false);
            };

            CancelButton.TouchUpInside += (sender, e) =>
            {
                Invoker?.DismissModalViewController(false);
            };
        }
    }
}