import * as folderHash from "folder-hash";
import * as crypto from "crypto";
import * as cp from "child_process";

export interface outputStream {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

export interface cmdObj
{
    cmd: string;
    cmdParams?: string[];
}

export type hashOptions = folderHash.HashElementOptions;

export type hashFunc = (paths: string[], oss: outputStream, hashPrefix?: string, hashOptions?: hashOptions) => Promise<string | undefined>

// Executes the cmd and returns 0 if success, any other numberic value otherwise.
export async function tryExecShellCommand(cmdObj: cmdObj, oss: outputStream): Promise<number | undefined>
{
    return new Promise<number>((resolve, reject) => {
        try
        {
            let buildCmd = cp.spawn(cmdObj.cmd, cmdObj.cmdParams, {"shell": true, "detached": false}); 
            
            buildCmd.stdout.on("data", (data) => {
                oss.info(data.toString());
            });
            buildCmd.stderr.on("data", (data) => {
                oss.info(data.toString());
            });
            buildCmd.on("exit", (code) =>{
                oss.info(`Child process ${cmdObj.cmd} exited with code ${code?.toString()}`);
                code === 0 ? resolve(code) : reject(code);
            });
        }
        catch (error)
        {
            oss.error(`Executing command ${cmdObj.cmd} failed: ${error.message}`);
            reject(-1);
        }
    });
}

// Given an array of paths, it creates a hash from the joined list of hashes of each subfolder and subfile.
// The final hash is prepend with a constant suffix different on each OS platform.
export async function tryGetHash(paths: string[], oss: outputStream, hashPrefix?: string, hashOptions?: hashOptions): Promise<string | undefined>
{
    try
    {
        const prefix = hashPrefix ?? `cache-${process.platform}-`;
        const folderHash = await hashFolders(paths, hashOptions);
        return prefix.concat(crypto.createHash("sha256").update(folderHash).digest("base64"));
    }
    catch (error)
    {
        oss.error(`Hashing failed: ${error}`);
        return undefined;
    }
}

// Can throw exceptions
async function hashFolders(paths: string[], hashOptions?: hashOptions): Promise<string>
{
    let hashes: string[] = [];
    for (let path of paths)
    {
        const hash = await folderHash.hashElement(path, hashOptions);
        hashes.push(hash.hash);
    }
    return hashes.join("");
}