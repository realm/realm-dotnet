$code = @"
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
interface IApplicationActivationManager
{
    IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] UInt32 options, [Out] out UInt32 processId);
}

[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
public class ApplicationActivationManager : IApplicationActivationManager
{
    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)/*, PreserveSig*/]
    public extern IntPtr ActivateApplication([In] String appUserModelId, [In] String arguments, [In] UInt32 options, [Out] out UInt32 processId);
     
}
"@
 
add-type -TypeDefinition $code
$appman = new-object ApplicationActivationManager

$appx = Get-AppxPackage "RealmTestsApp"
$id = $null;
$appman.ActivateApplication("$($appx.PackageFamilyName)!App", "--headless", 0, ([ref]$id))
$process = Get-Process -Id $id
Wait-Process -InputObject $process
$process.ExitCode