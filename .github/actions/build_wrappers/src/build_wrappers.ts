import * as core from "@actions/core";
import * as cache from "@actions/cache";
import * as utils from "./utils/common";


async function run(): Promise<void>
{
    try
    {
        // TODO to make this general purpose, we need to pass this either from a conf file or as input. Same for the next line
        const paths = [ "./wrappers" ];
        // TODO make this operate only on code folders
        const hashOptions = {
            files: { include: ["*.dll"] },
        };

        const cmdsToParse = core.getInput("cmds", { required: true });
        const cmds: utils.cmdObj[] = utils.tryParseCmdInputArray(cmdsToParse, core);
        if (cmds.length === 0)
        {
            core.setFailed(`No commands were supplied, nothing to do.`);
            return;
        }

        const hash = await utils.tryGetHash(paths, hashOptions, core);

        let cacheKey: string | undefined = undefined;
        if (hash !== undefined)
        {
            core.info(`Hash key for ${paths.join("\n")} is: ${hash}`);
            try
            {
                cacheKey = await cache.restoreCache(paths, hash);
            }
            catch (err)
            {
                core.error(`Impossible to retrieve cache: ${err}`);
            }
        }
        else
        {
            core.error(`No hash could be calculated, so it can't be searched for a previous cached result and what's going to be calculated/built now can't be cached either.`);
        }
        
        if (cacheKey === undefined)
        {
            core.info(`No cache was found, so the command will be executed...`);

            core.startGroup(`Build process output`);
            for (let cmd of cmds)
            {
                if (await utils.tryExecShellCommand(cmd, core) != 0)
                {
                    core.setFailed(`Executing a command failed. Stopping execution!`);
                    return;
                }
            }
            core.endGroup();

            if (hash !== undefined)
            {
                try
                {
                    const cacheId = await cache.saveCache(paths, hash);
                    core.info(`Cache properly created with id ${cacheId}`);
                }
                catch (error)
                {
                    core.error(`The cache could not be saved because ${error.message}`);
                }
            }
        }
        else
        {
            core.info(`A build was found in cache with cacheKey: ${cacheKey}\nskipping building...`);
        }
    }
    catch (error)
    {
        core.setFailed(`Something went wrong while retrieving the cache and or building: ${error.message}`);
    }
}

run();

export default run;