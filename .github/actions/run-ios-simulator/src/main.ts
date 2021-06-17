import * as core from "@actions/core";
import * as exec from "@actions/exec";
import { v4 as uuidv4 } from "uuid";
import * as childProcess from "promisify-child-process";

async function run(): Promise<void> {
    const id = uuidv4().split("-").join("");

    try {
        const appPath = core.getInput("appPath", { required: true });
        const bundleId = core.getInput("bundleId", { required: true });
        const iphoneToSimulate = core.getInput("iphoneToSimulate", { required: false });
        const args = core.getInput("arguments", { required: false });
        
        let runtimeId = "";
        const options : exec.ExecOptions = {};
        options.listeners = {
            stdout: (data: Buffer) => {
                runtimeId += data.toString();
            },
        };

        await execCmd("xcrun simctl list runtimes", options);

        // Sample output: iOS 14.5 (14.5 - 18E182) - com.apple.CoreSimulator.SimRuntime.iOS-14-5
        // and we want to extract "iOS 14.5"
        // If we want to allow launching watchOS/tvOS simulators, replace the 'iOS' with an 'os' argument
        const matches = runtimeId.match(/(iOS \d{1,2}(.\d{1,2})?)/g); 
        if (matches && matches.length > 0) {
            runtimeId = matches[0].replace(" ", "");
            core.info(`runtimeId: ${runtimeId}`);
        }

        try {
            await execCmd(`xcrun simctl create id com.apple.CoreSimulator.SimDeviceType.${iphoneToSimulate} ${runtimeId.toString()}`);
        }
        catch {
            // Different combinantions of xcode and macOs versions have shown different syntax acceptance about the runtime, therefore 1 last attempt with a different synxtax. 
            const { stdout, stderr } = await childProcess.exec("xcrun simctl list runtimes |  awk '/com.apple.CoreSimulator.SimRuntime.iOS/ { match($0, /com.apple.CoreSimulator.SimRuntime.iOS-[0-9.-]+/); print substr($0, RSTART, RLENGTH); exit }'");
            runtimeId = stdout?.toString() ?? "";
            if (stderr) {
                core.setFailed(stderr.toString());
            }

            await execCmd(`xcrun simctl create ${id} com.apple.CoreSimulator.SimDeviceType.${iphoneToSimulate} ${runtimeId.toString()}`);
        }

        await execCmd(`xcrun simctl boot ${id}`);
        await execCmd(`xcrun simctl install ${id} ${appPath}`);
        await execCmd(`xcrun simctl launch --console-pty ${id} ${bundleId} ${args}`);
    } catch (error) {
        core.setFailed(error.message);
    } finally {
        try {
            await execCmd(`xcrun simctl shutdown ${id}`);
            await execCmd(`xcrun simctl delete${id}`);
        } catch (error) {
            core.setFailed(`An error occurred during cleanup: ${error.toString()}`);
        }
    }
}

async function execCmd(cmd: string, options: exec.ExecOptions = {}): Promise<void> {
    const exitCode = await exec.exec(cmd, [], options);
    if (exitCode != 0) {
        const msgCmd = cmd.split(" ").slice(0, 3).join(" ");
        core.setFailed(`"${msgCmd}" failed with code ${exitCode}`);
    }
}

run();

export default run;
