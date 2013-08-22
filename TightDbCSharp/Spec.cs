using System;
using System.Collections.Generic;
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


    public class Spec : Handled 
    {
        //not accessible by source not in te TightDBCSharp namespace
        internal Spec(Table ownerRootTable,IntPtr handle, bool shouldbedisposed)
        {
            OwnerRootTable = ownerRootTable;
            SetHandle(handle, shouldbedisposed);
        }

        //if spec is the spec for a root table, OwnerTable is that root table
        //if spec is the spec for a table in a cell in a mixed column then OwnerTable is that table in that cell
        //if spec is the spec for a non-mixed subtable in a table, then OwnerTable is ***NOT*** that subtable
        //as all the subtables in that column share the same spec. Instead OwnerTable is the ultimate table that has this spec
        //as a subcolumn spec.
        //this reference will keep a table alive as long as we have spec's pointing to it, and it is used
        //for validation of spec operations
        public Table OwnerRootTable { get; private set; }

        public override bool Equals(object obj)
        {
            var spec = (Spec) obj;
            if (spec == null)
            {
                return false;
            }
            return Equals(spec);
        }

        private bool Equals(Spec spec)
        {
            return UnsafeNativeMethods.SpecEquals(this,spec);
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


        public long ColumnCount
        {
            get { return UnsafeNativeMethods.SpecGetColumnCount(this); }
        }

        //if false, the spechandle do not need to be disposed of, on the c++ side
        //wether to actually dispose or not is handled in tightdbcalls.cs so the spec object should act as if it should always dispose of itself

 
        //this method is for internal use only
        //it will automatically be called when the spec object is disposed
        //In fact, you should not at all it on your own

        protected override void ReleaseHandle()
        {
            UnsafeNativeMethods.SpecDeallocate(this);
        }

        public override string ObjectIdentification()
        {
            return string.Format(CultureInfo.InvariantCulture, "Spec:" + Handle);
        }


        //Depending on where we get the spec handle from, it could be a structure that should be
        //deleted or a structure that should not be deleted (deallocated) in c++
        //the second parameter in the constructor is indication of this spec handle should be deallocated
        //by a call to spec_delete or if c# should do nothing when the spec handle is no longer in use in c#
        //(the spec handles that need to be deleted have been allocated as new structures, the ones that
        //do not need to be deleted are pointers into structures that are owned by a table
        //This means that a spec that has been gotten from a table should not be used after that table have
        //been deallocated.

        //add this field to the current spec. Will add recursively if neeeded
        internal void AddFieldNoCheck(Field schema)
        {
            if (schema != null)
            {
                if (schema.FieldType != DataType.Table)
                {
                    AddColumn(schema.FieldType, schema.ColumnName);
                }
                else
                {
                    Field[] tfa = schema.GetSubTableArray();
                    Spec subspec = AddSubTableColumn(schema.ColumnName);
                    subspec.AddFieldsNoCheck(tfa);
                }
            }
            else
            {
                throw new ArgumentNullException("schema");
            }
        }

        public void AddField(Field schema)
        {
            OwnerRootTable.ValidateSpecChangeIsOkay();
            //if we are a subtable taken out from a row, then the above check will catch the situation as we must have columns and that makes spec changes not okay
            //if we are a spec gotten get spec.getsubspec etc then it is valid to add a field if the root table has no columns
           
            AddFieldNoCheck(schema);
        }

        // will add the field list to the current spec
        private void AddFieldsNoCheck(IEnumerable<Field> fields)
        {
            if (fields != null)
            {
                foreach (Field field in fields)
                {
                    AddFieldNoCheck(field);
                }
            }
            else
            {
                throw new ArgumentNullException("fields");
            }
        }

        

        public Spec AddSubTableColumn(String name)
        {
            ValidateOkayToModify();//updatefromspec can only be called once, on a table with no comitted columns.
            return UnsafeNativeMethods.AddSubTableColumn(this, name);
        }


        private void ValidateOkayToModify()
        {
            OwnerRootTable.ValidateNoColumns();//because updatefromspec can only be called once, on a root table, to create all the fields
            OwnerRootTable.ValidateIsValid();//of course our root table must be valid
            //in other words, the root table must have no rows, and ... no prior specified columns
            //The only way to use a spec to modify a table is via Updatefromspec, so we preemptively stop the user from modifying a spec
            //if we know in advance that update will fail on the root table
            //todo:we should also verify that the root table is not readonly (could realistically happen if You get a ST from a mixed in a readonly transaction)
        }

        public long AddColumn(DataType type, String name)
        {
            ValidateOkayToModify();
            return UnsafeNativeMethods.SpecAddColumn(this, type, name);
        }

        //The reason we have a specific AddSubTableColumn is bc it returns
        //the spec of the subtable that is being added (to enable sub sub columns)
        //You could also call AddColumn(Datatype.Table,"name") and that would work,
        //but then You would have to read back the subtable spec manually afterwards
        //To balance the interface, the methods below has been added so that You can 
        //add all types without having to specify a DataType constant
        public long AddBinaryColumn(String name)
        {
            return AddColumn(DataType.Binary, name);
        }

        public long AddBoolColumn(String name)
        {
            return AddColumn(DataType.Bool, name);
        }

        public long AddDateColumn(String name)
        {
            return AddColumn(DataType.Date, name);
        }

        public long AddDoubleColumn(String name)
        {
            return AddColumn(DataType.Double, name);
        }

        public long AddFloatColumn(String name)
        {
            return AddColumn(DataType.Float, name);
        }

        public long AddIntColumn(String name)
        {
            return AddColumn(DataType.Int, name);
        }

        public long AddMixedColumn(String name)
        {
            return AddColumn(DataType.Mixed, name);
        }

        public long AddStringColumn(String name)
        {
            return AddColumn(DataType.String, name);
        }

        public long AddTableColumn(String name)
        {
            return AddColumn(DataType.Table, name);
        }




        //I assume column_idx is a column with a table in it
        //if it is a mixed with a subtable in it, this method will throw
        public Spec GetSpec(long columnIndex)
        {
            if (GetColumnType(columnIndex) == DataType.Table)
            {
                return UnsafeNativeMethods.SpecGetSpec(this, columnIndex);
            }            
           throw new  ArgumentOutOfRangeException("columnIndex",columnIndex,"get spec(columnIndex) can only be called on a SubTable field");
        }

        public DataType GetColumnType(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnType(this, columnIndex);
        }

        

        public string GetColumnName(long columnIndex)
        {
            return UnsafeNativeMethods.SpecGetColumnName(this, columnIndex);
        }
    }
}