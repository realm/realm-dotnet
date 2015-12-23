using System;
using System.IO;

namespace Realms
{

    /// <summary>
    /// Realm configuration specifying settings that affect your Realm behaviour.
    /// </summary>
    public class RealmConfiguration
    {
        /// <summary>
        /// Standard filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <returns>A string representing a filename only, no path.</returns>      
        public static string DEFAULT_REALM_NAME  => "default.realm";

        /// <summary>
        /// Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.
        /// </summary>
        public readonly bool ShouldDeleteIfMigrationNeeded;

        /// <summary>
        /// The full path of any realms opened with this configuration, may be overriden by passing in a separate name.
        /// </summary>
        public string DatabasePath {get; private set;}

        /// <summary>
        /// Configuration you can override which is used when you create a new Realm without specifying a configuration.
        /// </summary>
        public static RealmConfiguration DefaultConfiguration { set; get;} = new RealmConfiguration();

        /// <summary>
        /// Constructor allowing path override.
        /// </summary
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <param name="shouldDeleteIfMigrationNeeded">Optional Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.</param> 
        public RealmConfiguration(string optionalPath = null, bool shouldDeleteIfMigrationNeeded=false)
        {
            ShouldDeleteIfMigrationNeeded = shouldDeleteIfMigrationNeeded;
            if (string.IsNullOrEmpty(optionalPath)) {
                DatabasePath = Path.Combine (
                    System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
                    DEFAULT_REALM_NAME);
            }
            else {
                if (!Path.IsPathRooted (optionalPath)) {
                    optionalPath = Path.Combine (
                        System.Environment.GetFolderPath (Environment.SpecialFolder.Personal), 
                        optionalPath);
                }
                if (optionalPath[optionalPath.Length-1] == Path.DirectorySeparatorChar)
                    optionalPath = Path.Combine (optionalPath, DEFAULT_REALM_NAME);
                DatabasePath = optionalPath;
            }
        }

        /// <summary>
        /// Clone method allowing you to override or customise the current path.
        /// </summary>
        /// <returns>An object with a fully-specified path.</returns>
        /// <param name="newConfigPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        public RealmConfiguration ConfigWithPath(string newConfigPath)
        {
            RealmConfiguration ret = (RealmConfiguration)MemberwiseClone();
            if (!string.IsNullOrEmpty(newConfigPath)) {
                if (Path.IsPathRooted (newConfigPath))
                    ret.DatabasePath = newConfigPath;
                else {
                    var usWithoutFile = Path.GetDirectoryName (DatabasePath);
                    if (newConfigPath[newConfigPath.Length - 1] == Path.DirectorySeparatorChar)
                        newConfigPath = Path.Combine (newConfigPath, DEFAULT_REALM_NAME);
                    ret.DatabasePath = Path.Combine (usWithoutFile, newConfigPath);
                }
            }
            return ret;
        }

    }  // class RealmConfiguration
}  // namespace Realms

