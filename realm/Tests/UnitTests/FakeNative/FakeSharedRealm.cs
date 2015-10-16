using System;
using UnitTests;

namespace RealmNet
{
    internal static class NativeSharedRealm
    {
        internal static IntPtr open(SchemaHandle schemaHandle, string path, IntPtr readOnly, IntPtr durability, string encryptionKey)
        {
            return (IntPtr) 0;
        }

        internal static IntPtr destroy(IntPtr sharedRealm)
        {
            return (IntPtr) 0;
        }

        internal static IntPtr has_table(SharedRealmHandle sharedRealm, string tableName)
        {
            return (IntPtr) 0;
        }

        internal static void begin_transaction(SharedRealmHandle sharedRealm)
        {
        }

        internal static void commit_transaction(SharedRealmHandle sharedRealm)
        {
        }

        internal static void cancel_transaction(SharedRealmHandle sharedRealm)
        {
        }

        internal static IntPtr is_in_transaction(SharedRealmHandle sharedRealm)
        {
            return (IntPtr) 0;
        }

        internal static void refresh(SharedRealmHandle sharedRealm)
        {
        }

        internal static IntPtr get_table(SharedRealmHandle sharedRealm, string tableName, IntPtr tableNameLength)
        {
            Logger.LogCall($"{nameof(tableName)} = \"{tableName}\"");
            return (IntPtr) 0;
        }
    }
}
