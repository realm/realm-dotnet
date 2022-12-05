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

        var disableAnalytics = bool.TryParse(Config.Attribute("DisableAnalytics")?.Value, out var result) && result;
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

        config.FrameworkVersion = frameworkName.Version.ToString();
        config.TargetOSName = AnalyticsUtils.GetTargetOsName(frameworkName);

        // For backward compatibility
        config.TargetOSVersion = frameworkName.Version.ToString();

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
