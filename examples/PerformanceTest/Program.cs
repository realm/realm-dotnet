using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using TightDbCSharp;

namespace PerformanceTest
{
    internal static class Program
    {

        //creates a table

        //times the insertion of  1 million records 

        //destroys the table again
        private static void MeasureInsertSpeed(DataType type, int size)
        {
            using (var t = new Table(new Field("testfield", type)))
            {
                DateTime dt = DateTime.Now;
                String s = "".PadRight(size, 'x');
                long i = 256*size;
                var timer1 = Stopwatch.StartNew();

                for (int n = 0; n < 1000*1000; n++)
                {
                    switch (type)
                    {
                        case DataType.Int:
                            t.Add(i);
                            break;
                        case DataType.String:
                            t.Add(s);
                            break;
                        case DataType.Date:
                            t.Add(dt);
                            break;
                    }
                }
                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1M Table.Insert({0} of size{1}): {2} seconds, {3} milliseconds.", type, size, seconds,
                    milliseconds);

                //test a similar C# construct
                timer1 = Stopwatch.StartNew();

                var longList = new List<long>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();

                for (int n = 0; n < 1000*1000; n++)
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
                Console.WriteLine("1M C#List.Insert({0} of size{1}): {2} seconds, {3} milliseconds.", type, size,
                    seconds, milliseconds);
            
            }
        }


        private static long MeasureSearchSpeed(DataType type, int size)
        {
            long temp = 0;
            using (var t = new Table(new Field("testfield", type)))
            {
                var dt = new DateTime(1980,1,1);
                String s = "".PadRight(size, 'x');
                long i = 256 * size;

                //fill table with searchable data
                for (int n = 0; n < 1000 * 1000; n++)
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
                for (int n = 1000*1000; n > 1000*1000 - 1000; n--)
                    
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
                Console.WriteLine("1K Table.FindFirst({0} of size{1}): {2} seconds, {3} milliseconds.", type, size, seconds,
                    milliseconds);

                //test a similar C# construct
                var longList = new List<long>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();

                for (int n = 0; n < 1000 * 1000; n++)
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
                for (int n = 1000 * 1000; n > 1000 * 1000 - 1000; n--)

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
                Console.WriteLine("1K C#List.Find({0} of size{1}): {2} seconds, {3} milliseconds.", type, size,
                    seconds, milliseconds);










            }
            return temp;
        }

        private static void MeasureGetSizeSpeed()
        {
            using (var t = new Table(new Field("testfield", DataType.String)))
            {
                var timer1 = Stopwatch.StartNew();
                long acc = 0;
                for (int n = 0; n < 1000*1000; n++)
                {
                    acc = acc + t.Size;
                }
                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("1M calls to Table.Size: {0} seconds, {1} milliseconds.", seconds, milliseconds);
                Console.WriteLine(acc);
            }
        }



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





        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)"
            ),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization",
             "CA1303:Do not pass literals as localized parameters", MessageId = "Console.WriteLine(System.String)")]

        private static void Main()
        {
            bool loop = true;
            while (loop)
            {
                loop = false;
                Console.WriteLine();
                Console.WriteLine(
                    "Press any key to finish test...\n 1=call measureinteropspeed\n 2=call MeasureInsertSpeed\n 3=call MeasureGetSizeSpeed\n4=call MeasureSearchSpeed\n");
                ConsoleKeyInfo ki = Console.ReadKey();
                Console.WriteLine();
                if (ki.Key == ConsoleKey.D1)
                {
                    MeasureInteropSpeed();
                    loop = true;
                }

                if (ki.Key == ConsoleKey.D2)
                {
                    MeasureInsertSpeed(DataType.Int, 1);
                    MeasureInsertSpeed(DataType.Int, 2);
                    MeasureInsertSpeed(DataType.Int, 3);
                    MeasureInsertSpeed(DataType.Int, 4);
                    MeasureInsertSpeed(DataType.Int, 5);
                    MeasureInsertSpeed(DataType.Int, 6);
                    MeasureInsertSpeed(DataType.Int, 7);
                    MeasureInsertSpeed(DataType.String, 8);
                    MeasureInsertSpeed(DataType.String, 80);
                    MeasureInsertSpeed(DataType.String, 800);
                    MeasureInsertSpeed(DataType.Date, 0);
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

