import * as core from "@actions/core";
import * as exec from "@actions/exec";
import { v4 as uuidv4 } from 'uuid';

async function run(): Promise<void> {
    const id = uuidv4().replace("-", "");

    try {
        const appPath = core.getInput("appPath", { required: true });
        const bundleId = core.getInput("bundleId", { required: true });
        const iphoneToSimulate = core.getInput("iphoneToSimulate", { required: false });
        const args = core.getInput("arguments", { required: false });
        
        core.info(`iphoneToSimulate: ${iphoneToSimulate}`);
        // Sample output: iOS 10.3 (10.3 - 14E269) (com.apple.CoreSimulator.SimRuntime.iOS-10-3) - we're looking for '10.3 - 14E269'
        // If we want to allow launching watchOS/tvOS simulators, replace the 'iOS' with an 'os' argument
        if (await exec.exec("xcrun simctl list runtimes") != 0) core.setFailed(`list runtimes failed`);

        let runtimeId: String = "";
        const options : exec.ExecOptions = {};
        options.listeners = {
        stdout: (data: Buffer) => {
            runtimeId += data.toString();
        },
        };
        core.info(`runtimeId: ${runtimeId}`);
        if (await exec.exec("xcrun simctl list runtimes | awk '/com.apple.CoreSimulator.SimRuntime.iOS/ { match($0, /com.apple.CoreSimulator.SimRuntime.iOS-[0-9.-]+/); print substr($0, RSTART, RLENGTH); exit }'") != 0) core.setFailed(`create simulator failed`);
        if (!runtimeId) {
            if (await exec.exec("xcrun simctl list runtimes | awk '/com.apple.CoreSimulator.SimRuntime.iOS/ { match($0, /([0-9.]+ - [a-zA-Z0-9]+)/); print substr($0, RSTART + 1, RLENGTH - 2); exit }'") != 0) core.setFailed(`create simulator failed`);
        }

        // exec.exec("xcrun", ["simctl", "create", id, "com.apple.CoreSimulator.SimDeviceType." + iphoneToSimulate, runtimeId.toString()]);
        if (await exec.exec("xcrun", ["simctl", "create", id, "com.apple.CoreSimulator.SimDeviceType.iPhone-8", runtimeId.toString()]) != 0) core.setFailed(`create simulator failed`);
        if (await exec.exec("xcrun", ["simctl", "boot", id]) != 0) core.setFailed(`boot simulator failed`);
        if (await exec.exec("xcrun", ["simctl", "install", id, appPath]) != 0) core.setFailed(`install app on  simulator failed`);
        if (await exec.exec("xcrun", ["simctl", "launch", "--console-pty", id, bundleId, args]) != 0) core.setFailed(`launch app on simulator failed`);
        // core.info(`The result is: ${sum}`);

        // core.setOutput("output", sum);
    } catch (error) {
        core.setFailed(error.message);
    } finally {
        try {
            if (await exec.exec("xcrun", ["simctl", "shutdown", id]) != 0) core.setFailed(`launch app on simulator failed`);
            if (await exec.exec("xcrun", ["simctl", "delete", id]) != 0) core.setFailed(`launch app on simulator failed`);
        } catch (error) {
            core.setFailed(`An error occurred during cleanup: ${error.toString()}`);
        }
    }
}

// export function runSimulator(id: number, runtimeId: number, bundleId: number, appPath: number, iphoneToSimulate: string): void {
    
// }

run();

export default run;
