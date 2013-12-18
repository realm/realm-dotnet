using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TightDbCSharp
{
    public class TableHandle:TightDbHandle
    {
        private TableHandle(TightDbHandle root) : base(root)
        {
        }

        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        private TableHandle()
        {
        }

        protected override void Unbind()
        {
            UnsafeNativeMethods.TableUnbind(this);
        }

        /*
        //if root is null the this tablehandle is responsible for cleaning up the tableView and its children
        //if root is something else, it is the this tablehandles root, and that root should also manage the tableview and its children
        //note that IgnoreUnbind is set to false, the tableview should be unbound
        private TableViewHandle RootedTableViewHandle()
        {
            return (Root == null) ?
                new TableViewHandle(this):
                new TableViewHandle(Root);
        }
        */

        private QueryHandle RootedQueryHandle()
        {
            return (Root == null) ?
                new QueryHandle(this):
                new QueryHandle(Root);
        }

        private static TableHandle RootedTableHandle()
        {
            return new TableHandle(null);
        }

        //call with a parent, will set the correct root (parent if parent.root=null, or parent.root otherwise)
        //if You want a RootedTableHandle with is self-rooted, call with no parameter
        internal static TableHandle RootedTableHandle(TightDbHandle parent)
        {
            return (parent.Root == null)
                ? new TableHandle(parent)
                : new TableHandle(parent.Root);
        }

        //Returns a copy of this table as a new handle
        internal TableHandle TableCopyTable()
        {
            var th = RootedTableHandle();//the resulting table is freestanding and its own root

            //At this point th is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                th.SetHandle(UnsafeNativeMethods.TableCopyTable(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return th;
        }


        //acquire a TableHandle And set root in an atomic fashion 
        internal TableHandle TableGetSubTable(long columnIndex, long rowIndex)
        {
            var th = RootedTableHandle(this);//the resulting table will have the same root as this

            //At this point th is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                th.SetHandle(UnsafeNativeMethods.TableGetSubTable(this,columnIndex,rowIndex));//call core with this and the subtable location. put the returned subtable handle into th
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return th;
        }

        //acquire a TableViewHandle And set root in an atomic fashion 
        internal TableViewHandle TableDistinct(long columnIndex)
        {
            var sh = TableViewHandle.RootedTableViewHandle(this);//will have same root as this

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                sh.SetHandle(UnsafeNativeMethods.TableDistinct(this,columnIndex));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return sh;
        }


        //acquire a QueryHandle from table_where And set root in an atomic fashion 
        internal QueryHandle TableWhere()
        {
            var queryHandle = RootedQueryHandle();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                queryHandle.SetHandle(UnsafeNativeMethods.TableWhere(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllInt(long columnIndex,long value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllInt(this,columnIndex,value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllBool(long columnIndex, bool value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllBool(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllDateTime(long columnIndex, DateTime value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllDateTime(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllFloat(long columnIndex, float value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllFloat(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllDouble(long columnIndex, double value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllDouble(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllString(long columnIndex, string value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllString(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        //this method covers two c++ calls, one for empty binary array search, and one for searching
        //some actual value. The empty has its own code as it would be somewhat faster that way,
        //saving pinning and marshalling an empty array.
        internal TableViewHandle TableFindAllBinary(long columnIndex, byte[] value)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);
            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything
            if (value == null || value.Length == 0) //special case empty array search
            {
                //now, set the TableView handle...
                RuntimeHelpers.PrepareConstrainedRegions();
                //the following finally will run with no out-of-band exceptions
                try
                {
                }
                finally
                {
                    tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllEmptyBinary(this, columnIndex));
                }
                //at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly                
            }
            else //the byte array actually contains something
            {
                var byteshandle = GCHandle.Alloc(value, GCHandleType.Pinned);//GCHandle that points to the array,which is now fixed
                try
                {
                    //now value cannot be moved or garbage collected by garbage collector
                    var valuePointer = byteshandle.AddrOfPinnedObject();//raw pointer that points to value[0]
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                    }
                    finally
                    {
                        tvHandle.SetHandle(UnsafeNativeMethods.TableFindAllBinary(this, columnIndex, valuePointer,(IntPtr)value.Length));
                    }
                }
                finally
                {
                    byteshandle.Free();
                }
            }
            return tvHandle;
        }



        //acquire a spec handle And set IgnoreUnbind in an atomic fashion (table_get_spec)
        internal SpecHandle GetSpec()
        {
            SpecHandle sh = null;
            try
            {
                //if root is null the this tablehandle is responsible for cleaning up the spec and any specs taken out from it
                //if root is something else, it is the this tablehandles root, and that root should also manage the specs
                //note that IgnoreUnbind is set to true, so in fact the spec created here will not call back.
                //but at least theoreticall (if the binding is extended) spec children might want to be unbindable and they must be linked to root
                sh = Root == null ? new SpecHandle(true, this) : new SpecHandle(true, Root);

                //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly, as is the IgnoreUnbind setting
                //a finalize at this point will not leak anything and the handle will not do anything

                //now, set the spec handle...
                RuntimeHelpers.PrepareConstrainedRegions();
                    //the following finally will run with no out-of-band exceptions
                try
                {
                }
                finally
                {
                    sh.SetHandle(UnsafeNativeMethods.TableGetSpec(this));
                }
                    //at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
                return sh;
            }
            catch
            {
                if (sh != null)
                    sh.Dispose();
                throw;
            }
        }
    }
}

/*sample implementation of two-tiered handle instantiation , from http://blogs.msdn.com/b/bclteam/archive/2005/03/15/396335.aspx
 * 
 * //Best practice to avoid object allocation inside CER.
MySafeHandle mySafeHandle = new MySafeHandle(0, true);
IntPtr myHandle;
IntPtr invalidHandle = new IntPtr(-1));
Int32 ret;
 
       // The creation of myHandle and assignment to mySafeHandle should be done inside a CER
RuntimeHelpers.PrepareConstrainedRegions();
try {// Begin CER
}
        finally {
ret = MyNativeMethods.CreateHandle(out myHandle);
              if (ret ==0 && !myHandle.IsNull() && myHandle != invalidHandle)
            mySafeHandle.SetHandle(myHandle);
        }// End CER
*/

