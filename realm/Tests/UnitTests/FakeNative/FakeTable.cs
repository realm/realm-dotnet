using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal static class NativeTable
    {
        internal static IntPtr add_column(TableHandle tableHandle, IntPtr type,
             string name, IntPtr nameLen)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr add_empty_row(TableHandle tableHandle)
        {
            throw new NotImplementedException();
        }

        internal static void set_string(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx,
             string value, IntPtr valueLen)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr get_string(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex,
            IntPtr buffer, IntPtr bufsize)
        {
            throw new NotImplementedException();
        }

        internal static void set_bool(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, IntPtr value)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr get_bool(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex)
        {
            throw new NotImplementedException();
        }

        internal static void set_int64(TableHandle tablePtr, IntPtr columnNdx, IntPtr rowNdx, Int64 value)
        {
            throw new NotImplementedException();
        }

        internal static Int64 get_int64(TableHandle handle, IntPtr columnIndex, IntPtr rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void set_float(TableHandle TableHandle, long columnIndex, long rowIndex, float value)
        {
            throw new NotImplementedException();
        }

        public static float get_float(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        public static void set_double(TableHandle TableHandle, long columnIndex, long rowIndex, double value)
        {
            throw new NotImplementedException();
        }

        public static double get_double(TableHandle TableHandle, long columnIndex, long rowIndex)
        {
            throw new NotImplementedException();
        }

        internal static IntPtr where(TableHandle handle)
        {
            throw new NotImplementedException();
        }

        internal static void unbind(IntPtr tableHandle)
        {
            throw new NotImplementedException();
        }

        public static void remove_row(TableHandle tableHandle, RowHandle rowHandle)
        {
            throw new NotImplementedException();
        }

         //returns -1 if the column string does not match a column index
       internal static IntPtr get_column_index(TableHandle tablehandle,
             string name, IntPtr nameLen)
        {
            throw new NotImplementedException();
        }

	}
}