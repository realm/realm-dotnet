using System;
using ObjCRuntime;

[assembly: LinkWith ("libsilly2.a", LinkTarget.Simulator, SmartLink = true, ForceLoad = true)]
