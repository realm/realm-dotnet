// import * as core from "@actions/core";
import * as cache from "@actions/cache";
import * as utils from "./utils/common";

export interface result
{
    result?: any;
    error?: Error;
}

class resultImpl implements result
{
    result?: any;
    error?: Error;

    constructor(result?: any, error?: Error)
    {
        this.result = result;
        this.error = error;
    }
}

export async function actionCore(
    paths: string[],
    cmds: utils.cmdObj[],
    hashFunc: (paths: string[], oss: utils.outputStream) => Promise<string | undefined>,
    oss: utils.outputStream): Promise<result>
{
    if (cmds.length === 0)
    {
        return new resultImpl(undefined, new Error(`No commands were supplied, nothing to do.`));
    }

    const hash = await hashFunc(paths, oss);
    
    let cacheKey: string | undefined = undefined;
    if (hash !== undefined)
    {
        oss.info(`Hash key for ${paths.join("\n")} is: ${hash}`);
        try
        {
            cacheKey = await cache.restoreCache(paths, hash);
        }
        catch (err)
        {
            oss.error(`Impossible to retrieve cache: ${err}`);
        }
    }
    else
    {
        oss.error(`No hash could be calculated, so it can't be searched for a previous cached result and what's going to be built now can't be cached either.`);
    }
    
    if (cacheKey === undefined)
    {
        oss.info(`No cache was found, so the command will be executed...`);

        for (let cmd of cmds)
        {
            if (await utils.tryExecShellCommand(cmd, oss) !== 0)
            {
                return new resultImpl(undefined, new Error(`Executing a command failed. Stopping execution!`));
            }
        }

        if (hash !== undefined)
        {
            try
            {
                const cacheId = await cache.saveCache(paths, hash);
                oss.info(`Cache properly created with id ${cacheId}`);
            }
            catch (error)
            {
                oss.error(`The cache could not be saved because ${error.message}`);
            }
        }
    }
    else
    {
        oss.info(`A build was found in cache with cacheKey: ${cacheKey}\nskipping building...`);
    }

    return new resultImpl(true, undefined);
}