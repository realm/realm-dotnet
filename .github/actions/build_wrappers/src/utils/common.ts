import * as cp from 'child_process';

export interface output {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

export function execShellCommand(cmd: string, outputStream: output): Promise<void>
{
    return new Promise((resolve, reject) => {
        let buildCmd = cp.exec(cmd);
        buildCmd.stdout?.on("data", (data) => {
            outputStream.info(data.toString());
        });
        buildCmd.stderr?.on("data", (data) => {
            outputStream.info(data.toString());
        });
        buildCmd.on("exit", (code) =>{
            outputStream.info(`Child process exited with code ${code?.toString()}`);
        });
    });
}