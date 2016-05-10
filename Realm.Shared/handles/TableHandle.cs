////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////
 
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Realms
{
    internal class TableHandle: RealmHandle
    {
        private TableHandle(RealmHandle root) : base(root)
        {
        }

        //keep this one even though warned that it is not used. It is in fact used by marshalling
        //used by P/Invoke to automatically construct a TableHandle when returning a size_t as a TableHandle
        [Preserve]
        public TableHandle()
        {
        }

        protected override void Unbind()
        {
            NativeTable.unbind(handle);
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
            return new QueryHandle(Root ?? this);
        }

        private LinkListHandle RootedLinkListHandle()
        {
            return new LinkListHandle(Root ?? this);
        }

        //call with a parent, will set the correct root (parent if parent.root=null, or parent.root otherwise)
        //if You want a RootedTableHandle with is self-rooted, call with no parameter
        internal static TableHandle RootedTableHandle(RealmHandle parent)
        {
            return new TableHandle(parent.Root ?? parent);
        }

        //acquire a QueryHandle from table_where And set root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
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
                queryHandle.SetHandle(NativeTable.where(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return queryHandle;
        }

        //acquire a LinkListHandle from table_get_linklist And set root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands"), SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal LinkListHandle TableLinkList(IntPtr columnIndex, RowHandle rowHandle)
        {
            var listHandle = RootedLinkListHandle();

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                var rowIndex = rowHandle.RowIndex;
                listHandle.SetHandle( NativeTable.get_linklist (this, columnIndex, (IntPtr)rowIndex) );
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return listHandle;
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

