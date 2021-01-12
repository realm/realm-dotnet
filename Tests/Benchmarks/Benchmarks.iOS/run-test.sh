#!/bin/bash

appPath="bin/iPhoneSimulator/Release/Benchmarks.iOS.app"
simId="benchmark-simulator"
bundleId="io.realm.benchmarks"
runtimeId="iOS14.3"
simType="com.apple.CoreSimulator.SimDeviceType.iPhone-12"
resultPath=$(pwd)

benchmarkArguments="--headless --join --artifacts /Users/ferdinando.papale/MongoDB/realm-dotnet/Tests/Benchmarks/Benchmarks.iOS"

benchmarkArguments="--headless --join --artifacts ${resultPath} -f QueryTests"

msbuild -p:Platform=iPhoneSimulator -p:Configuration=Release /restore

xcrun simctl create ${simId} ${simType} ${runtimeId}
xcrun simctl boot ${simId}
xcrun simctl install ${simId} ${appPath}
xcrun simctl launch --console-pty ${simId} ${bundleId} ${benchmarkArguments}

xcrun simctl shutdown ${simId}
xcrun simctl delete ${simId}