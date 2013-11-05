using System;
using System.Globalization;


//using System.Threading.Tasks; not portable as of 2013-4-2

//C# SPEC Class. This class is a wrapper of the C++ Spec class.
//Calls are routed this way :
//The C# spec have ordinary C# types and generally keeps the same int size across physical machine layouts
//The C# spec calls methods in TightDBCalls. These TightDBCalls generally have an ordinary C# external interface,
//and then internally call on to functions exported from the c++ DLL
//The design is so, that the C# class does not have any C++ like types or structures, except the SpecHandle variable

namespace TightDbCSharp
{
    /*
    //custom exception for Spec class. When Table runs into a Table related error, TableException is thrown
    //some system exceptions might also be thrown, in case they have not much to do with Table operation
    //following the pattern described here http://msdn.microsoft.com/en-us/library/87cdya3t.aspx
    [Serializable]
    public class SpecException : Exception
    {
        public SpecException()
        {
        }

        public SpecException(string message)
            : base(message)
        {
        }

        public SpecException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SpecException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }
    }
    */


    /// <summary>
    /// Spec is depricated and should only be used in binding and by users if absolutely neccessary.
    /// Spec returns the table specification for a table and have methods to investigate and mainpulate this
    /// specification.
    /// </summary>
    public class Spec : Handled 
    {
        //not accessible by source not in te TightDBCSharp namespace
        internal Spec(Table ownerRootTable,IntPtr handle, bool shouldbedisposed)
        {
            OwnerRootTable = ownerRootTable;
            SetHandle(handle, shouldbedisposed,ownerRootTable.ReadOnly);
            //BUG: If this spec is a spec for a subtable on a read write table then 
            //this spec will be set as not readonly even though it actually is
            //todo:unit test that showcases this
            //reason the bug is here, is that we're getting rid of spec anyways
        }

        /// <summary>
        ///if spec is the spec for a root table, OwnerTable is that root table
        ///if spec is the spec for a table in a cell in a mixed column then OwnerTable is that table in that cell
        ///if spec is the spec for a non-mixed subtable in a table, then OwnerTable is ***NOT*** that subtable
        ///as all the subtables in that column share the same spec. Instead OwnerTable is the ultimate table that has this spec
        ///as a subcolumn spec.
        ///this reference will keep a table alive as long as we have spec's pointing to it, and it is used
        ///for validation of spec operations
        /// </summary>
        public Table OwnerRootTable { get; private set; }

        /* I hope this is not needed - removed in the process of trying to get rid of spec as much as possible
        /// <summary>
        /// Determines whether the specified spec is equal to the current spec.
        /// </summary>
        /// <returns>
        /// true if the specified spec is equal to the current spec equality is same field names and types, recursively in case of subtables
        /// </returns>
        /// <param name="obj">The spec object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var spec = (Spec) obj;
            if (spec == null)
            {
                return false;
            }
            return UnsafeNativeMethods.SpecEqualsSpec(spec.OwnerRootTable, OwnerRootTable);
        }


        //this is pretty slow due to two interop calls, but still faster than if all field names were used in the hash.
        //a spec will have a hash consisting of the column count XOR'ed with the string name of the last column in the table
        public override int GetHashCode()
        {
            int res = 0;
            long n = ColumnCount;
            if (n > 0)
            {
                res = res ^ GetColumnName(n-1).GetHashCode();
            }
            return res;
        }
        */


        /// <summary>
        /// Column count for this table specification.
        /// columns including eventual columns only in spec, not applied to table yet
        /// </summary>
        public long ColumnCount
        {
            get { return UnsafeNativeMethods.SpecGetColumnCount(this); }
        }

        //if false, the spechandle do not need to be disposed of, on the c++ side
        //wether to actually dispose or not is handled in tightdbcalls.cs so the spec object should act as if it should always dispose of itself

 
        //this method is for internal use only
        //it will automatically be called when the spec object is disposed
        //In fact, you should not at all it on your own

        /// <summary>
        /// Do not use when subclassing spec. Internal stuff
        /// </summary>
        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.SpecDeallocate(this);
        }

        internal override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Spec:" + Handle);
        }


        

        /// <summary>
        /// Get the spec for the subtable at columnIndex.
        /// columnIndex must point to a DataType.Table column.
        /// if the column at  columnIndex is mixed or mixed table,
        /// and exception is thrown
        /// </summary>
        /// <param name="columnIndex">Zero based column index of a DataType.Table column</param>
        /// <returns>Spec for the specified subtable</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Spec GetSpec(long columnIndex)
        {
            if (GetColumnType(columnIndex) == DataType.Table)
            {
                return UnsafeNativeMethods.SpecGetSpec(this, columnIndex);
            }            
           throw new  ArgumentOutOfRangeException("columnIndex",columnIndex,"get spec(columnIndex) can only be called on a SubTable field");
        }

        /// <summary>
        /// return the DataType of the column identified by index
        /// </summary>
        /// <param name="columnIndex">Zero based columnIndex of the column whose DataType to return</param>
        /// <returns>DatType of the specicfied column</returns>
        public DataType GetColumnType(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnType(this, columnIndex);
        }
        

        /// <summary>
        /// Return the name of the column specified by index
        /// </summary>
        /// <param name="columnIndex">Zero based column index of column whose name to return</param>
        /// <returns></returns>
        public string GetColumnName(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnName(this, columnIndex);
        }
    }
}