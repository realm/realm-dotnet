using Foundation;
using System;
using UIKit;
//using Realms;
using DrawXShared;

namespace DrawX.iOS
{

    //TODO enable Login button only if enter text in all three
    //TODO handle return key to move between fields and trigger launch (and change storyboard settings on fields if do so)
    public partial class LoginViewController : UIViewController
    {

        public Action<bool> OnCloseLogin {get;set;}  // caller should set so can use to dismiss

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var s = DrawXSettingsManager.Settings;
            ServerEntry.Text = s.ServerIP;
            UsernameEntry.Text = s.Username;
            PasswordEntry.Text = s.Password;

            LoginButton.TouchUpInside += (sender, e) =>
            {
                bool changedServer = DrawXSettingsManager.UpdateCredentials(ServerEntry.Text,  UsernameEntry.Text, PasswordEntry.Text);
                // TODO handle failure to login
                OnCloseLogin(changedServer);
            };

            CancelButton.TouchUpInside += (sender, e) =>
            {
                OnCloseLogin(false);
            };
        }
    }
}