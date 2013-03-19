using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//This file is an early attempt to code against the tightdb C interface. Placeholder for newer things to come
//SPEC Specifies a table
namespace tightdb.Tightdbcsharp
{
    public class Spec
    {
        //not accessible by source not in te TightDBCSharp namespace
        internal UIntPtr TDBspec {get;set;}  //handle (or pointer) to a c++ hosted spec.

        public void add_column(TightDbDataType type, string name)
        {
            TightDBCalls.spec_add_column(this, type, name);
        }

        public Spec add_column_table(Spec spec, string name)
        {
            return TightDBCalls.spec_add_column_table(spec, name);
        }

        //I assume column_idx is a column with a table in it, or a mixed with a table?
        public Spec get_spec(long column_idx) 
        {
           return TightDBCalls.spec_get_spec(this,column_idx);
        }

        public long get_column_count()
        {
            return TightDBCalls.spec_get_column_count(this);
        }

        public TightDbDataType get_column_type(long column_idx)
        {
            return TightDBCalls.spec_get_column_type(this, column_idx);
        }

        public string get_column_name(long column_idx)
        {
            return TightDBCalls.spec_get_column_name(this, column_idx);
        }


        //after having called this method - this spec is invalid and should not be used anymore
        public void delete()
        {
            TightDBCalls.spec_delete(this);
        }


    }
}
