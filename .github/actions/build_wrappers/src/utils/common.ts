import * as cp from 'child_process';

export interface output {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

export async function execShellCommand(outputStream: output, cmd: string, cmdParams?: string[], envVars?: string[]): Promise<void>
{
    let buildCmd: cp.ChildProcess | undefined; 

    try
    {
        if (envVars !== undefined)
        {
            buildCmd = cp.spawn(cmd, cmdParams, {env: { REALM_CMAKE_CONFIGURATION: "Release" }});
        }
    }
    catch (err)
    {
        outputStream.error(`failed to execute command: ${err.message}`);
    }
    
    buildCmd?.stdout?.on("data", (data) => {
        outputStream.info(data.toString());
    });
    buildCmd?.stderr?.on("data", (data) => {
        outputStream.info(data.toString());
    });
    buildCmd?.on("exit", (code) =>{
        outputStream.info(`Child process exited with code ${code?.toString()}`);
    });
}