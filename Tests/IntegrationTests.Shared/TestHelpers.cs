/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System.IO;
using Realms;

#if __IOS__
using Foundation;
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
            string sourceDir = "";
            #if __IOS__
            sourceDir = NSBundle.MainBundle.BundlePath;
            #endif
            //TODO add cases for Android and Windows setting sourcedir for bundled files
            destPath = RealmConfiguration.PathToRealm(destPath);  // any relative subdir or filename works
            File.Copy(Path.Combine(sourceDir, realmName), destPath, overwrite);
        }

    }
}
