using System.Linq;
using NUnit.Framework.Interfaces;
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
    }

    public void RunStarted(ITest testsToRun)
    {
        HackyLogger.Log($"Test run started: {testsToRun.TestCaseCount} total tests", important: true);
    }

    public void TestFinished(ITestResult result)
    {
        if (!result.Test.IsSuite)
        {
            var className = result.Test.ClassName?.Split('.').LastOrDefault();
            var status = result.ResultState.Status.ToString().ToUpper();
            var message = $"\t[{status}] {className}.{result.Test.Name} ({(result.EndTime - result.StartTime).TotalMilliseconds} ms)";

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
            Debug.LogFormat(logType, LogOption.NoStacktrace, null, message.Replace("{", "{{").Replace("}", "}}"));
        }
    }
}
