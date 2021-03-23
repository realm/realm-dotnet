import * as cache from "@actions/cache";
import * as exec from "@actions/exec";
import * as utils from "./utils/common";
import * as input from "./utils/input_parsing";

/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param paths New line separated paths that need to be cached after the build (same paths used to create a hash)
 * @param cmds New line separated cmds to build
 * @param oss Output stream where to print the messages
 * @param hashPrefix Optional prefix added in front of the hash that is going to be used as key in the cache dictionary
 * @param hashOptions Optional extra options for the hash function, be it the default of the supplied custom
 * @param hashFunc Optional custom hash function if the default doesn't fullfil the user's needs
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
export async function actionCore(
    paths: string,
    cmds: string,
    oss: utils.outputStream,
    hashPrefix?: string,
    hashOptions?: utils.hashOptions,
    hashFunc?: utils.hashFunc
): Promise<string | undefined> {
    if (cmds.length === 0 || paths.length === 0) {
        throw new Error(`No commands were supplied, nothing to do.`);
    }
    const parsedPaths = input.parsePaths(paths);
    const parsedCmds = input.parseCmds(cmds);

    let hashKey: string | undefined;
    try {
        hashKey =
            hashFunc !== undefined
                ? await hashFunc(parsedPaths, oss, hashPrefix, hashOptions)
                : await utils.getHash(
                      parsedPaths,
                      oss,
                      hashPrefix,
                      hashOptions
                  );
    } catch (err) {
        throw new Error(
            `While calculating the hash something went terribly wrong: ${err.message}`
        );
    }

    let cacheHit: string | undefined = undefined;
    if (hashKey !== undefined) {
        oss.info(`Hash key for ${parsedPaths.join("\n")} is: ${hashKey}`);
        try {
            cacheHit = await cache.restoreCache(parsedPaths, hashKey);
        } catch (err) {
            oss.error(
                `Impossible to retrieve cache: ${err}\n The build will start momentarily...`
            );
        }
    } else {
        throw new Error(
            `No hash could be calculated, so nothing to search in cache. Since what's going to be built now can't be cached, abort!`
        );
    }

    if (cacheHit === undefined) {
        oss.info(`No cache was found, so the command will be executed...`);

        try {
            for (const cmd of parsedCmds) {
                const returnCode = await exec.exec(cmd);
                if (returnCode !== 0) {
                    throw Error(
                        `Executing a command ${cmd} failed with code ${returnCode}. Stopping execution!`
                    );
                }
            }
        } catch (err) {
            throw new Error(
                `Something went terribly wrong while executing a shell command: ${err.message}`
            );
        }

        if (hashKey !== undefined) {
            try {
                const cacheId = await cache.saveCache(parsedPaths, hashKey);
                oss.info(`Cache properly created with id ${cacheId}`);
            } catch (error) {
                throw new Error(
                    `The cache could not be saved: ${error.message}`
                );
            }
        } else {
            throw new Error(
                `HashKey was undefined, so the current build can't be save. This should have never happened!`
            );
        }
    } else {
        oss.info(
            `A build was found in cache for hashKey ${hashKey}\nskipping building...`
        );
    }

    return hashKey;
}
