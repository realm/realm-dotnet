using NUnit.Framework.Interfaces;
using Realms.Tests;
using System;
using System.Globalization;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEngine.TestRunner;

[assembly: TestRunCallback(typeof(TestManager))]
public class TestManager : ITestRunCallback
{
    public void RunFinished(ITestResult result)
    {
        var total = result.FailCount + result.PassCount + result.InconclusiveCount;
        var message = $"Test run finished: {total} Passed: {result.PassCount} Failed: {result.FailCount} Inconclusive: {result.InconclusiveCount} ({result.EndTime - result.StartTime:c})";
        HackyLogger.Log(message, important: true);

        OutputResults(result);
        if (!Application.isEditor)
        {
            Application.Quit();
        }
    }

    public void RunStarted(ITest testsToRun)
    {
        HackyLogger.Log($"Test arguments: {string.Join(", ", Environment.GetCommandLineArgs())}");
        HackyLogger.Log($"Test run started: {testsToRun.TestCaseCount} total tests", important: true);
    }

    public void TestFinished(ITestResult result)
    {
        if (!result.Test.IsSuite)
        {
            var className = result.Test.ClassName?.Split('.').LastOrDefault();
            var status = result.ResultState.Status.ToString().ToUpper();
            var message = $"\t[{status}] {className}.{result.Test.Name} ({(result.EndTime - result.StartTime).TotalMilliseconds} ms)";

            if (result.ResultState.Status == TestStatus.Failed)
            {
                message += $"{Environment.NewLine}{result.Message} - {result.StackTrace}";
            }

            HackyLogger.Log(message);
        }
    }

    public void TestStarted(ITest test)
    {
    }

    private static class HackyLogger
    {
        public static void Log(string message, bool important = false)
        {
            var logType = important && !Application.isEditor ? LogType.Error : LogType.Log;
            var timePrefix = DateTimeOffset.UtcNow.ToString("HH:mm:ss");
            Debug.LogFormat(logType, LogOption.NoStacktrace, null, $"{timePrefix} {message.Replace("{", "{{").Replace("}", "}}")}");
        }
    }

    private static void OutputResults(ITestResult result)
    {
        var resultPath = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--result="))?.Replace("--result=", string.Empty);
        if (resultPath == null)
        {
            HackyLogger.Log("No results path found, not transforming results.");
            return;
        }

        using (var writer = XmlWriter.Create(resultPath))
        {
            var resultNode = result.ToXml(recursive: true);
            StripSystemOut(resultNode);

            var testNode = MakeTestRunElement(result);
            testNode.ChildNodes.Add(resultNode);

            testNode.WriteTo(writer);

            writer.Flush();
        }

        HackyLogger.Log($"Results path is {resultPath}, transforming results to junit.");
        TestHelpers.TransformTestResults(resultPath);
    }

    // This is deliberately identical to NUnitLite to match other test results we generate:
    // https://github.com/nunit/nunit/blob/34a39b3f7ce1ae49ed7d7bcddaec9573c6364f97/src/NUnitFramework/nunitlite/OutputWriters/NUnit3XmlOutputWriter.cs#L75
    private static TNode MakeTestRunElement(ITestResult result)
    {
        var testRun = new TNode("test-run");

        testRun.AddAttribute("id", "2");
        testRun.AddAttribute("name", result.Name);
        testRun.AddAttribute("fullname", result.FullName);
        testRun.AddAttribute("testcasecount", result.Test.TestCaseCount.ToString());

        testRun.AddAttribute("result", result.ResultState.Status.ToString());
        if (result.ResultState.Label != string.Empty)
            testRun.AddAttribute("label", result.ResultState.Label);

        testRun.AddAttribute("start-time", result.StartTime.ToString("o"));
        testRun.AddAttribute("end-time", result.EndTime.ToString("o"));
        testRun.AddAttribute("duration", result.Duration.ToString("0.000000", NumberFormatInfo.InvariantInfo));

        var total = result.FailCount + result.PassCount + result.InconclusiveCount;
        testRun.AddAttribute("total", total.ToString());
        testRun.AddAttribute("passed", result.PassCount.ToString());
        testRun.AddAttribute("failed", result.FailCount.ToString());
        testRun.AddAttribute("inconclusive", result.InconclusiveCount.ToString());
        testRun.AddAttribute("skipped", result.SkipCount.ToString());
        testRun.AddAttribute("asserts", result.AssertCount.ToString());

        return testRun;
    }

    private static void StripSystemOut(TNode node)
    {
        var outputNode = node.ChildNodes.FirstOrDefault(n => n.Name == "output");
        if (outputNode != null)
        {
            var didPass = node.Attributes.FirstOrDefault(a => a.Key == "result").Value == "Passed";
            if (didPass)
            {
                node.ChildNodes.Remove(outputNode);
            }
        }
        else
        {
            foreach (var child in node.ChildNodes)
            {
                StripSystemOut(child);
            }
        }
    }
}
