using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RealmNet.Interop
{
    //tables and tableviews will always return query c++ classes that must be unbound
    //all other query returning calls (on query itself) will return the same object as was called,
    //and therefore have been changed to void calls in c++ part of binding
    //so these handles always represent a qeury object that should be released when not used anymore
    //the C# binding methods on query simply return self to add the . nottation again
    //A query will be a child of whatever root its creator has as root (queries are usually created by tableviews and tables)
    internal class QueryHandle:RealmHandle, IQueryHandle
    {
        public QueryHandle(RealmHandle root) : base(root)
        {
        }

/*
        //acquire a TableView handle with the result And set Root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal TableViewHandle QueryFindAll(long start, long end, long limit)
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);//same root as the query

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(NativeQuery.query_find_all(this, start, end , limit));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }

        //acquire a TableView handle with the result And set Root in an atomic fashion 
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
        internal TableViewHandle QueryFindAll()
        {
            var tvHandle = TableViewHandle.RootedTableViewHandle(this);//same root as the query

            //At this point sh is invalid due to its handle being uninitialized, but the root is set correctly
            //a finalize at this point will not leak anything and the handle will not do anything

            //now, set the TableView handle...
            RuntimeHelpers.PrepareConstrainedRegions();//the following finally will run with no out-of-band exceptions
            try
            { }
            finally
            {
                tvHandle.SetHandle(NativeQuery.query_find_all_np(this));
            }//at this point we have atomically acquired a handle and also set the root correctly so it can be unbound correctly
            return tvHandle;
        }
*/
        protected override void Unbind()
        {
            NativeQuery.query_delete(this);
        }
        
    }
}
