/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Reflection;
using System.IO;
using Realms;

#if __IOS__
using Foundation;
#endif


namespace IntegrationTests
{
    public static class TestHelpers
    {
        private static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            PropertyInfo propInfo = null;
            do
            {
                propInfo = type.GetTypeInfo().GetDeclaredProperty(propertyName);
                type = type.GetTypeInfo().BaseType;
            }
            while (propInfo == null && type != null);
            return propInfo;
        }

        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            var propInfo = GetPropertyInfo(obj.GetType(), propertyName);
            return (T)propInfo.GetValue(obj, null);
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
