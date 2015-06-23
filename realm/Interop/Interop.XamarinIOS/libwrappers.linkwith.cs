using System;
using ObjCRuntime;

[assembly: LinkWith ("libwrappers.a", LinkTarget.ArmV7 | LinkTarget.Arm64, SmartLink = true, ForceLoad = true)]
