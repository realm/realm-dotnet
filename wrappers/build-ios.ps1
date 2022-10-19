#!/usr/bin/env pwsh

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [ValidateSet('Device', 'Simulator', 'Catalyst')]
    [Parameter(Position = 0)]
    [string[]]$Platforms = ('Simulator'),

    [Switch]$Incremental,

    [Switch]$EnableLTO
)

$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot

$build_directory = "$PSScriptRoot/cmake/iOS"

New-Item $build_directory -ItemType Directory -Force -ErrorAction Ignore > $null
Push-Location $build_directory

if (-Not $Incremental) {
    Remove-Item * -Recurse -Force -ErrorAction Ignore > $null
    cmake "$PSScriptRoot" -DCMAKE_BUILD_TYPE=$Configuration -GXcode `
        -DCMAKE_XCODE_ATTRIBUTE_DYLIB_INSTALL_NAME_BASE='@rpath' `
        -DCMAKE_TOOLCHAIN_FILE="$PSScriptRoot"'/realm-core/tools/cmake/xcode.toolchain.cmake' `
        -DCMAKE_LIBRARY_OUTPUT_DIRECTORY="$PSScriptRoot"'/build/$(PLATFORM_NAME)/$<CONFIG>' `
        -DCMAKE_INTERPROCEDURAL_OPTIMIZATION="$EnableLTO"
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
if ($Platforms.Contains('Catalyst')) {
    $destinations += $('-destination', 'generic/platform=macOS,variant=Mac Catalyst')
}

xcodebuild -scheme realm-wrappers -configuration $Configuration @destinations
exit $LASTEXITCODE
