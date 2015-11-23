/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */
 
using System;
using System.Runtime.InteropServices;

namespace RealmNet
{
    internal class MarshalHelpers
    {
        public static IntPtr BoolToIntPtr(Boolean value)
        {
            return value ? (IntPtr)1 : (IntPtr)0;
        }

        public static Boolean IntPtrToBool(IntPtr value)
        {
            return (IntPtr)1 == value;
        }

        public static IntPtr StrAllocateBuffer(out long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            currentBufferSizeChars = bufferSizeNeededChars;
            return Marshal.AllocHGlobal((IntPtr)(bufferSizeNeededChars * sizeof(char)));
            //allocHGlobal instead of  AllocCoTaskMem because allcHGlobal allows lt 2 gig on 64 bit (not that .net supports that right now, but at least this allocation will work with lt 32 bit strings)   
        }

        public static string StrBufToStr(IntPtr buffer, int bufferSizeNeededChars)
        {
            string retStr = bufferSizeNeededChars > 0 ? Marshal.PtrToStringUni(buffer, bufferSizeNeededChars) : "";
            //return "" if the string is empty, otherwise copy data from the buffer
            Marshal.FreeHGlobal(buffer);
            return retStr;
        }

        public static Boolean StrBufferOverflow(IntPtr buffer, long currentBufferSizeChars, long bufferSizeNeededChars)
        {
            if (currentBufferSizeChars < bufferSizeNeededChars)
            {
                Marshal.FreeHGlobal(buffer);

                return true;
            }
            return false;
        }

        //TODO: This should be a pair of extension methods on DateTimeOffset, mimicking .Net 4.6's DateTimeOffset.ToUnixTimeSeconds and its counterpart
        //but let's store it here for now
        private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static Int64 DateTimeOffsetToUnixTimeSeconds(DateTimeOffset value)
        {
            return Convert.ToInt64((value.ToUniversalTime() - UnixEpoch).TotalSeconds);
        }

        public static DateTimeOffset UnixTimeSecondsToDateTimeOffset(Int64 value)
        {
            return UnixEpoch.AddSeconds(value);
        }

        public static IntPtr RealmColType(Type columnType)
        {
            // ordered in decreasing likelihood of type
            if (columnType == typeof(string))
                return (IntPtr)2;
            if (columnType == typeof(int))
                return (IntPtr)0;
            if (columnType == typeof(float))
                return (IntPtr)9;
            if (columnType == typeof(double))
                return (IntPtr)10;
            if (columnType == typeof(DateTimeOffset))
                return (IntPtr)7;
            if (columnType == typeof(bool))
                return (IntPtr)1;
            /*
            TODO
                    Binary = 4,
                    Table = 5,
                    Mixed = 6,

            */
            throw new NotImplementedException();
        }
    }
}
