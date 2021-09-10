param(
    [string]$PackageLocation = "$PSScriptRoot/AppPackages/Tests.UWP_1.0.0.0_Test"
)

if (-not (Test-Path -Path $PackageLocation)) {
    Write-Output "Tests package not found at $PackageLocation"
    exit 1
}

if (-not (Test-Path -Path "$PackageLocation/Install.ps1")) {
    Write-Output "Install.ps1 not found at $PackageLocation/Install.ps1"
    exit 2
}

& $PackageLocation/Install.ps1 -Force

$PackagePath = get-appxpackage -name realm.uwp.tests | select -expandproperty PackageFamilyName
$ResultsPath = "$env:LOCALAPPDATA/Packages/$PackagePath/LocalState/TestResults.UWP.xml"
$RunOutputPath = "$env:LOCALAPPDATA/Packages/$PackagePath/LocalState/TestRunOutput.txt"

$TestAppProcess = Start-Process "shell:AppsFolder\$PackagePath!App" -ArgumentList "--headless --labels=After --result=TestResults.UWP.xml" -PassThru -RedirectStandardOutput pwshout.txt -RedirectStandardError pwsherr.txt
Write-Output "The test application is launched, this step is monitoring it and it will terminate when the tests are fully run"

do
{
    Start-Sleep -s 3
} while (-not $TestAppProcess.HasExited -and -not (Test-Path -Path $ResultsPath))

Write-Output "Test run completed with exit code $TestAppProcess.ExitCode"

if (Test-Path -Path pwshout.txt) {
    Write-Output "Found powershell output at pwshout.txt"
    Get-Content $RunOutputPath
}

if (Test-Path -Path pwsherr.txt) {
    Write-Output "Found powershell error output at pwsherr.txt"
    Get-Content $RunOutputPath
}

if (Test-Path -Path $RunOutputPath) {
    Write-Output "Found run output at $RunOutputPath"
    Get-Content $RunOutputPath
}

if (-not (Test-Path -Path $ResultPath))
{
    Write-Output "Failed to find results file at: $ResultPath, exiting."
    exit 3
}

return $ResultsPath