using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.TestTools;
using UnityEngine;
using UnityEngine.TestTools;

[assembly: TestPlayerBuildModifier(typeof(HeadlessPlayModeSetup))]
[assembly: PostBuildCleanup(typeof(HeadlessPlayModeSetup))]
public class HeadlessPlayModeSetup : ITestPlayerBuildModifier, IPostBuildCleanup
{
    private static bool s_RunningPlayerTests;
    public BuildPlayerOptions ModifyOptions(BuildPlayerOptions playerOptions)
    {
        // Do not launch the player after the build completes.
        playerOptions.options &= ~BuildOptions.AutoRunPlayer;
        playerOptions.options |= BuildOptions.AllowDebugging;

        // The settings file controls things like the backend or API compatibility, so we want to put
        // artifacts with different settings files in different folders.
        var buildDifferentiatior = GetBuildDifferentiatorSettingsFileName();

        // Set the headlessBuildLocation to the output directory you desire. It does not need to be inside the project.
        var headlessBuildLocation = Path.GetFullPath(Path.Combine(Application.dataPath, $".//..//PlayModeTestPlayer_{buildDifferentiatior}"));
        var fileName = Path.GetFileName(playerOptions.locationPathName);
        if (!string.IsNullOrEmpty(fileName))
        {
            headlessBuildLocation = Path.Combine(headlessBuildLocation, fileName);
        }

        playerOptions.locationPathName = headlessBuildLocation;

        Debug.Log($"Build artifacts will be output to {playerOptions.locationPathName}");

        // Instruct the cleanup to exit the Editor if the run came from the command line. 
        // The variable is static because the cleanup is being invoked in a new instance of the class.
        s_RunningPlayerTests = true;
        return playerOptions;
    }

    public void Cleanup()
    {
        if (s_RunningPlayerTests && IsRunningTestsFromCommandLine())
        {
            // Exit the Editor on the next update, allowing for other PostBuildCleanup steps to run.
            EditorApplication.update += () => { EditorApplication.Exit(0); };
        }
    }

    private static bool IsRunningTestsFromCommandLine()
    {
        var commandLineArgs = Environment.GetCommandLineArgs();
        return commandLineArgs.Any(value => value == "-runTests");
    }

    private static string GetBuildDifferentiatorSettingsFileName()
    {
        var commandLineArgs = Environment.GetCommandLineArgs();

        for (var i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "-testSettingsFile" && (i + 1) < commandLineArgs.Length)
            {
                var settings = commandLineArgs[i + 1].Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
                return Path.GetFileNameWithoutExtension(settings);
            }
        }

        return "unknown";
    }
}
