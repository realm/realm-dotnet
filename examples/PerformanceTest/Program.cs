using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using TightDbCSharp;
using TightDbCSharp.Extensions;

namespace PerformanceTest
{
    internal static class Program
    {
        enum TestType
        {
            Int,
            Long,
            String,
            Date,
            Float,
            Double,
            Customer,
            GameState,
            PriceBar
        }


        //a class containing information about a currency price (say the euro) for a given second
        //all the prices are in tens of thousands of a dollar
        //Many of them are delta off other prices instead of the full amount
        class PriceBar
        {
            public DateTime timestamp;//The UTC second the bar is information for
            public int lowestbid;//lowest bid this second
            public int lowestask;//delta off lowest bid (usually 10 to 40)
            public int highestbid;//delta off lowest bid (usually less than 100)
            public int highestask;//delta off highest bid (usually 10 to 40)
            public int openinglasttrade;
            public int closinglasttrade;
            public int openingbid;//bid price at the start of this period
            public int openingask;//ask price -delta from bid price
            public int closingbid;//end of period - delta from openingbid
            public int closingask;//end of period - delta from bid

            public PriceBar(int seed)
            {
                timestamp = new DateTime(2010,1,1).AddSeconds(seed);
                lowestbid = 13000 + seed%1001;
                lowestask = seed%21 + 10;
                highestbid = seed%200+10;
                highestask = seed%22 + 10;                
                openingbid = seed%51;
                openingask = seed%23 + 10;
                closingbid = seed%998 - 499;
                closingask = seed%19 + 10;
                openinglasttrade = openingbid + seed%702 -349;
                closinglasttrade = openingbid + closingbid +seed%50-25;
            }
        }


        //a class containing information about a customer
        //the two lists are always of the same length
        //PhoneNames is the names of phone# N
        //PhoneNumbers is the phone number of phone #N
        class Customer
        {
            private static string someRandomChars = "12345abcdefghijkl67"+
                                                    "890mnopqrstuvwxyzæø"+
                                                    "åaABCDEFGHIJcegikmoq"+
                                                    "suwQWERTYUIOPBVCXyæå";//used to generate some names that are not all the same
            public String FirstName;
            public String LastName;
            public String Address;
            public List<String> PhoneNames;
            public List<String> PhoneNumbers;
            public float discount;
            public bool Credit;
            public DateTime CreatedDate;

            //return a string from inside somerandomchars. 
            //Tightdb doesn't compress acc. to string contents so no need to waste
            //test time doing real random strings
            private string  stringOfSize(int start, int length)
            {
                if (length == 0)
                    return "";
                length--;
                return someRandomChars.Substring(start % (someRandomChars.Length - length), length).PadRight(1,someRandomChars[start%42]);//the pad should force a copy as the last char is not following the other chars
            }

            //create a pseudo random customer whose contents and size is dependent on the seed specified
            public Customer(int seed)
            {
                FirstName = stringOfSize(seed, (seed % 16)+4);//first name is a random string betw 4 and 20 long
                LastName = stringOfSize(seed+13, (seed % 16) + 4);//last name is a random string betw 4 and 20 long
                Address = stringOfSize(seed + 71, (seed%30) + 8);//addreess is at least 8 and up to 30 long
                int numphones = seed%4;//0 to 3 phone numbers stored
                if (numphones > 0)
                {
                    PhoneNames= new List<string>();
                    PhoneNumbers=new List<string>();
                }
                for (var n = 0; n < numphones; ++n)
                {                    
                    PhoneNames.Add(stringOfSize(seed+21,(seed %3+4)));//Home, Mobile or the like
                    PhoneNumbers.Add(stringOfSize(seed,seed%4+12));//12 is a reasonable length for a phone number, give or take 3
                }
                discount =1.0f-1.0f/(seed%20+1);
                Credit = seed%3 == 2;
                CreatedDate = DateTime.UtcNow.AddSeconds(seed);//won't create unique dates, and don't have to.
            }
        }



        private static string GetNewString(int size)
        {
            return "".PadRight(size, 'x');
        }

        private static Table GetTableForType(TestType t)
        {
            switch (t)
            {
                case TestType.Int:
                case TestType.Long:
                    return new Table("intfield".Int());                    
                case TestType.String:
                    return new Table("string".String());
                case TestType.Date:
                    return new Table("date".Date());
                case TestType.Float:
                    return new Table("float".Float());
                case TestType.Double:
                    return new Table("double".Double());
                case TestType.Customer:
                    return new Table("First Name".String(),
                                     "Last Name".String(),
                                     "Address".String(),
                                     "Phones".SubTable("Phone".String(),"number".String()),
                                     "discount".Float(),
                                     "Credit".Bool(),
                                     "CreatedDate".Date());
                case TestType.GameState:
                    return new Table();//not coded yet                    
                case TestType.PriceBar:
                    return new Table("Timestamp".Date(),
                                     "lowestbid".Int(),
                                     "lowestask".Int(),
                                     "highestbid".Int(),
                                     "highestask".Int(),
                                     "openinglasttrade".Int(),
                                     "closinglasttrade".Int(),
                                     "openingbid".Int(),
                                     "openingask".Int(),
                                     "closingbid".Int(),
                                     "closingask".Int()
                                     );
                default:
                    throw new ArgumentOutOfRangeException("t");
            }
        }


        //destroys the table again
        private static void MeasureInsertSpeed(TestType type, int size)
        {
            using (var t = GetTableForType(type))
            {
                DateTime dt = DateTime.Now;
                 
                long i = (long) Math.Pow(256, size)/2 - 1;
                int i32 = 0;
                short i16 = 0;
                bool cantestwithint = false;
                if (i <= int.MaxValue)
                {
                    i32 = (int) i;
                    cantestwithint = true;
                }

                if (i <= short.MaxValue)
                {
                    i16 = (short) i;
                }

                //at this time i is a long representation of our test value
                //and i32 is an int representation if the value fits, or zero


                var before = getMemUsedBefore();
                var timer1 = Stopwatch.StartNew();
                const int numrows = 1000*1000*3;
                t.AddEmptyRow(numrows);

                //add some data. The for loop is inside the if's to make sure we don't do 
                //checks on what kind of record to insert every time we insert a record

                if (type == TestType.Int)
                {
                    if (cantestwithint)
                    {
                        for (var n = 0; n < numrows; ++n)
                        {
                            t.SetInt(0, n, i32);
                        }
                    }
                    else
                    {
                        for (var n = 0; n < numrows; ++n)
                        {
                            t.SetLong(0, n, i);
                        }                        
                    }
                }

                if (type == TestType.Long)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetLong(0, n, i);
                    }
                }

                if (type == TestType.Float)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetFloat(0, n, i/32.0f);
                    }
                }

                if (type == TestType.Double)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetDouble(0, n, i / 32.0f);
                    }
                }

                if (type == TestType.String)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetString(0, n, GetNewString(size));
                    }
                }

                if (type == TestType.Date)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        t.SetDateTime(0, n, dt);
                    }
                }

                if (type == TestType.Customer)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        var customer = new Customer(n);
                        t.Add(customer.FirstName, customer.LastName, customer.Address, 
                            null, customer.discount,customer.Credit, customer.CreatedDate);

                        if (customer.PhoneNames != null)
                        {
                            using (var sub = t.GetSubTable(3, n))//get the subtable for the phone numbers
                            {
                                for (var phn = 0; phn < customer.PhoneNames.Count; ++phn)
                                {
                                    sub.Add(customer.PhoneNames[phn], customer.PhoneNumbers[phn]);
                                }
                            }
                        }
                    }
                }

                if (type == TestType.PriceBar)
                {
                    for (var n = 0; n < numrows; ++n)
                    {
                        var priceBar = new PriceBar(n);
                        t.Add(priceBar.timestamp, priceBar.lowestbid, priceBar.lowestask, priceBar.highestbid,
                            priceBar.highestask,
                            priceBar.openinglasttrade, priceBar.closinglasttrade, priceBar.openingbid,
                            priceBar.openingask, priceBar.closingbid, priceBar.closingask);
                    }
                }

                timer1.Stop();
                var after = getMemUsedAfter();
                var seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("{0,8}{1,8}{2,8}{3,5}{4,5}{5,5}{6,12}", "TightDb", numrows, type, size, seconds, milliseconds, after - before);

                //test a similar C# construct
                before = getMemUsedBefore();
                timer1 = Stopwatch.StartNew();
                
                var ShortList = new List<short>();
                var IntList = new List<int>();
                var longList = new List<long>();
                var floatList = new List<float>();
                var doubleList = new List<double>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();
                var customerList = new List<Customer>();
                var priceBarList = new List<PriceBar>();

                switch (type)
                {
                    case TestType.Int:
                        if (size > 4)
                        {
                            for (var n = 0; n < numrows; n++)
                            {
                                longList.Add(i);
                            }
                        }
                        else if (size > 2)
                        {
                            for (var n = 0; n < numrows; n++)
                            {
                                IntList.Add(i32);
                            }
                        }
                        else
                        {
                            for (var n = 0; n < numrows; n++)
                            {
                                ShortList.Add(i16);
                            }
                        }                


                break;
                    case TestType.String:
                        for (var n = 0; n < numrows; n++)
                        {
                            stringList.Add(GetNewString(size));
                        }
                        break;
                    case TestType.Float:
                        for (var n = 0; n < numrows; n++)
                        {
                            floatList.Add(n/32.0f);
                        }
                        break;
                    case TestType.Double:
                        for (var n = 0; n < numrows; n++)
                        {
                            doubleList.Add(n / 32.0d);
                        }
                        break;

                    case TestType.Date:
                        for (var n = 0; n < numrows; n++)
                        {
                            dateTimeList.Add(dt);
                        }
                        break;
                    case TestType.Customer:
                        for (var n = 0; n < numrows; ++n)
                        {
                            customerList.Add(new Customer(n));
                        }
                        break;
                    case TestType.PriceBar:
                        for (var n = 0; n < numrows; ++n)
                        {
                            priceBarList.Add(new PriceBar(n));
                        }
                        break;
                }

                timer1.Stop();
                after = getMemUsedAfter();
                seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("{0,8}{1,8}{2,8}{3,5}{4,5}{5,5}{6,12}", "C#", numrows, type, size, seconds, milliseconds, after - before);
            }
        }


        private static long Processmem()
        {
            return System.Diagnostics.Process.GetCurrentProcess().NonpagedSystemMemorySize64 +
                   System.Diagnostics.Process.GetCurrentProcess().PagedMemorySize64 +
                   System.Diagnostics.Process.GetCurrentProcess().PagedSystemMemorySize64 +
                   System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 +
                   System.Diagnostics.Process.GetCurrentProcess().VirtualMemorySize64;                                
            
        }
        private static long getMemUsedBefore()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return Processmem();            
        }

        private static long getMemUsedAfter()
        {
            return Processmem();
        }
        
        private static long MeasureSearchSpeed(TestType type, int size)
        {
            long temp = 0;
            using (var t = GetTableForType(type))
            {
                var dt = new DateTime(1980,1,1);
                var s = "".PadRight(size, 'x');//used to pad the string to target size
                long i = 256 * size;

                //fill table with searchable data
                for (var n = 0; n < 1000 * 1000; n++)
                {
                    switch (type)
                    {
                        case TestType.Int:
                            t.Add(n+i);
                            break;
                        case TestType.String:
                            t.Add(n.ToString(CultureInfo.InvariantCulture)+s);
                            break;
                        case TestType.Date:
                            t.Add(dt.AddMilliseconds(n));
                            break;
                    }
                }


                //search for the last 1000 inserted rows
                var timer1 = Stopwatch.StartNew();                
                for (var n = 1000*1000; n > 1000*1000 - 1000; n--)
                    
                switch (type)
                {
                    case TestType.Int:
                         temp = t.FindFirstInt(0,n+i);
                        break;
                    case TestType.String:
                         temp = t.FindFirstString(0,n.ToString(CultureInfo.InvariantCulture)+s);
                        break;
                    case TestType.Date:
                         temp = t.FindFirstDateTime(0,dt.AddMilliseconds(n));
                        break;
                }

                timer1.Stop();
                double seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("{0,20}{1,10}{2,5}{3,5}","Table.FindFirst",type, seconds,milliseconds);

                //test a similar C# construct
                var longList = new List<long>();
                var stringList = new List<string>();
                var dateTimeList = new List<DateTime>();

                for (var n = 0; n < 1000 * 1000; n++)
                {
                    switch (type)
                    {
                        case TestType.Int:
                            longList.Add(n);
                            break;
                        case TestType.String:
                            stringList.Add(n.ToString(CultureInfo.InvariantCulture)+s);
                            break;
                        case TestType.Date:
                            dateTimeList.Add(dt.AddMilliseconds(n));
                            break;
                    }
                }
                
                //search for the last 10000 inserted rows
                 timer1 = Stopwatch.StartNew();                
                for (var n = 1000 * 1000; n > 1000 * 1000 - 1000; n--)

                    switch (type)
                    {
                        case TestType.Int:
                            temp = longList.IndexOf(n);
                            break;
                        case TestType.String:
                            temp =  stringList.IndexOf(n.ToString(CultureInfo.InvariantCulture))  ;
                            break;
                        case TestType.Date:
                            temp = dateTimeList.IndexOf(dt.AddMilliseconds(n));
                            break;
                    }

                timer1.Stop();
                seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                milliseconds = timer1.Elapsed.Milliseconds;
                Console.WriteLine("{0,20}{1,10}{2,5}{3,5}","C#List.Find", type, seconds, milliseconds);

            }
            return temp;
        }

        private static void MeasureGetSizeSpeed()
        {
            var before = getMemUsedBefore();

            using (var t = new Table(new StringColumn("testfield")))
            {
                var timer1 = Stopwatch.StartNew();
                long acc = 0;
                for (var n = 0; n < 1000*100; n++)
                {
                    if (n%10 == 0)
                    {
                        t.AddEmptyRow(1);
                    }
                    acc = acc +  t.Size;
                }
                timer1.Stop();
                var seconds = Math.Floor(timer1.Elapsed.TotalSeconds);
                double milliseconds = timer1.Elapsed.Milliseconds;
                var after = getMemUsedAfter();
                Console.WriteLine("Table.Size sec:{0} millisec:{1} Res:{2}  mem:{3}", seconds, milliseconds, acc,after-before);
                Console.WriteLine(t.Size);//keep t alive until after we measured its memory footprint
            }

            
            var timer2 = Stopwatch.StartNew();
            long acc2 = Toolbox.TestNativeSizeCalls();
            timer2.Stop();
            var seconds2 = Math.Floor(timer2.Elapsed.TotalSeconds);
            double milliseconds2 = timer2.Elapsed.Milliseconds;
            Console.WriteLine("100K calls to Table.Size, coded in C++: {0} seconds, {1} milliseconds. Result{2}", seconds2, milliseconds2, acc2);            
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
                    Console.WriteLine("{0,8}{1,8}{2,8}{3,5}{4,5}{5,5}{6,12}", "System", "Rows", "Type", "Size", "Sec.", "Msec.", "Memory");
                    MeasureInsertSpeed(TestType.Float, 0); 
                    MeasureInsertSpeed(TestType.Double, 0);
                    MeasureInsertSpeed(TestType.PriceBar, 0);//size param is not used with customer
                    MeasureInsertSpeed(TestType.Customer, 0);//size param is not used with pricebar
                    MeasureInsertSpeed(TestType.Int, 1);
                    MeasureInsertSpeed(TestType.Int, 2);
                    MeasureInsertSpeed(TestType.Int, 3);
                    MeasureInsertSpeed(TestType.Int, 4);
                    MeasureInsertSpeed(TestType.Int, 5);
                    MeasureInsertSpeed(TestType.Int, 6);
                    MeasureInsertSpeed(TestType.Int, 7);
                    MeasureInsertSpeed(TestType.String, 4);
                    MeasureInsertSpeed(TestType.String, 8);
                    MeasureInsertSpeed(TestType.String, 16);
                    MeasureInsertSpeed(TestType.String, 32);
                    MeasureInsertSpeed(TestType.String, 64);
                    MeasureInsertSpeed(TestType.String, 128);
                    MeasureInsertSpeed(TestType.Date, 0);
                    loop = true;
                }
                if (ki.Key == ConsoleKey.D3)
                {
                    MeasureGetSizeSpeed();
                    loop = true;
                }
                if (ki.Key == ConsoleKey.D4)
                {
                   // long temp = 0;
                    Console.WriteLine("{0,20}{1,10}{2,5}{3,5}","Operation", "type","Sec.","msec");
                    /*temp=temp+*/MeasureSearchSpeed(TestType.Int,4);
                    /*temp = temp -*/
                     MeasureSearchSpeed(TestType.String, 0);
                    /*temp = temp + */MeasureSearchSpeed(TestType.Date, 0);
                    
              /*      if (temp==42)
                        Console.WriteLine(temp);//this never happes, but compiler has to calculate temp all the way*/
                    loop = true;
                }
            }
        }
    }
}

