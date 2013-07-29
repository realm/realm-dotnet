using System;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;
using TightDbCSharp;

[assembly: CLSCompliant(true)] //mark the public interface of this program as cls compliant (can be run from any .net language)

namespace TightDbCSharpTest
{
    static class TestHelper
    //various common functions that the unit tests use, mostly related to dumping of table contents and schema
    {
        //custom string assert that removes \r from expected as the text files have accidentially had their cr's removed by github
        //by removing \r from both files, CRLF or just LF differences will not break unit tests
        public static void Cmp(String s1, String s2)
        {
            Assert.AreEqual(s1.Replace("\r", ""), s2.Replace("\r", ""));
        }


        private static void PrintHeader(StringBuilder res, string tablename, long count)
        {
            res.AppendLine(Sectiondelimitor);
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Column count: {0}", count));
            res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table Name  : {0}", tablename));
            res.AppendLine(Sectiondelimitor);
        }


        private static void PrintMetadataFooter(StringBuilder res)
        {
            res.AppendLine(Sectiondelimitor);
            res.AppendLine("");
        }

        private const string Sectiondelimitor = "------------------------------------------------------";


        //dumps table structure to a string for debugging purposes.
        //the string is easily human-readable
        //this version uses the table column information as far as possible, then shifts to spec on subtables
        public static string TableDumper(String fileName, String tableName, TableOrView t)
        {
            var res = new StringBuilder(); //temporary storange of text of dump

            long count = t.ColumnCount;
            PrintHeader(res, tableName, count);
            for (long n = 0; n < count; n++)
            {
                string name = t.GetColumnName(n);
                DataType type = t.ColumnType(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0,2} {2,10}  {1,-20}", n, name, type));
                if (type == DataType.Table)
                {
                    Spec subSpec = t.Spec.GetSpec(n);
                    Specdumper(res, "   ", subSpec, "Subtable");
                }
            }
            PrintMetadataFooter(res);
            TableDataDumper("", res, t);

            Console.Write(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        private static void Specdumper(StringBuilder res, String indent, Spec s, string tableName)
        {

            long count = s.ColumnCount;

            if (String.IsNullOrEmpty(indent))
            {
                PrintHeader(res, tableName, count);
            }

            for (long n = 0; n < count; n++)
            {
                String name = s.GetColumnName(n);
                DataType type = s.GetColumnType(n);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, "{0}{1,2} {2,10}  {3,-20}", indent, n, type,
                    name));
                if (type == DataType.Table)
                {
                    Spec subspec = s.GetSpec(n);
                    Specdumper(res, indent + "   ", subspec, "Subtable");
                }
            }

            if (String.IsNullOrEmpty(indent))
            {
                PrintMetadataFooter(res);
            }
        }

        //dump the table only using its spec
        public static String TableDumperSpec(String fileName, String tablename, Table t)
        {
            var res = new StringBuilder();
            Specdumper(res, "", t.Spec, tablename);

            TableDataDumper("", res, t);
            Console.WriteLine(res.ToString());
            File.WriteAllText(fileName + ".txt", res.ToString());
            return res.ToString();
        }


        private static void TableDataDumper(string indent, StringBuilder res, TableOrView table)
        {
            const string startrow = "{{ //Start row {0}";
            const string endrow = "}} //End row {0}";
            const string startfield = @"{0}:";
            const string endfield = ",//column {0}{1}";
            const string endfieldlast = "//column {0}{1}"; //no comma
            const string starttable = "[ //{0} rows";
            const string endtable = "]";
            const string mixedcomment = "//Mixed type is {0}";
            var firstdatalineprinted = false;
            long tableSize = table.Size;
            foreach (Row tr in table)
            {
                if (firstdatalineprinted == false)
                {
                    if (String.IsNullOrEmpty(indent))
                    {
                        res.Append(indent);
                        res.AppendLine(String.Format(CultureInfo.InvariantCulture, "Table Data Dump. Rows:{0}",
                            tableSize));
                        res.Append(indent);
                        res.AppendLine(Sectiondelimitor);
                    }
                    firstdatalineprinted = true;
                }
                res.Append(indent);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, startrow, tr.RowIndex)); //start row marker

                foreach (RowColumn trc in tr)
                {
                    string extracomment = "";
                    res.Append(indent);
                    string name = trc.ColumnName;
                    //so we can see it easily in the debugger
                    res.Append(String.Format(CultureInfo.InvariantCulture, startfield, name));
                    if (trc.ColumnType == DataType.Table)
                    {
                        Table sub = trc.GetSubTable();
                        //size printed here as we had a problem once with size reporting 0 where it should be larger, so nothing returned from call
                        res.Append(String.Format(CultureInfo.InvariantCulture, starttable, sub.Size));
                        TableDataDumper(indent + "   ", res, sub);
                        res.Append(endtable);
                    }
                    else
                    {
                        if (trc.ColumnType == DataType.Mixed)
                        {
                            extracomment = string.Format(CultureInfo.InvariantCulture, mixedcomment, trc.MixedType);
                            //dumping a mixed with a simple value is done by simply calling trc.value - it will return the value inside the mixed
                            if (trc.MixedType == DataType.Table)
                            {
                                var sub = trc.Value as Table;
                                TableDataDumper(indent + "   ", res, sub);
                            }
                            else
                            {
                                res.Append(trc.Value);
                            }
                        }
                        else
                            res.Append(trc.Value);
                    }
                    res.Append(indent);
                    res.AppendLine(String.Format(CultureInfo.InvariantCulture,
                        trc.IsLastColumn() ? endfieldlast : endfield, trc.ColumnIndex,
                        extracomment));
                }
                res.Append(indent);
                res.AppendLine(String.Format(CultureInfo.InvariantCulture, endrow, tr.RowIndex)); //end row marker

            }
            if (firstdatalineprinted && String.IsNullOrEmpty(indent))
            //some data was dumped from the table, so print a footer
            {
                res.Append(indent);
                res.AppendLine(Sectiondelimitor);
            }
        }


    }
}
