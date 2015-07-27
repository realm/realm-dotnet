using System;
using System.Runtime.CompilerServices;

namespace RealmNet.Interop
{
    internal class TableViewHandle:RealmHandle, ITableViewHandle //todo:consider if we can make the handle classes internal such that users cannot instantiate them
    {
        protected override void Unbind()
        {
            UnsafeNativeMethods.tableview_unbind(this);
        }

        //used in Table to create TableviewHandles atomically, specifying both root and handle at the same time
        private TableViewHandle(RealmHandle root)
            : base(root)
        {
        }

        //This method returns a new TableViewHandle that is with the same root as the calling TableViewHandle
        //if root is null the this tableViewhandle is responsible for cleaning up the tableView and its children
        //if root is something else, it is this tablehandles root, and that root should also manage the tableview and its children
        //note that IgnoreUnbind is set to false, the tableview should be unbound
        private TableViewHandle RootedTableViewHandle()
        {
            return RootedTableViewHandle(this);
        }

        //Return a TableViewHandle where its root is the same as the root of the specified parent object
        //parent can be any kind of RealmHandle
        //Generate a TableView object with its root set to eiter parent or to parents root
        //that is, the link will be directly to the root of the collection of classes
        //a root object R will have root==null
        //all other root objects that have this root object as root, wil have root==R
        //the method is put here instead of into the parents because if it was in each kind of parent
        //there would be more duplicated code
        internal static TableViewHandle RootedTableViewHandle(RealmHandle parent)
        {
            return (parent.Root == null) ?
                new TableViewHandle(parent) :
                new TableViewHandle(parent.Root);
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllInt(long columnIndex, long value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_int(this,columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllBool(long columnIndex, bool value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_bool(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllDateTime(long columnIndex, DateTime value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_datetime(this,columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllFloat(long columnIndex, float value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_float(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllDouble(long columnIndex, double value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_double(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableViewHandle TableViewFindAllString(long columnIndex, string value)
        {
            var tvHandle = RootedTableViewHandle();//attach to our own root

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(UnsafeNativeMethods.tableview_find_all_string(this, columnIndex, value));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }


        //acquire a TableView handle with the result And set Root in an atomic fashion 
        internal TableHandle TableViewGetSubTable(long columnIndex,long rowIndex)
        {
            var tHandle = TableHandle.RootedTableHandle(this); //subtable will get same root as this tableview

            //At this point tvHandle is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tHandle.SetHandle(UnsafeNativeMethods.tableview_get_subtable(this, columnIndex, rowIndex));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tHandle;
        }

    }
}
