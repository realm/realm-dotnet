////////////////////////////////////////////////////////////////////////////
//
// Copyright 2016 Realm Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil.Cil;
using RealmWeaver;
using static RealmWeaver.Analytics;

public partial class ModuleWeaver : Fody.BaseModuleWeaver, ILogger
{
    public override void Execute()
    {
        var targetFramework = ModuleDefinition.Assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
        if (targetFramework == null)
        {
            WriteError($"Failed to determine the target framework of {ModuleDefinition.Assembly.Name}. This is likely because GenerateTargetFrameworkAttribute is " +
                $"set to false in your MSBuild project. Either set it to true or manually add a [TargetFramework(...)] attribute to your assembly.");
            return;
        }

        var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);

        var weaver = new Weaver(ModuleDefinition, this, frameworkName.Identifier);

        var executionResult = weaver.Execute(GetAnalyticsConfig(frameworkName));
        WriteInfo(executionResult.ToString());
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System";
        yield return "System.Runtime";
        yield return "System.Core";
        yield return "netstandard";
        yield return "System.Collections";
        yield return "System.ObjectModel";
        yield return "System.Threading";
    }

    private Config GetAnalyticsConfig(FrameworkName frameworkName)
    {
        var analyticsCollection = bool.TryParse(Config.Attribute("DisableAnalytics")?.Value, out var result) && result ? AnalyticsCollection.Disabled : AnalyticsCollection.Full;

        if (analyticsCollection != AnalyticsCollection.Disabled)
        {
            if (Enum.TryParse<AnalyticsCollection>(Config.Attribute("AnalyticsCollection")?.Value, out var collection))
            {
                analyticsCollection = collection;
            }
#if DEBUG
            else
            {
                analyticsCollection = AnalyticsCollection.DryRun;
            }
#endif
        }

        var config = new Config
        {
            Framework = "xamarin", // This is for backwards compatibility
            AnalyticsCollection = analyticsCollection,
            AnalyticsLogPath = Config.Attribute("AnalyticsLogPath")?.Value,
        };

        config.FrameworkVersion = frameworkName.Version.ToString();
        config.TargetOSName = GetTargetOSName(frameworkName);

        // For backward compatibility
        config.TargetOSVersion = frameworkName.Version.ToString();

        return config;
    }

    private static string GetTargetOSName(FrameworkName frameworkName)
    {
        try
        {
            // Legacy reporting used ios, osx, and android
            switch (frameworkName.Identifier)
            {
                case "Xamarin.iOS":
                    return "ios";
                case "Xamarin.Mac":
                    return "osx";
                case "MonoAndroid":
                case "Mono.Android":
                    return "android";
            }

            if (frameworkName.Identifier.EndsWith("-android", StringComparison.OrdinalIgnoreCase))
            {
                return "android";
            }

            if (frameworkName.Identifier.EndsWith("-ios", StringComparison.OrdinalIgnoreCase))
            {
                return "ios";
            }

            if (frameworkName.Identifier.EndsWith("-maccatalyst", StringComparison.OrdinalIgnoreCase))
            {
                return "osx";
            }
        }
        catch
        {
#if DEBUG
            // Make sure we get build failures and address the problem in debug,
            // but don't fail users' builds because of that.
            throw;
#endif
        }

        return "windows";
    }

    void ILogger.Debug(string message)
    {
        WriteDebug(message);
    }

    void ILogger.Info(string message)
    {
        WriteInfo(message);
    }

    void ILogger.Error(string message, SequencePoint sequencePoint)
    {
        WriteError(message, sequencePoint);
    }

    void ILogger.Warning(string message, SequencePoint sequencePoint)
    {
        WriteWarning(message, sequencePoint);
    }
}
