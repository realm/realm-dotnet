import * as exec from "@actions/exec";

export async function execCmd(cmd: string): Promise<string> {
    let stdout = "";
    let stderr = "";
    const options: exec.ExecOptions = {};
    options.listeners = {
        stdout: (data: Buffer) => {
            stdout += data.toString();
        },
        stderr: (data: Buffer) => {
            stderr += data.toString();
        },
    };

    const exitCode = await exec.exec(cmd, [], options);
    if (exitCode !== 0) {
        throw new Error(`"${cmd}" failed with code ${exitCode} giving error:\n ${stderr}`);
    }

    return stdout;
}
