import * as core from "@actions/core";
import * as exec from "@actions/exec";
import { v4 as uuidv4 } from "uuid";

async function run(): Promise<void> {
    const id = uuidv4();

    try {
        const appPath = core.getInput("appPath", { required: true });
        const bundleId = core.getInput("bundleId", { required: true });
        const iphoneToSimulate = core.getInput("iphoneToSimulate", { required: false });
        const args = core.getInput("arguments", { required: false });

        let runtimeId = await execCmd("xcrun simctl list runtimes");

        // Sample output: iOS 14.5 (14.5 - 18E182) - com.apple.CoreSimulator.SimRuntime.iOS-14-5
        // and we want to extract "iOS 14.5" and "com.apple.CoreSimulator.SimRuntime.iOS-14-5"
        // If we want to allow launching watchOS/tvOS simulators, replace the 'iOS' with an 'os' argument
        const matches =/(?<runtime1>iOS \d{1,2}(.\d{1,2})?).*(?<runtime2>com\.apple\.CoreSimulator\.SimRuntime\.iOS-[0-9.-]+)/g.exec(runtimeId);
        if (!matches?.groups?.runtime1 || !matches?.groups?.runtime2) {
            core.setFailed(`Impossible to fetch a runtime. Check runtimes and retry.\n${runtimeId}`);
            return;
        }
        core.info(`runtimeId: ${runtimeId}`);

        try {
            runtimeId = matches.groups.runtime1.replace(" ", "");
            await execCmd(`xcrun simctl create ${id} com.apple.CoreSimulator.SimDeviceType.${iphoneToSimulate} ${runtimeId}`);
        }
        catch {
            // Different combinantions of xcode and macOS versions have shown different syntax acceptance about the runtime, therefore 1 last attempt with a different syntax.
            runtimeId = matches.groups.runtime2;
            await execCmd(`xcrun simctl create ${id} com.apple.CoreSimulator.SimDeviceType.${iphoneToSimulate} ${runtimeId}`);
        }

        await execCmd(`xcrun simctl boot ${id}`);
        await execCmd(`xcrun simctl install ${id} ${appPath}`);
        await execCmd(`xcrun simctl launch --console-pty ${id} ${bundleId} ${args}`);
    } catch (error) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

async function execCmd(cmd: string): Promise<string> {
    let stdout = "";
    let stderr = "";
    const options : exec.ExecOptions = {};
    options.listeners = {
        stdout: (data: Buffer) => {
            stdout += data.toString();
        },
        stderr: (data: Buffer) => {
            stderr += data.toString();
        }
    };

    const exitCode = await exec.exec(cmd, [], options);
    if (exitCode != 0) {
        throw new Error(`"${cmd}" failed with code ${exitCode} giving error:\n ${stderr}`);
    }

    return stdout;
}

run();

export default run;
