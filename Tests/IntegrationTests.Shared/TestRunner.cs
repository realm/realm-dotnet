/* Copyright 2015 Realm Inc - All Rights Reserved
 * Proprietary and Confidential
 */

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Internal;

namespace IntegrationTests.Shared
{
	internal class TestRunner
	{
		internal static void Run(string title, Stream outputStream)
		{
			var suite = new TestSuite (title);
			var builder = new NUnitLiteTestAssemblyBuilder ();
			suite.Add (builder.Build(System.Reflection.Assembly.GetExecutingAssembly(), new Dictionary<string, object>()));

			var testExecutionContext = TestExecutionContext.CurrentContext;
			testExecutionContext.WorkDirectory = Environment.CurrentDirectory;

			var workItem = suite.CreateWorkItem (TestFilter.Empty);
			workItem.Execute (testExecutionContext);

			var testWriter = new NUnitLite.Runner.NUnit2XmlOutputWriter (DateTime.Now);
			using (var writer = new StreamWriter (outputStream)) 
			{
				testWriter.WriteResultFile (workItem.Result, writer);
			}
		}
	}
}

