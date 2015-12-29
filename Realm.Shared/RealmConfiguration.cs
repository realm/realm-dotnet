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
        /// <returns>A string representing a filename only, no path.</returns>      
        public static string DefaultRealmName  => "default.realm";

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
        /// </summary>
        /// <param name="optionalPath">Path to the realm, must be a valid full path for the current platform, relative subdir, or just filename.</param>
        /// <param name="shouldDeleteIfMigrationNeeded">Optional Flag mainly to help with temp databases and testing, indicates content can be abandoned when you change the schema.</param> 
        public RealmConfiguration(string optionalPath = null, bool shouldDeleteIfMigrationNeeded=false)
        {
            ShouldDeleteIfMigrationNeeded = shouldDeleteIfMigrationNeeded;
            if (string.IsNullOrEmpty(optionalPath)) {
                DatabasePath = Path.Combine (
                    System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), 
                    DefaultRealmName);
            }
            else {
                if (!Path.IsPathRooted (optionalPath)) {
                    optionalPath = Path.Combine (
                        System.Environment.GetFolderPath (Environment.SpecialFolder.Personal), 
                        optionalPath);
                }
                if (optionalPath[optionalPath.Length-1] == Path.DirectorySeparatorChar)
                    optionalPath = Path.Combine (optionalPath, DefaultRealmName);
                DatabasePath = optionalPath;
            }
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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="Realms.RealmConfiguration"/>.
        /// </summary>
        /// <param name="rhs">The <see cref="System.Object"/> to compare with the current <see cref="Realms.RealmConfiguration"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="Realms.RealmConfiguration"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(Object rhs)
        {
            if (rhs == null)
                return false;
            return Equals(rhs as RealmConfiguration);
        }


        public bool Equals(RealmConfiguration rhs)
        {
            if (rhs == null)
                return false;
            if (GC.ReferenceEquals(this, rhs))
                return true;
            return ShouldDeleteIfMigrationNeeded == rhs.ShouldDeleteIfMigrationNeeded &&
                DatabasePath == rhs.DatabasePath;
        }


        /// <summary>
        /// Serves as a hash function for a <see cref="Realms.RealmConfiguration"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return DatabasePath.GetHashCode();
        }

    }  // class RealmConfiguration
}  // namespace Realms

