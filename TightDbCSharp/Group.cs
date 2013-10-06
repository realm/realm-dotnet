using System;
using System.ComponentModel;
using System.Globalization;

namespace TightDbCSharp
{
    /// <summary>
    /// Handles a collection of tables that are not shared with other
    /// processes or programs. (see SharedGroup)
    /// </summary>
    public class Group:Handled
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
            UnsafeNativeMethods.GroupNew(this); //calls sethandle itself
            ReadOnly = readOnly;
        }

        /*
        //calld by sharedgroup to get a transaction:group class without the handle set
        //because the handle is then set to the group that the c++ sharedgroup returns
        //in the transaction constructor.
        //if acquirehandle is false, handle will be set to GroupHandle
        internal Group(Boolean acquirehandle,Boolean readOnly)
        {
            if (acquirehandle)
            {
                AcquireHandle(readOnly);
            }
            else
            {
                
            }
        }
        */


        /// <summary>
        ///  //Open in read-only mode. Fail if the file does not already exist.
        ///  mode_ReadOnly,
        ///  //Open in read/write mode. Create the file if it doesn't exist.
        ///  mode_ReadWrite,
        ///  //Open in read/write mode. Fail if the file does not already exist.
        ///  mode_ReadWriteNoCreate
        /// </summary>        
        public enum OpenMode    //nested type inside Group, as in core. CA10344 warning ignored.
        {

            /// <summary>
            /// Open in read-only mode. Fail if the file does not already exist.
            /// </summary>
            ModeReadOnly,

            /// <summary>
            /// Open in read/write mode. Create the file if it doesn't exist.
            /// </summary>

            ModeReadWrite,

            /// <summary>
            /// Open in read/write mode. Fail if the file does not already exist.
            /// </summary>

            ModeReadWriteNoCreate
        }



        //TODO:(also in asana)erorr handling if user specifies an illegal filename or path.
        //We will probably have to do the error handling on the c++ side. It is
        //a problem that c++ seems to crash only when an invalid group(file) is freed or used
        //not when created. Perhaps we should do this in c++
        //1) create the group
        //2) delete it just after //this will get us an exception if the group file is invalid
        //3) create the group again //we only get this far if the file is valid
        //4) if an exception was thrown when it was deleted, return null, indicating the filename is invalid
        //5) otherwise return the group pointer we got from 3)


        //as group files can create problems at any time, any group related calls should probably be wrapped in exception handlers, and
        //should be able to return error codes to C#
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
        public Group(string path,OpenMode openMode)
        {
            try
            {
                UnsafeNativeMethods.GroupNewFile(this, path,openMode);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Dispose();
                throw;
            }
        }
        
        /// <summary>
        /// A group that is part of a readonly transaction will have readonly set to true.
        /// This property is not 100% implemented and thus not 100% reliable
        /// </summary>
        public Boolean ReadOnly { get; private set; }

        /// <summary>
        /// if true, some unexpected error condition exists and this group should never be used
        /// </summary>
        public Boolean Invalid { get; internal set; }

        /// <summary>
        /// True if a table exists in the group with the specified name
        /// </summary>
        /// <param name="tableName">table name to search for</param>
        /// <returns>true if a table with specified name exists</returns>
        public bool HasTable(string tableName)
        {
            return UnsafeNativeMethods.GroupHassTable(this, tableName);
        }

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
            if (HasTable(tableName)) {
            Table fromGroup = UnsafeNativeMethods.GroupGetTable(this, tableName);
//            fromGroup.HasColumns = fromGroup.ColumnCount > 0;//will set HasColumns true if there are columns, even if they are uncomitted as c++ reports uncomitted as well as comitted
                return fromGroup;                            //therefore, the user is expected to call updatefromspec on the same table wrapper that he used to do spec.addcolumn
            }
            throw new InvalidEnumArgumentException(String.Format(CultureInfo.InvariantCulture,"Group.GetTable called with a table name that does not exist {0}",tableName));
        }

        private void ValidateReadOnly()
        {
            if (ReadOnly) throw new InvalidOperationException("Read/Write operation initiated on a Read Only Group");
        }

        /// <summary>
        ///  use this method to create new tables in the group
        ///  either a new table with no columns yet is returned,
        ///  or a table matching the parameter specification is returned.
        ///   Do no call if the table name is already in use
        /// </summary>
        /// <param name="tableName">Name of new table in group</param>
        /// <param name="schema">Column specification of new table</param>
        /// <returns>New table that is part of the group, according to specifications given in the parameter</returns>
        public Table CreateTable(string tableName, params ColumnSpec[] schema)
        {
            ValidateReadOnly();
            if (schema != null && schema.Length>0)
            {
                return UnsafeNativeMethods.GroupGetTable(this, tableName).DefineSchema(schema);
            }
            return UnsafeNativeMethods.GroupGetTable(this, tableName);
        }

        /// <summary>
        /// Writes this group to a directory specified by path
        /// </summary>
        /// <param name="path"></param>
        public void Write(String path)
        {
            UnsafeNativeMethods.GroupWrite(this, path);
        }

        
        /// <summary>
        /// returns a byte[] with the group binary serialized in it
        /// </summary>
        /// <returns>byte array with the seraialized group in it</returns>
        public byte[] WriteToMemory()
        {
            return UnsafeNativeMethods.GroupWriteToMemory(this);
        }



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
                {
                    throw new ArgumentNullException("binaryGroup", " Group cannot be created from a null pointer ");
                }
                if (binaryGroup.Length == 0)
                {
                    throw new ArgumentNullException("binaryGroup", "Group cannot be created from an array of size 0");
                }
                UnsafeNativeMethods.GroupFrombinaryData(this, binaryGroup);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <summary>
        /// Tells c++ that this Group should be disposed of,
        /// remove the accessor object in c++ memory
        /// *do not* subclass Group and call this method - the database could become corrupted
        /// </summary>
        protected override void ReleaseHandle()
        {            
            UnsafeNativeMethods.GroupDelete(this);
        }

        internal override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Group:({0:d}d)  ({1}h)" ,Handle,Handle.ToString("X"));
        }
    }
}
