using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using InteropShared;
using RealmNet.Interop;

namespace RealmNet
{
    /// <summary>
    /// Handles a collection of tables that are not shared with other
    /// processes or programs. (see SharedGroup)
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class Group : Handled
    {

        /// <summary>
        /// Return new Group. is IDisposable as it has a handle to a c++ managed Group
        /// </summary>
        public Group()
        {
            try
            {
                AcquireHandle(false);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        //this constructor is called by transaction to avoid group() being called, which would 
        //create a new c++ group class which the transaction does not need as it gets its handle
        //from shared group 

        internal Group(Boolean isReadOnly)
        {
            ReadOnly = isReadOnly;
        }

        //actually acquire the handle to this group
        //the caller must know if this group is readonly or not
        private void AcquireHandle(bool readOnly)
        {
            SetHandle(Realm.ActiveCoreProvider.NewGroup(), readOnly);
            //NativeGroup.groupNew(this.GroupHandle, readOnly); //calls sethandle itself
            IsValid = true;
        }


        /*MSDN 
         * By default, the operator == tests for reference equality by determining if two references indicate the same object,
         * so reference types do not need to implement operator == in order to gain this functionality. When a type is immutable,
         * meaning the data contained in the instance cannot be changed, overloading operator == to compare value equality
         * instead of reference equality can be useful because, as immutable objects, they can be considered the same as
         * long as they have the same value. Overriding operator == in non-immutable types is not recommended.
         * 
         * Above is the rationale behind Group not having an overwritten == operator
         * http://msdn.microsoft.com/en-us/library/ms173147(v=vs.80).aspx
         */

        /*MSDN
         * Guidelines for Reference Types The following guidelines apply to overriding Equals(Object) for a reference type:
         * 
         * Consider overriding Equals if the semantics of the type are based on the fact that the type represents some value(s). 
         * Most reference types must not overload the equality operator, even if they override Equals. However,
         * if you are implementing a reference type that is intended to have value semantics, such as a complex number type,
         * you must override the equality operator.
         * You should not override Equals on a mutable reference type. 
         * This is because overriding Equals requires that you also override the GetHashCode method,
         * as discussed in the previous section. This means that the hash code of an instance of a mutable reference type 
         * can change during its lifetime, which can cause the object to be lost in a hash table.
         * http://msdn.microsoft.com/en-us/library/bsc2ak47(v=vs.110).aspx
         */

        //As stated above, it is not a good idea to overide the == operator or implement the IComparable interface on 
        //mutable reference types. We will provide a hook into group== by supplying a third alternative
/*
        /// <summary>
        /// Compare this group with another group for equality. Two groups are equal if, and
        /// only if, they contain the same tables in the same order, that
        /// is, for each table T at index I in one of the groups, there is
        /// a table at index I in the other group that is equal to T.
        /// </summary>
        /// <param name="otherGroup">a Group</param>
        /// <returns>true if the groups parameter is with the same data as this</returns>
        public Boolean EqualsGroup(Group otherGroup)
        {
            ValidateIsValid();
            return otherGroup != null && NativeGroup.equals(this.GroupHandle, otherGroup.GroupHandle);
        }
*/

        //TODO:(also in asana)erorr handling if user specifies an illegal filename or path.
        //We will probably have to do the error handling on the c++ side. It is
        //a problem that c++ seems to crash only when an invalid group(file) is freed or used
        //not when created. Perhaps we should do this in c++
        //1) create the group
        //2) delete it just after //this will get us an exception if the group file is invalid
        //3) create the group again //we only get this far if the file is valid
        //4) if an exception was thrown when it was deleted, return null, indicating the filename is invalid
        //5) otherwise return the group pointer we got from 3)


        /// <summary>
        /// Create a group object. Either have it represent a group already stored on a file, or have a new
        /// file created which then contains the data of this group.
        /// Beware that writing a group back to its own file with group.write is not allowed. 
        /// </summary>
        /// <param name="path">Path and filename to open</param>
        /// <param name="openMode">
        /// ModeReadOnly : Open in read-only mode. Fail if the file does not already exist.
        /// ModeReadWrite: Open in read/write mode. Create the file if it doesn't exist.
        /// ModeReadWriteNoCreate : Open in read/write mode. Fail if the file does not already exist.
        /// </param>
        public Group(string path, GroupOpenMode openMode)
        {
            try
            {
                var handle = Realm.ActiveCoreProvider.NewGroupFromFile(path, openMode);
                SetHandle(handle, openMode == GroupOpenMode.ModeReadOnly);
                IsValid = true;
            }
            catch (Exception)
            {
                //Console.WriteLine(e.Message);
                Dispose();//this is okay. GroupNewFile has not set handle if we get an exception so dispose will figure not to call core with a null handle
                throw;
            }
        }

/*
        /// <summary>
        /// Create a Group from a binary representation craeted with WriteToMemory
        /// </summary>
        /// <param name="binaryGroup">a byte array containing the binary representation of the group to create</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Group(byte[] binaryGroup)
        {
            try
            {
                if (binaryGroup == null)
                    throw new ArgumentNullException("binaryGroup", " Group cannot be created from a null pointer ");

                if (binaryGroup.Length == 0)
                    throw new ArgumentException("Group cannot be created from an array of size 0", "binaryGroup");

                NativeGroup.frombinary_data(this.GroupHandle, binaryGroup);
                IsValid = true;
            }
            catch (Exception)
            {
                Dispose();//not entirely sure if this is neccessary in all cases, but better safe than sorry.
                //also not sure if calling dispose is safe if something went wrong in c++ consttructing the group
                throw;
            }
        }

        //group.open not implemented as C# groups are always created as a file group or as a binary group
        //or as an "owns its own memory" group

        //group.is_attached is not implemented due to reasons stated above


            

        //todo:implement get_table_name(std::size_t table_ndx)

        /// <summary>
        /// True if a table exists in the group with the specified name
        /// </summary>
        /// <param name="tableName">table name to search for</param>
        /// <returns>true if a table with specified name exists</returns>
        public bool HasTable(string tableName)
        {
            ValidateIsValid();
            return NativeGroup.has_table(this.GroupHandle, tableName);
        }

*/
        internal IGroupHandle GroupHandle
        {
            get { return Handle as IGroupHandle; }
        }

        /*
        /// <summary>
        /// use this method to get a table that already exists in the group
        /// will return the table associated with tableName in the group, or if no such table exists,
        /// an exception is thrown. Name is case sensitive
        ///  </summary>
        /// <param name="tableName"></param>
        /// <returns>The first table in the group with the specified name</returns>
        /// <exception cref="InvalidEnumArgumentException">Thrown if no table exists with that name</exception>
        public Table GetTable(string tableName)
        {
            ValidateIsValid();
            if (HasTable(tableName))
            {//todo:the HasTable check might be moved to c++ to save an interop roundtrip 
                return GetTableInternal(tableName);
            }
            throw new InvalidEnumArgumentException(String.Format(CultureInfo.InvariantCulture, "Group.GetTable called with a table name that does not exist {0}", tableName));
        }

        //unconditionally return the named table, or a new table from this group
        private Table GetTableInternal(String tableName)
        {
            return new Table(GroupHandle.GetTable(tableName), ReadOnly);
        }

        /// <summary>
        /// Return the table that has the specified index in the group
        /// </summary>
        /// <param name="tableIndex">zero based index of an existing table in the Group</param>
        /// <returns>The table at the index in the groups list of tables</returns>
        /// <exception cref="ArgumentOutOfRangeException">If the index is negative or larger than or equal to size</exception>
        private Table GetTable(long tableIndex)
        {
            if (tableIndex < 0 || tableIndex >= Size)
            {
                throw new ArgumentOutOfRangeException("tableIndex", String.Format(CultureInfo.InvariantCulture, "Group.GetTable called with an index ({0}) that is out of range :{1}", tableIndex, ToString()));
            }
            return new Table(GroupHandle.GetTable(tableIndex), ReadOnly);
        }

        /// <summary>
        ///  use this method to create new tables in the group
        ///  either a new table with no columns yet is returned,
        ///  or a table matching the parameter specification is returned.        
        ///  (Known Bug:)
        ///  Do not take the same table out multiple times A, and B from the same group, and then use the tables interleaved,
        ///  Modifying table A will not invalidate TableViews connected to Table B
        /// </summary>
        /// <param name="tableName">Name of new table in group</param>
        /// <param name="schema">Column specification of new table</param>
        /// <returns>New table that is part of the group, according to specifications given in the parameter</returns>
        public Table CreateTable(string tableName, params ColumnSpec[] schema)
        {
            ValidateIsValid();
            ValidateReadWrite();
            if (schema != null && schema.Length > 0)
            {
                return GetTableInternal(tableName).DefineSchema(schema);
            }
            return GetTableInternal(tableName);
        }

        /// <summary>
        /// Writes this group to a directory specified by path
        /// </summary>
        /// <param name="path"></param>
        public void Write(String path)
        {
            ValidateIsValid();
            NativeGroup.write(this.GroupHandle, path);
        }


        /// <summary>
        /// returns a byte[] with the group binary serialized in it
        /// </summary>
        /// <returns>byte array with the seraialized group in it</returns>
        public byte[] WriteToMemory()
        {
            ValidateIsValid();
            return NativeGroup.write_to_memory(this.GroupHandle);
        }
        */

        /// <summary>
        /// Flushes changes to the group back to its file
        /// </summary>
        public void Commit()
        {
            ValidateIsValid();
            Realm.ActiveCoreProvider.GroupCommit(this.GroupHandle);
        }

        //todo:implement ToJson (by calling core template<class S> void to_json(S& out))


        /// <summary>
        /// Enhance toString to also show the group to string data from core
        /// </summary>
        public override string ToString()
        {
            ValidateIsValid();
            return base.ToString() + " " + GroupHandle;
        }

        /// <summary>
        /// True if the group has no tables
        /// False if the group has tables
        /// </summary>
        /// <returns>True if the group has no Tables, False if the group has any tables</returns>
        public Boolean IsEmpty()
        {
            ValidateIsValid();
            return Realm.ActiveCoreProvider.GroupIsEmpty(this.GroupHandle);
        }


        /// <summary>
        /// This property will return the number of tables in the Group
        /// Could take a little while to execute as it has to round trip to core
        /// </summary>
        public long Size
        {
            get
            {
                ValidateIsValid();
                return Realm.ActiveCoreProvider.GroupSize(this.GroupHandle);
            }
        }

        /// <summary>
        /// if true, some unexpected error condition exists and this group should never be used
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Boolean IsValid { get; internal set; }//todo:figure a situation where a group becomes invalid, create a unit test for that situation,add code that invalidates a group

        private void ValidateIsValid()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot do operations on group that has IsValid==True");
            }
        }
    }
}
