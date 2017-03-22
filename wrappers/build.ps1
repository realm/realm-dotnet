<#
     Copyright 2016 Realm Inc.
     Licensed under the Apache License, Version 2.0 (the "License");
     you may not use this file except in compliance with the License.
     You may obtain a copy of the License at
     http://www.apache.org/licenses/LICENSE-2.0
     Unless required by applicable law or agreed to in writing, software
     distributed under the License is distributed on an "AS IS" BASIS,
     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     See the License for the specific language governing permissions and
     limitations under the License.
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [ValidateSet('Win32', 'x64', 'ARM')]
    [string[]]$Platforms = ('Win32'),

    [ValidateSet('Windows', 'WindowsStore')]
    [Parameter(Position=0)]
    [string]$Target = 'Windows'
)

Push-Location $PSScriptRoot

if (!(Get-Module -ListAvailable -Name VSSetup)) {
    Install-Module -Name VSSetup -Scope CurrentUser -Force
}

$vs = Get-VSSetupInstance | Select-VSSetupInstance -Latest -Require Microsoft.VisualStudio.Component.VC.CMake.Project
$cmake = Join-Path $vs.InstallationPath -ChildPath "Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
$cmakeArgs = "-DCMAKE_SYSTEM_NAME=$Target", "-DCMAKE_INSTALL_PREFIX=$PSScriptRoot\build"

if ($Target -eq 'WindowsStore') {
    $cmakeArgs += "-DCMAKE_SYSTEM_VERSION='10.0'"
}

foreach ($platform in $Platforms) {
    Remove-Item .\cmake\$Target\$platform -Recurse -Force -ErrorAction Ignore
    New-Item .\cmake\$Target\$platform -ItemType "Directory" | Out-Null
    Push-Location .\cmake\$Target\$platform
    & $cmake $PSScriptRoot $cmakeArgs  -DCMAKE_GENERATOR_PLATFORM="$platform"
    & $cmake --build . --target install --config $Configuration
    Pop-Location
}