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

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Mono.Cecil.Cil;
using RealmWeaver;

public partial class ModuleWeaver : Fody.BaseModuleWeaver, ILogger
{
    public override void Execute()
    {
        var targetFramework = ModuleDefinition.Assembly.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == typeof(TargetFrameworkAttribute).FullName);
        var frameworkName = new FrameworkName((string)targetFramework.ConstructorArguments.Single().Value);

        var weaver = new Weaver(ModuleDefinition, this, frameworkName);

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

    private Analytics.Config GetAnalyticsConfig(FrameworkName frameworkName)
    {
        var disableAnalytics = bool.TryParse(Config.Attribute("DisableAnalytics")?.Value, out var result) && result;

        var config = new Analytics.Config
        {
            Framework = "xamarin", // This is for backwards compatibility
            RunAnalytics = !disableAnalytics,
        };

        var version = "UNKNOWN";

        // Default to windows for backward compatibility
        var name = "windows";

        try
        {
            // Legacy reporting used ios, osx, and android
            switch (frameworkName.Identifier)
            {
                case "Xamarin.iOS":
                    name = "ios";
                    break;
                case "Xamarin.Mac":
                    name = "osx";
                    break;
                case "MonoAndroid":
                case "Mono.Android":
                    name = "android";
                    break;
                default:
                    name = frameworkName.Identifier;
                    break;
            }

            version = frameworkName.Version.ToString();
        }
        catch
        {
#if DEBUG
            // Make sure we get build failures and address the problem in debug,
            // but don't fail users' builds because of that.
            throw;
#endif
        }

        config.FrameworkVersion = version;
        config.TargetOSName = name;

        // For backward compatibility
        config.TargetOSVersion = version;

        return config;
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
