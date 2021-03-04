import * as cp from 'child_process';

export interface output {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

export async function execShellCommand(outputStream: output, cmd: string, cmdParams?: string[], options?: cp.SpawnOptionsWithoutStdio): Promise<number>
{
    return new Promise<number>((resolve, reject) => {
        let buildCmd = cp.spawn(cmd, cmdParams, options); 
        
        buildCmd.stdout.on("data", (data) => {
            outputStream.info(data.toString());
        });
        buildCmd.stderr.on("data", (data) => {
            outputStream.info(data.toString());
        });
        buildCmd.on("exit", (code) =>{
            outputStream.info(`Child process exited with code ${code?.toString()}`);
            code === 0 ? resolve(code) : reject(code);
        });
    });
}