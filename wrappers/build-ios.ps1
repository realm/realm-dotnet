#!/usr/bin/env pwsh

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [ValidateSet('Device', 'Simulator')]
    [Parameter(Position=0)]
    [string[]]$Platforms = ('Device', 'Simulator'),

    [Switch]$Incremental,

    [Switch]$EnableLTO,

    [Switch]$SkipXCFramework
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot

$build_directory = "$PSScriptRoot/cmake/iOS"
$install_prefix = "$PSScriptRoot/build"

New-Item $build_directory -ItemType Directory -Force -ErrorAction Ignore > $null
Push-Location $build_directory

if (-Not $Incremental) {
    Remove-Item * -Recurse -Force -ErrorAction Ignore > $null
    cmake "$PSScriptRoot" -DCMAKE_INSTALL_PREFIX="$install_prefix" -DCMAKE_BUILD_TYPE=$Configuration -GXcode `
        -DCMAKE_SYSTEM_NAME=iOS `
        -DCMAKE_XCODE_ATTRIBUTE_ONLY_ACTIVE_ARCH=NO `
        -DCMAKE_XCODE_ATTRIBUTE_DYLIB_INSTALL_NAME_BASE='@rpath' `
        -DCMAKE_TRY_COMPILE_TARGET_TYPE=STATIC_LIBRARY `
        -DCMAKE_TOOLCHAIN_FILE="$PSScriptRoot/realm-core/tools/cmake/ios.toolchain.cmake" `
        -DCMAKE_INTERPROCEDURAL_OPTIMIZATION=$EnableLTO
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

$destinations = @()
if ($Platforms.Contains('Simulator')) {
    $destinations += $('-destination', 'generic/platform=iOS Simulator')
}
if ($Platforms.Contains('Device')) {
    $destinations += $('-destination', 'generic/platform=iOS')
}

xcodebuild -scheme realm-wrappers -configuration $Configuration @destinations
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

if ($SkipXCFramework) {
    exit
}

$xcframework_path = "$install_prefix/iOS/$Configuration/realm-wrappers.xcframework"
Remove-Item $xcframework_path -Force -Recurse -ErrorAction Ignore > $null

$frameworks = @()
if ($Platforms.Contains('Simulator')) {
    $frameworks += $('-framework', "$build_directory/src/$Configuration-iphonesimulator/realm-wrappers.framework")
}
if ($Platforms.Contains('Device')) {
    $frameworks += $('-framework', "$build_directory/src/$Configuration-iphoneos/realm-wrappers.framework")
}

xcodebuild -create-xcframework @frameworks -output "$xcframework_path"
exit $LASTEXITCODE
