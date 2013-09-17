using System;
using System.ComponentModel;
using System.Globalization;

namespace TightDbCSharp
{
    public class Group:Handled
    {

        
        //constructor called by customers. It will create a c++ class to wrap
        //this group will be unattached and without transactions and readwrite
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

        //a group that is part of a readonly transaction will have readonly set to true
        public Boolean ReadOnly { get; private set; }

        public Boolean Invalid { get; internal set; }//if true, some unexpected error condition exists and this group should never be used

        public bool HasTable(string tableName)
        {
            return UnsafeNativeMethods.GroupHassTable(this, tableName);
        }

        //use this method to get a table that already exists in the group
        //will return the table associated with tableName in the group, or if no such table exists, 
        //an exception is thrown
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

        //use this method to create new tables in the group
        //will return the table associated with tableName in the group, or if no such table exists, 
        //a new table will be created in the group, associated with tableName, and having the schema provided in the second parameter
        public Table CreateTable(string tableName, params Field[] schema)
        {
            ValidateReadOnly();
            if (schema != null && schema.Length>0)
            {
                return UnsafeNativeMethods.GroupGetTable(this, tableName).DefineSchema(schema);
            }
            return UnsafeNativeMethods.GroupGetTable(this, tableName);
        }

        public void Write(string path)
        {
            UnsafeNativeMethods.GroupWrite(this, path);
        }

        //returns a byte[] with the group binary serialized in it
        public byte[] WriteToMemory()
        {
            return UnsafeNativeMethods.GroupWriteToMemory(this);
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
        public Group(string path)
        {
            try
            {
                UnsafeNativeMethods.GroupNewFile(this, path);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

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
