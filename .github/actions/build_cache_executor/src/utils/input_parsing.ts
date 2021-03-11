import { cmdObj, outputStream } from "./common"

export function parsePaths(str: string): string[]
{
    return str.split(" ");
}

export function tryParseCmdInputArray(cmds: string, oss: outputStream): cmdObj[]
{
    let finalCmds: cmdObj[] = [];
    try
    {
        finalCmds = JSON.parse(cmds);
    }
    catch(error)
    {
        oss.error(`Error while parsing cmds: ${error.message}`);
    }

    return finalCmds;
}