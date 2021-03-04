import * as cp from 'child_process';

export interface output {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

export async function execShellCommand(outputStream: output, cmdObj: cmdObj): Promise<number>
{
    return new Promise<number>((resolve, reject) => {
        let buildCmd = cp.spawn(cmdObj.cmd, cmdObj.cmdParams, cmdObj.execOptions); 
        
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

export function parseCmdInputArray(cmds: string[]): cmdObj[]
{
    let finalCmds: cmdObj[] = [];
    //console.debug(`list is:\n ${cmds}`);
    for (let cmd of cmds)
    {
        //console.debug(`the object is:\n ${cmd}`);
        finalCmds.push( Object.assign(new cmdObj, JSON.parse(cmd)) );
    }

    return finalCmds;
}

export class cmdObj
{
    public cmd: string = "";
    public cmdParams?: string[] = undefined;
    public execOptions?: cp.SpawnOptionsWithoutStdio = undefined;

    constructor(){};

    // constructor(cmd: string, cmdParams?: string[], execOptions?: cp.SpawnOptionsWithoutStdio)
    // {
    //     this.cmd = cmd;
    //     this.cmdParams = cmdParams;
    //     this.execOptions = execOptions;
    // }

}