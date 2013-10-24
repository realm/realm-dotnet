using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using TightDbCSharp;

namespace PerformanceTest
{
    internal static class Program
    {

        //creates a table

        //times the insertion of  1 million records 

        //destroys the table again
        private static void MeasureInsertSpeed(DataType type, int size,bool use32BitInts)
        {
            using (var t = new Table(new ColumnSpec("testfield", type)))
            {
                DateTime dt = DateTime.Now;
                String s = "".PadRight(size, 'x');
                long i = (long)Math.Pow(256, size)/2-1;
                int i32 = 0;
                bool cantestwithint = false;
                if (i <= int.MaxValue)
                {
                    i32 = (int) i;
                    cantestwithint = true;
                }

                //at this time i is a long representation of our test value
                //and i32 is an int representation if the value fits, or zero

                
                var timer1 = Stopwatch.StartNew();
                const int numrows = 1000*1000*3;
                t.AddEmptyRow(numrows);


                if (type == DataType.Int && use32BitInts && cantestwithint)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetInt(0, n, i32);
                    }
                }

                if (type == DataType.Int && !use32BitInts)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetLong(0, n, i);
                    }
                }

                if (type == DataType.String )
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetString(0, n, s);
                    }
                }

                if (type == DataType.Date)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetDateTime(0, n, dt);                            
                    }
                }                           


                timer1.Stop();
                var seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                var usingtype = use32BitInts ? "Using SetInt " : "Using SetLong";
                if (type != DataType.Int)
                    usingtype = "             ";
                var sizeStr = String.Format("{0} bytes {1}",size,usingtype);
                Console.WriteLine("{0} Table.Insert({1} of size{2}): {3} seconds, {4} milliseconds.",numrows, type, sizeStr, seconds,milliseconds);

                //test a similar C# construct
                timer1 = Stopwatch.StartNew();

                var longList = new List<long>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();

                for (var n = 0; n < 1000*1000; n++)
                {
                    switch (type)
                    {
                        case DataType.Int:
                            longList.Add(i);
                            break;
                        case DataType.String:
                            stringList.Add(s);
                            break;
                        case DataType.Date:
                            dateTimeList.Add(dt);
                            break;
                    }
                }

                timer1.Stop();
                seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1M C#List.Insert({0} of size{1}): {2} seconds, {3} milliseconds.", type, size,seconds, milliseconds);
            
            }
        }


        private static long MeasureSearchSpeed(DataType type, int size)
        {
            long temp = 0;
            using (var t = new Table(new ColumnSpec("testfield", type)))
            {
                var dt = new DateTime(1980,1,1);
                var s = "".PadRight(size, 'x');
                long i = 256 * size;

                //fill table with searchable data
                for (var n = 0; n < 1000 * 1000; n++)
                {
                    switch (type)
                    {
                        case DataType.Int:
                            t.Add(n+i);
                            break;
                        case DataType.String:
                            t.Add(n.ToString(CultureInfo.InvariantCulture)+s);
                            break;
                        case DataType.Date:
                            t.Add(dt.AddMilliseconds(n));
                            break;
                    }
                }


                //search for the last 10000 inserted rows
                var timer1 = Stopwatch.StartNew();                
                for (var n = 1000*1000; n > 1000*1000 - 1000; n--)
                    
                switch (type)
                {
                    case DataType.Int:
                         temp = t.FindFirstInt(0,n+i);
                        break;
                    case DataType.String:
                         temp = t.FindFirstString(0,n.ToString(CultureInfo.InvariantCulture)+s);
                        break;
                    case DataType.Date:
                         temp = t.FindFirstDateTime(0,dt.AddMilliseconds(n));
                        break;
                }

                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1K Table.FindFirst({0}): {1} seconds, {2} milliseconds.", type, seconds,
                    milliseconds);

                //test a similar C# construct
                var longList = new List<long>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();

                for (var n = 0; n < 1000 * 1000; n++)
                {
                    switch (type)
                    {
                        case DataType.Int:
                            longList.Add(n);
                            break;
                        case DataType.String:
                            stringList.Add(n.ToString(CultureInfo.InvariantCulture)+s);
                            break;
                        case DataType.Date:
                            dateTimeList.Add(dt.AddMilliseconds(n));
                            break;
                    }
                }
                

                //search for the last 10000 inserted rows
                 timer1 = Stopwatch.StartNew();                
                for (var n = 1000 * 1000; n > 1000 * 1000 - 1000; n--)

                    switch (type)
                    {
                        case DataType.Int:
                            temp = longList.IndexOf(n);
                            break;
                        case DataType.String:
                            temp =  stringList.IndexOf(n.ToString(CultureInfo.InvariantCulture))  ;
                            break;
                        case DataType.Date:
                            temp = dateTimeList.IndexOf(dt.AddMilliseconds(n));
                            break;
                    }



                timer1.Stop();
                seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1K C#List.Find({0}): {1} seconds, {2} milliseconds.", type,seconds, milliseconds);










            }
            return temp;
        }

        private static void MeasureGetSizeSpeed()
        {
            using (var t = new Table(new StringColumn("testfield")))
            {
                var timer1 = Stopwatch.StartNew();
                long acc = 0;
                for (int n = 0; n < 1000*1000; n++)
                {
                    if (n%10000 == 0)
                    {
                        t.AddEmptyRow(1);
                    }
                    acc = acc + t.Size;
                }
                timer1.Stop();
                var seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1M calls to Table.Size: {0} seconds, {1} milliseconds.", seconds, milliseconds);
                Console.WriteLine(acc);
            }

            var diag = new Diagnostics();
            var timer2 = Stopwatch.StartNew();
            long acc2=diag.TestNativeSizeCalls();
            timer2.Stop();
            var seconds2 = Math.Floor(timer2.Elapsed.TotalSeconds);
            double milliseconds2 = timer2.Elapsed.Milliseconds;
            Console.WriteLine("1M calls to Table.Size, coded in C++: {0} seconds, {1} milliseconds.", seconds2, milliseconds2);
        }




        /*
        private static void MeasureInteropSpeed()
        {
                long acc = 0;

                Console.WriteLine("Interop Test started");

                var timer1 = Stopwatch.StartNew();
                for (long n = 0; n < 1000*1000; n++)
                {
                    acc = acc + Table.TestAddInteger(n); //c++ function
                }
                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1M calls C# to c++ took: {0} sec., {1} millisec.  result {2}", seconds, milliseconds,
                    acc);
            
        }
        */




        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"
            ),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
             "CA1303:Do not pass literals as localized parameters", MessageId = "Console.WriteLine(System.String)")]

        private static void Main()
        {
            var loop = true;
            Table.ShowVersionTest();
            Table.TestInterop();
            while (loop)
            {
                loop = false;
                Console.WriteLine();
                Console.WriteLine(
                    "Press any key to finish test...\n 1=not implemented\n 2=call MeasureInsertSpeed\n 3=call MeasureGetSizeSpeed\n 4=call MeasureSearchSpeed\n");
                ConsoleKeyInfo ki = Console.ReadKey();
                Console.WriteLine();
                if (ki.Key == ConsoleKey.D1)
                {
                   // MeasureInteropSpeed();
                    loop = true;
                }

                if (ki.Key == ConsoleKey.D2)
                {
                    MeasureInsertSpeed(DataType.Int, 1,true);
                    MeasureInsertSpeed(DataType.Int, 2, true);
                    MeasureInsertSpeed(DataType.Int, 3, true);
                    MeasureInsertSpeed(DataType.Int, 4, true);
                    MeasureInsertSpeed(DataType.Int, 1, false);
                    MeasureInsertSpeed(DataType.Int, 2, false);
                    MeasureInsertSpeed(DataType.Int, 3, false);
                    MeasureInsertSpeed(DataType.Int, 4, false);
                    MeasureInsertSpeed(DataType.Int, 5,false);
                    MeasureInsertSpeed(DataType.Int, 6,false);
                    MeasureInsertSpeed(DataType.Int, 7,false);
                    MeasureInsertSpeed(DataType.String, 8,false);
                    MeasureInsertSpeed(DataType.String, 80,false);
                    MeasureInsertSpeed(DataType.String, 800,false);
                    MeasureInsertSpeed(DataType.Date, 0,false);
                    loop = true;
                }
                if (ki.Key == ConsoleKey.D3)
                {
                    MeasureGetSizeSpeed();
                    loop = true;
                }
                if (ki.Key == ConsoleKey.D4)
                {
                    long temp = 0;
                    temp=temp+MeasureSearchSpeed(DataType.Int,4);
                    temp = temp - MeasureSearchSpeed(DataType.String, 0);
                    temp = temp + MeasureSearchSpeed(DataType.Date, 0);
                    
                    if (temp==42)
                        Console.WriteLine(temp);//this never happes, but compiler has to calculate temp all the way
                    loop = true;
                }
            }
        }
    }
}

