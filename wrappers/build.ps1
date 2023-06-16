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

    [ValidateSet('Win32', 'x64', 'ARM', 'ARM64')]
    [string[]]$Platforms = ('Win32'),

    [ValidateSet('Windows', 'WindowsStore')]
    [Parameter(Position = 0)]
    [string]$Target = 'Windows',

    [Switch]$Incremental,

    [Switch]$EnableLTO,

    [string]$ToolsetVersion = ''
)

Push-Location $PSScriptRoot

if (!(Get-Module -ListAvailable -Name VSSetup)) {
    Install-PackageProvider NuGet -Scope CurrentUser -Force
    Import-PackageProvider NuGet -Force
    Set-PSRepository -Name PSGallery -InstallationPolicy Trusted
    Install-Module -Name VSSetup -Scope CurrentUser -Force
}

$vs = Get-VSSetupInstance | Select-VSSetupInstance -Product * -Latest -Require Microsoft.VisualStudio.Component.VC.CMake.Project

# Work-around for Visual Studio 16.0 and CMake 3.13
$Env:path += ";$($vs.InstallationPath)\MSBuild\Current\Bin"

$cmake = Join-Path $vs.InstallationPath -ChildPath "Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
$cmakeArgs = "-DCMAKE_GENERATOR_INSTANCE=$($vs.InstallationPath)",
             "-DCMAKE_BUILD_TYPE=$Configuration",
             "-DCMAKE_SYSTEM_NAME=$Target",
             "-DCMAKE_INSTALL_PREFIX=$PSScriptRoot\build",
             "-DCMAKE_SYSTEM_VERSION=10.0"

if ($EnableLTO) {
    $cmakeArgs += "-DCMAKE_INTERPROCEDURAL_OPTIMIZATION=ON"
}

function triplet([string]$target, [string]$platform) {
    $arch = $platform.ToLower()
    if ($arch -eq 'win32') {
        $arch = 'x86'
    }

    $plat = $target.ToLower()
    if ($plat -eq 'windowsstore') {
        $plat = 'uwp'
    }

    return "$arch-$plat-static"
}

foreach ($platform in $Platforms) {
    if (-Not $Incremental) {
        Remove-Item .\cmake\$Target\$Configuration-$platform -Recurse -Force -ErrorAction Ignore
    }
    New-Item .\cmake\$Target\$Configuration-$platform -ItemType "Directory" | Out-Null
    Push-Location .\cmake\$Target\$Configuration-$platform

    if ($ToolsetVersion -ne '') {
        $cmakeArgs += "-T v143,version=$ToolsetVersion"
    }

    if (-Not $Incremental) {
        & $cmake $PSScriptRoot $cmakeArgs -DCMAKE_GENERATOR_PLATFORM="$platform"
    }

    & $cmake --build . --target install --config $Configuration
    Pop-Location
}
