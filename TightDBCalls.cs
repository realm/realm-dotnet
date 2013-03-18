using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace tightdb.Tightdbcsharp
{
    //mirrors the enum in the C interface
    /*     
   Note: These must be kept in sync with those in
   <tightdb/data_type.hpp> of the core library. 
typedef enum {
    tightdb_Bool   =  1,
    tightdb_Int    =  0,
    tightdb_String =  2,
    tightdb_Binary =  4,
    tightdb_Date   =  7,
    tightdb_Table  =  5,
    tightdb_Mixed  =  6
}
TightdbDataType;
     */

    //Used exclusively in table constructors
    public enum TDB
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Binary = 4,
        Table = 5,
        Mixed = 6,
        Date = 7
    }
   
    public enum TightDbDataType
    {
        Int = 0,
        Bool = 1,
        String = 2,
        Binary = 4,
        Table = 5,
        Mixed = 6,
        Date = 7
    }

    //this class contains methods for calling the c++ TightDB system, which has been flattened out as C type calls   
    //The individual methods call the C inteface using types and values suitable for the C interface, but the methods take and give
    //values that are suitable for C# 
    //all the stuff that is sent to TightDB c++ via P/Invoce is prefixed TDB so a C# spec is called spec while the 
    //value holding the c++ address of the real c++ hosted spec is called TDBspec
    class TightDBCalls
    {

        /*** Spec ************************************/

        //c code  :   void        spec_add_column(Spec *spec,  TightdbDataType type, const char *name);
        public static void spec_add_column(Spec spec, TightDbDataType type, string name)
        {          
            //add pinvoke call and mashalling to  void        spec_add_column(Spec *spec,  TightdbDataType type, const char *name);
            fakepinvoke(spec.TDBspec, type, name);
            //vaules to send : TDBspec, type, name
        }

        public static Spec spec_add_column_table(Spec spec, string name)
        {
            Spec r = new Spec();
            //add pinvoke call and mashalling to  Spec       *spec_add_column_table(Spec *spec, const char *name);            
            r.TDBspec = (UIntPtr)fakepinvoke(spec, name);
            return r;
        }


        //column indexes in C# for specs are ints 
        public static Spec spec_get_spec(Spec spec, long column_idx)
        {
            Spec r = new Spec();
            UIntPtr TDBColix = (UIntPtr)column_idx;//should safely set the TDBColix to the value of the int index
            UIntPtr TDBspec = (UIntPtr)fakepinvoke(spec.TDBspec, TDBColix);
            //Spec       *spec_get_spec(Spec *spec, size_t column_ndx);
            r.TDBspec = TDBspec;
            return r;
        }


        //size_t spec_get_column_count(Spec* spec);
        public static long spec_get_column_count(Spec spec)
        {            
            UIntPtr res = (UIntPtr)fakepinvoke(spec.TDBspec);        //size_t spec_get_column_count(Spec* spec);
            return (long)res;
        }



        //TightdbDataType spec_get_column_type(Spec* spec, size_t column_idx);
        public static TightDbDataType spec_get_column_type(Spec spec,long column_idx)
        {            
            UIntPtr TDBcolix = (UIntPtr)column_idx;
            TightDbDataType r = (TightDbDataType)fakepinvoke(spec.TDBspec, TDBcolix);
            return r;
        }


        // const char *spec_get_column_name(Spec *spec, size_t column_idx);
        public static String spec_get_column_name(Spec spec, long column_idx)
        {
            UIntPtr TDBcolix = (UIntPtr)column_idx;
            String res = (string)fakepinvoke(spec.TDBspec, TDBcolix);
            return res;
        }


        //    size_t      spec_get_column_index(Spec *spec, const char *name);

        public static long spec_get_column_index(Spec spec, String name)
        {            
            UIntPtr col_idx = (UIntPtr)fakepinvoke(spec.TDBspec, name);
            long res = (long)col_idx;
            return res;
        }

        /* Delete spec after use of functions that returns a Spec* */
        // void spec_delete(Spec* spec);
        // so this will deallocate the internal spec pointer inside the spec objet You send.
        // intended use is that table_get_spec gets you a spec, and when finished, you must call spec_delete(spec)
        public static void spec_delete(Spec spec)
        {            
            fakepinvoke(spec.TDBspec);
            spec.TDBspec = UIntPtr.Zero;
        }

        //methods here below are not in use atm.
        // c code :    size_t      spec_get_ref(Spec *spec);
        // expected functionality : unknown (seems to take a pointer to a TDBspec and return a pointer sized int)
        // Not sure if that pointer sized int is a pointer to a spec or what it is
        private static Spec spec_get_ref(Spec spec)
        {
            //add pinvoke call and marshalling to :     size_t      spec_get_ref(Spec *spec);            
            UIntPtr TDBSpecresult  = (UIntPtr)fakepinvoke(spec.TDBspec);

            Spec ret = new Spec();
            ret.TDBspec = TDBSpecresult;
            return ret;
        }


        /*** Table ************************************/

 //       Table* table_new();
 
        public static void  table_new(Table table)
        {
            UIntPtr TDBTAble = (UIntPtr) fakepinvoke(); //a call to table_new 
            table.TDBTable = TDBTAble;            
        }
 
//        void table_delete(Table* t);       /* Delete after use of table_new() */
        public static void table_delete(Table t)
        {
            fakepinvoke(t.TDBTable);//replace with call to table_delete
        }


//      void    table_unbind(const Table *t); /* Ref-count delete of table* from table_get_table() */
        public static void table_unbind(Table t)
        {            
            fakepinvoke(t.TDBTable);//call table_unbind
        }


//        Spec* table_get_spec(Table* t);     /* Use spec_delete() when done */
        public static Spec table_get_spec(Table t)
        {            
            UIntPtr TDBSpec = (UIntPtr)fakepinvoke(t.TDBTable);
            Spec ret = new Spec();
            ret.TDBspec = TDBSpec;
            return ret;
        }


 //       void table_update_from_spec(Table* t);
        public static void table_update_from_spec(Table t)
        {
            
            fakepinvoke(t.TDBTable);
        }
 

//            size_t      table_register_column(Table *t,  TightdbDataType type, const char *name);
        public static long table_register_column(Table t, TDB type, string name)
        {            
            UIntPtr col_idx = (UIntPtr) fakepinvoke(t.TDBTable, type, name);
            return (long)col_idx;            
        }


//            size_t      table_get_column_count(const Table *t);
        public static long table_get_column_count(Table t)
        {
            UIntPtr column_cnt = (UIntPtr)fakepinvoke(t.TDBTable);
            return (long)column_cnt;
        }

//            size_t      table_get_column_index(const Table *t, const char *name);
        public static long table_get_column_index(Table t, string name)
        {            
            UIntPtr column_idx = (UIntPtr)fakepinvoke(t.TDBTable, name);
            return (long)column_idx;
        }


//            const char *table_get_column_name(const Table *t, size_t ndx);
        public static string table_get_column_name(Table t, long column_idx)
        {
            UIntPtr TDBColix = (UIntPtr)column_idx;
            return (string)fakepinvoke(t.TDBTable, TDBColix);//will likely need some marshalling and postprocessing of the return string
        }

        //    TightdbDataType table_get_column_type(const Table *t, size_t ndx);
        public static TightDbDataType table_get_column_type(Table t, long column_idx)
        {
            UIntPtr TDBColix = (UIntPtr)column_idx;
            TightDbDataType res = (TightDbDataType)fakepinvoke(t.TDBTable, TDBColix);
            return res;
        }


//             void table_add(Table *t, ...);
        //I assume this one adds an empty colum to the end of the table and returns the ix of that row
        public static long table_add(Table t)
        {
            UIntPtr colix = (UIntPtr)fakepinvoke(t.TDBTable);
            return (long)colix;
        }


        //             void table_add(Table *t, ...);
        //I assume this one adds an empty colum to the end of the table and returns the ix of that row
        public static long table_insert(Table t,long col_idx)
        {
            UIntPtr colix = (UIntPtr)fakepinvoke(t.TDBTable);
            return (long)colix;
        }

        public static TightDbDataType table_get_columntype(Table t, long columnindex)
        {
           UIntPtr datatype =(UIntPtr)fakepinvoke(t.TDBTable,columnindex);
           return (TightDbDataType)datatype;
        }

        public static long table_get_int(Table t ,long ColumnIndex,long RecordIndex)
        {
            UIntPtr res = (UIntPtr)fakepinvoke(t.TDBTable, ColumnIndex, RecordIndex);
            return (long)res;
        }

        /* Getting values */
        //    int64_t     table_get_int(const Table *t, size_t column_ndx, size_t ndx);
        //    bool        table_get_bool(const Table *t, size_t column_ndx, size_t ndx);
        //    time_t      table_get_date(const Table *t, size_t column_ndx, size_t ndx);
        //    const char *table_get_string(const Table *t, size_t column_ndx, size_t ndx);
        //    BinaryData *table_get_binary(const Table *t, size_t column_ndx, size_t ndx);
        //    Mixed      *table_get_mixed(const Table *t, size_t column_ndx, size_t ndx);
        //   TightdbDataType table_get_mixed_type(const Table *t, size_t column_ndx, size_t ndx);
        //    Table       *table_get_subtable(Table *t, size_t column_ndx, size_t ndx);
        //    const Table *table_get_const_subtable(const Table *t, size_t column_ndx, size_t ndx);
        /* Use table_unbind() to 'delete' the table after use */


        //not used - enables us to write code that resemples the external methods to be created later
        //fakepinvoke can be calld with any number of parametres of any type and return any kind of object.
        private static Object fakepinvoke(params object[] parametres) { return null; }


    }
}