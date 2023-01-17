param(
    [string]$ExtraAppArgs = ""
)

$PackageLocation = "$PSScriptRoot\AppPackages\Tests.UWP_1.0.0.0_Test"

if (-not (Test-Path -Path $PackageLocation)) {
    $PackageLocation = "$PSScriptRoot\AppPackages\Tests.UWP_1.0.0.0_Debug_Test"
}

if (-not (Test-Path -Path $PackageLocation)) {
    Write-Output "Tests package not found at $PackageLocation"
    exit 1
}

if (-not (Test-Path -Path "$PackageLocation/Install.ps1")) {
    Write-Output "Install.ps1 not found at $PackageLocation/Install.ps1"
    exit 2
}

& $PackageLocation/Install.ps1 -Force

$PackagePath = get-appxpackage -name realm.uwp.tests | Select-Object -expandproperty PackageFamilyName
$ResultsPath = "$env:LOCALAPPDATA\Packages\$PackagePath\LocalState\TestResults.UWP.xml"
$RunOutputPath = "$env:LOCALAPPDATA\Packages\$PackagePath\LocalState\TestRunOutput.txt"

$AppArgs = "--headless --labels=After --result=TestResults.UWP.xml $ExtraAppArgs"

Write-Output "Launching shell:AppsFolder\$PackagePath!App with arguments: $AppArgs"

Start-Process "shell:AppsFolder\$PackagePath!App" -ArgumentList "$AppArgs"
Write-Output "The test application is launched, this step is monitoring it and it will terminate when the tests are fully run"

do {
    Start-Sleep -s 3

    if (!$TestAppProcess) {
        $TestAppProcess = Get-Process Tests.UWP -ErrorAction SilentlyContinue
    }
} while (!($TestAppProcess -and $TestAppProcess.HasExited ) -and !(Test-Path -Path $ResultsPath))

Write-Output "Test run completed with exit code $($TestAppProcess.ExitCode)"

if (Test-Path -Path $RunOutputPath) {
    Write-Output "Found run output at $RunOutputPath"
    Get-Content $RunOutputPath
}

if (-not (Test-Path -Path $ResultsPath)) {
    Write-Output "Failed to find results file at: $ResultsPath, exiting."
    exit 3
}

Write-Output "Results file is located at $ResultsPath"

echo "TEST_RESULTS=$ResultsPath" | Out-File $env:GITHUB_ENV -Encoding utf8 -Append