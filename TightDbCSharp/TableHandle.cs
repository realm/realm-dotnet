using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TightDbCSharp
{
    class TableHandle:TightDbHandle
    {
        public TableHandle(TightDbHandle root) : base(root)
        {
        }

        protected override void Unbind()
        {
            UnsafeNativeMethods.TableUnbind(handle);
        }

        //if root is null the this tablehandle is responsible for cleaning up the tableView and its children
        //if root is something else, it is the this tablehandles root, and that root should also manage the tableview and its children
        //note that IgnoreUnbind is set to false, the tableview should be unbound
        private TableViewHandle RootedTableViewHandle()
        {
            return Root == null ?
                new TableViewHandle(this):
                new TableViewHandle(Root);
        }
        //acquire a TableViewHandle And set root in an atomic fashion 
        internal TableViewHandle TableDistinct(long columnIndex)
        {
            var sh = RootedTableViewHandle();

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



        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableFindAllInt(long columnIndex,long value)
        {
            var tvHandle = RootedTableViewHandle();

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
            var tvHandle = RootedTableViewHandle();

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
            var tvHandle = RootedTableViewHandle();

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
            var tvHandle = RootedTableViewHandle();

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
            var tvHandle = RootedTableViewHandle();

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
        //this method covers two c++ calls, one for empty binary array search, and one for searching
        //some actual value. The empty has its own code as it would be somewhat faster that way,
        //saving pinning and marshalling an empty array.
        internal TableViewHandle TableFindAllBinary(long columnIndex, byte[] value)
        {
            var tvHandle = RootedTableViewHandle();
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
            //if root is null the this tablehandle is responsible for cleaning up the spec and any specs taken out from it
            //if root is something else, it is the this tablehandles root, and that root should also manage the specs
            //note that IgnoreUnbind is set to true, so in fact the spec created here will not call back.
            //but at least theoreticall (if the binding is extended) spec children might want to be unbindable and they must be linked to root
            var sh = Root == null ? new SpecHandle(true, this) : new SpecHandle(true,Root);
            
            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly, as is the IgnoreUnbind setting
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the spec handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                sh.SetHandle(UnsafeNativeMethods.TableGetSpec(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return sh;
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

