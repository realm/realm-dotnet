using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace TightDbCSharp
{
    public class Group:Handled
    {
        //constructor called by customers
        public Group()
        {
            UnsafeNativeMethods.GroupNew(this);//calls sethandle itself
        }

        
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
            return UnsafeNativeMethods.GroupGetTable(this, tableName);
            }
            throw new InvalidEnumArgumentException(String.Format(CultureInfo.InvariantCulture,"Group.GetTable called with a table name that does not exist {0}",tableName));
        }


        //use this method to create new tables in the group
        //will return the table associated with tableName in the group, or if no such table exists, 
        //a new table will be created in the group, associated with tableName, and having the schema provided in the second parameter
        public Table CreateTable(string tableName, params Field[] schema)
        {
            if (schema != null)
            {
                return UnsafeNativeMethods.GroupGetTable(this, tableName).DefineSchema(schema);
            }
            return UnsafeNativeMethods.GroupGetTable(this, tableName);
        }

        public void Write(string path)
        {
            UnsafeNativeMethods.GroupWrite(this, path);
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
            UnsafeNativeMethods.GroupNewFile(this,path);
        }

        internal override void ReleaseHandle()
        {            
            Console.WriteLine("ReleaseHandle called "+ObjectIdentification());
            UnsafeNativeMethods.GroupDelete(this);
            Console.WriteLine("Returned from c++ " + ObjectIdentification());
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Group:({0:d}d)  ({1}h)" ,Handle,Handle.ToString("X"));
        }
    }
}
