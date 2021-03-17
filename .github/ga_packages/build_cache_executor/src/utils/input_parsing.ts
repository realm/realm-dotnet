import { outputStream } from "./common"

export function parsePaths(str: string): string[]
{
    return str.split("\n");
}

export function parseCmds(str: string): string[]
{
    return str.split("\n");
}