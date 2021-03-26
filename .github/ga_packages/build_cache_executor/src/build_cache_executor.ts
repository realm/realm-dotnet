import * as cache from "@actions/cache";
import * as exec from "@actions/exec";
import * as folderHash from "folder-hash";
import * as fs from "fs-extra";

/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param path Path that needs to be cached after the build (same paths used to create a hash)
 * @param cmd Cmd to execute to obtain a build
 * @param logger Output stream where to print the messages
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
export async function actionCore(path: string, cmd: string, logger: logger): Promise<string | undefined> {
    if (cmd.length === 0) {
        throw new Error(`No command was supplied, nothing to do.`);
    }
    if (path.length === 0) {
        throw new Error(`No path was supplied, nothing to cache.`);
    }

    let hashKey: string | undefined;
    try {
        hashKey = cmd.concat(await getHash(path));
    } catch (err) {
        throw new Error(`While calculating the hash something went terribly wrong: ${err.message}`);
    }

    let cacheHit: string | undefined = undefined;
    if (hashKey !== undefined) {
        logger.info(`Hash key for ${path} is: ${hashKey}`);
        try {
            cacheHit = await cache.restoreCache([path], hashKey);
        } catch (err) {
            logger.error(`Impossible to retrieve cache: ${err}\n The build will start momentarily...`);
        }
    } else {
        throw new Error(
            `No hash could be calculated, so nothing to search in cache. Since what's going to be built now can't be cached, abort!`
        );
    }

    if (cacheHit === undefined) {
        logger.info(`No cache was found, so the command will be executed...`);

        try {
            const returnCode = await exec.exec(cmd);
            if (returnCode !== 0) {
                throw Error(`Executing a command ${cmd} failed with code ${returnCode}. Stopping execution!`);
            }
        } catch (err) {
            throw new Error(`Something went terribly wrong while executing a shell command: ${err.message}`);
        }

        if (hashKey !== undefined) {
            try {
                const cacheId = await cache.saveCache([path], hashKey);
                logger.info(`Cache properly created with id ${cacheId}`);
            } catch (error) {
                throw new Error(`The cache could not be saved: ${error.message}`);
            }
        } else {
            throw new Error(
                `HashKey was undefined, so the current build can't be save. This should have never happened!`
            );
        }
    } else {
        logger.info(`A build was found in cache for hashKey ${hashKey}\nskipping building...`);
    }

    return hashKey;
}

export interface logger {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

/** @internal */
// Given a path, it calculates a hash resulting from the joined hashes of all subfolders and subfiles.
// Can throw exceptions.
export async function getHash(path: string): Promise<string> {
    if (path.length === 0) {
        throw new Error("There is no path supplied");
    }
    if (!(await fs.pathExists(path))) {
        throw new Error(`${path} path doesn't exist`);
    }
    return (await folderHash.hashElement(path)).hash;
}
