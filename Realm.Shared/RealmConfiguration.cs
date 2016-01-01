using System;
using System.IO;

// see internals/RealmConfigurations.md for a detailed diagram of how this interacts with the ObjectStore configuration

namespace Realms
{

    /// <summary>
    /// Realm configuration specifying settings that affect your Realm behaviour.
    /// </summary>
    /// <remarks>
    /// Main role is generating a canonical path from whatever absolute, relative subdir or just filename user supplies.
    /// </remarks>
    public class RealmConfiguration
    {
        /// <summary>
        /// Standard filename to be combined with the platform-specific document directory.
        /// </summary>
        /// <value>A string representing a filename only, no path.</value>      
        public static string DefaultRealmName  => "default.realm";

        /// <summary>
        /// Constant used for SchemaVersion to indicate is not versioned.
        /// </summary>
        /// <remarks>
        /// Must be maintained to match an internal ObjectStore::NotVersioned.
        /// </remarks>
        /// <value>Maximum value of UInt64.</value>
        public static UInt64 NotVersioned => UInt64.MaxValue;

        /// <summary>
        /// Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.
        /// </summary>
        public readonly bool ShouldDeleteIfMigrationNeeded;

        /// <summary>
        /// The full path of any realms opened with this configuration, may be overriden by passing in a separate name.
        /// </summary>
        public string DatabasePath {get; private set;}

        /// <summary>
        /// Utility to build a path in which a realm will be created so can consistently use filenames and relative paths.
        /// </summary>
        public static string PathToRealm(string optionalPath = null)
        {
            if (string.IsNullOrEmpty(optionalPath)) {
                return Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.Personal), DefaultRealmName);
            }
            if (!Path.IsPathRooted (optionalPath)) {
                optionalPath = Path.Combine (Environment.GetFolderPath(Environment.SpecialFolder.Personal), optionalPath);
            }
            if (optionalPath[optionalPath.Length-1] == Path.DirectorySeparatorChar)   // ends with dir sep
                optionalPath = Path.Combine (optionalPath, DefaultRealmName);
             return optionalPath;
        }

        /// <summary>
        /// Number indicating the version, can be used to arbitrarily distinguish between schemas even if they have the same objects and properties.
        /// </summary>
        /// <value>0-based value initially set to indicate user is not versioning.</value>
        public UInt64 SchemaVersion { get; set;} = RealmConfiguration.NotVersioned;

        /// <summary>
        /// Configuration you can override which is used when you create a new Realm without specifying a configuration.
        /// </summary>
        public static RealmConfiguration DefaultConfiguration { set; get;} = new RealmConfiguration();

        /// <summary>
        /// Constructor allowing path override.
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <param name="shouldDeleteIfMigrationNeeded">Optional Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.</param> 
        public RealmConfiguration(string optionalPath = null, bool shouldDeleteIfMigrationNeeded=false)
        {
            ShouldDeleteIfMigrationNeeded = shouldDeleteIfMigrationNeeded;
            DatabasePath = PathToRealm(optionalPath);
        }

        /// <summary>
        /// Clone method allowing you to override or customise the current path.
        /// </summary>
        /// <returns>An object with a fully-specified, canonical path.</returns>
        /// <param name="newConfigPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        public RealmConfiguration ConfigWithPath(string newConfigPath)
        {
            RealmConfiguration ret = (RealmConfiguration)MemberwiseClone();
            string candidatePath;  // may need canonicalising
            if (!string.IsNullOrEmpty(newConfigPath)) {
                if (Path.IsPathRooted (newConfigPath))
                    candidatePath = newConfigPath;
                else {  // append a relative path, maybe just a relative subdir needing filename
                    var usWithoutFile = Path.GetDirectoryName (DatabasePath);
                    if (newConfigPath[newConfigPath.Length - 1] == Path.DirectorySeparatorChar) // ends with separator
                        newConfigPath = Path.Combine (newConfigPath, DefaultRealmName);  // add filename to relative subdir
                    candidatePath = Path.Combine (usWithoutFile, newConfigPath);
                }
                ret.DatabasePath = Path.GetFullPath(candidatePath);  // canonical version, removing embedded ../ and other relative artifacts
            }
            return ret;
        }

    }  // class RealmConfiguration
}  // namespace Realms

