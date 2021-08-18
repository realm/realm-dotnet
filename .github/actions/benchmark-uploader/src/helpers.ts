import * as exec from "@actions/exec";
import * as fs from "fs";
import du from "du";

export async function execCmd(cmd: string, args?: string[]): Promise<string> {
    let stdout = "";
    let stderr = "";
    const options: exec.ExecOptions = {
        listeners: {
            stdout: (data: Buffer) => {
                stdout += data.toString();
            },
            stderr: (data: Buffer) => {
                stderr += data.toString();
            },
        },
    };

    const exitCode = await exec.exec(cmd, args, options);
    if (exitCode !== 0) {
        throw new Error(`"${cmd}" failed with code ${exitCode} giving error:\n ${stderr.trim()}`);
    }

    return stdout.trim();
}

export async function getDirectorySizes(
    path: string,
    fileNameMapper?: (file: string) => string,
): Promise<{file: string; size: number}[]> {
    const folders = fs
        .readdirSync(path, {withFileTypes: true})
        .filter(d => d.isDirectory())
        .map(d => d.name);

    const results = new Array<{file: string; size: number}>();

    for (const folder of folders) {
        const size = await du(`${path}/${folder}`);
        results.push({file: (fileNameMapper && fileNameMapper(folder)) || folder, size});
    }

    return results;
}
