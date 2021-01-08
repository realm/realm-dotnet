#!/bin/bash

appPath="bin/iPhoneSimulator/Release/Benchmarks.iOS.app"
id="test-simulator"
bundleId="io.realm.benchmarks"
runtimeId="iOS14.3"
benchmarkArguments="--headless -f \"QueryTests\" --join"

msbuild -p:Platform=iPhoneSimulator -p:Configuration=Release /restore

xcrun simctl create ${id} com.apple.CoreSimulator.SimDeviceType.iPhone-8 ${runtimeId}
xcrun simctl boot ${id}
xcrun simctl install ${id} ${appPath}
xcrun simctl launch --console-pty ${id} ${bundleId} ${benchmarkArguments}

xcrun simctl shutdown ${id}
xcrun simctl delete ${id}