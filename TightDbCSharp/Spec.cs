using System;
using System.Globalization;
using System.Runtime.Serialization;


//using System.Threading.Tasks; not portable as of 2013-4-2

//C# SPEC Class. This class is a wrapper of the C++ Spec class.
//Calls are routed this way :
//The C# spec have ordinary C# types and generally keeps the same int size across physical machine layouts
//The C# spec calls methods in TightDBCalls. These TightDBCalls generally have an ordinary C# external interface,
//and then internally call on to functions exported from the c++ DLL
//The design is so, that the C# class does not have any C++ like types or structures, except the SpecHandle variable

namespace TightDbCSharp
{
    //custom exception for Table class. When Table runs into a Table related error, TableException is thrown
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



    public class Spec : Handled 
    {
        //not accessible by source not in te TightDBCSharp namespace
        internal Spec(IntPtr handle, bool shouldbedisposed)
        {
            SetHandle(handle, shouldbedisposed);
        }

        public override bool Equals(object obj)
        {
            Spec spec = (Spec) obj;
            if (spec == null)
            {
                return false;
            }
            return Equals(spec);
        }

        public bool Equals(Spec spec)
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

        internal override void ReleaseHandle()
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
        public void AddField(Field schema)
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
                    subspec.AddFields(tfa);
                }
            }
            else
            {
                throw new ArgumentNullException("schema");
            }
        }


        // will add the field list to the current spec
        public void AddFields(Field[] fields)
        {
            if (fields != null)
            {
                foreach (Field field in fields)
                {
                    AddField(field);
                }
            }
            else
            {
                throw new ArgumentNullException("fields");
            }
        }

        

        public Spec AddSubTableColumn(String columnName)
        {
            return UnsafeNativeMethods.AddSubTableColumn(this, columnName);
        }

        public void AddColumn(DataType type, String name)
        {
            UnsafeNativeMethods.SpecAddColumn(this, type, name);
        }

        //shorthand methods as C# constants doesn't exist so we can't do AddColumn(Type_Int,"name")
        //instead we have AddIntColumn("name") which is sort of almost just as good
        public void AddIntColumn(String name)
        {
            AddColumn(DataType.Int, name);
        }

        //I assume column_idx is a column with a table in it
        //if it is a mixed with a subtable in it, this method will throw
        public Spec GetSpec(long columnIndex)
        {
            if (GetColumnType(columnIndex) == DataType.Table)
            {
                return UnsafeNativeMethods.SpecGetSpec(this, columnIndex);
            }            
           throw new SpecException("get spec(columnIndex) can only be called on a SubTable field");
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