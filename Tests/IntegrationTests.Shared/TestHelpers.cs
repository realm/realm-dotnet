/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.IO;
using Realms;

#if __IOS__
using Foundation;
using CoreFoundation;
#endif

namespace IntegrationTests
{
    public static class TestHelpers
    {
        public static object GetPropertyValue(object o, string propName)
        {
            return o.GetType().GetProperty(propName).GetValue(o, null);
        }

        public static void SetPropertyValue(object o, string propName, object propertyValue)
        {
            o.GetType().GetProperty(propName).SetValue(o, propertyValue);
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            return (T) GetPropertyValue(obj, propertyName); 
        }

        public static void CopyBundledDatabaseToDocuments(string realmName, string destPath=null, bool overwrite=true)
        {
            destPath = RealmConfiguration.PathToRealm(destPath);  // any relative subdir or filename works

            #if __ANDROID__
            using (var asset = Android.App.Application.Context.Assets.Open(realmName))
            using (var destination = File.OpenWrite(destPath))
            {
                asset.CopyTo(destination);
            }

            return;
            #endif

            string sourceDir = "";
            #if __IOS__
            sourceDir = NSBundle.MainBundle.BundlePath;
            #endif
            //TODO add cases for Windows setting sourcedir for bundled files

            File.Copy(Path.Combine(sourceDir, realmName), destPath, overwrite);
        }


        public static void RunEventLoop(TimeSpan duration)
        {
#if __IOS__
            CFRunLoop.Current.RunInMode(CFRunLoop.ModeDefault, duration.TotalSeconds, false);
#elif __ANDROID__
            int fd, events;
            IntPtr data;
            ALooper_pollAll((int)duration.TotalMilliseconds, out fd, out events, out data);
#else
            throw new NotImplementedException();
#endif
        }

#if __ANDROID__
        [System.Runtime.InteropServices.DllImport("android")]
        private static extern int ALooper_pollAll(int timeoutMillis, out int fd, out int events, out IntPtr data);
#endif
    }
}
