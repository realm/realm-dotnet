import * as cache from "@actions/cache";
import * as exec from "@actions/exec";
import * as utils from "./utils/common";
import * as input from "./utils/input_parsing";

/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, an hash is calculated over paths and the result is used as key in the dictionary of the cache.
 * The function can throw exceptions.
 * @param paths New line separated paths that need to be cached after the build (same paths used to create a hash)
 * @param cmds New line separated  cmds to build
 * @param oss Where to print the output messages
 * @param hashPrefix Prefix added in front of the hash that is going to be used as key in the cache dictionary
 * @param hashOptions Extra options for the default hash function
 * @param hashFunc Custom hash function if the default doesn't fullfil the user's needs
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned, otherwise.
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

  let hash: string | undefined;
  try {
    hash =
      hashFunc !== undefined
        ? await hashFunc(parsedPaths, oss, hashPrefix)
        : await utils.tryGetHash(parsedPaths, oss, hashPrefix);
  } catch (err) {
    throw new Error(
      `While calculating the hash something went terribly wrong: ${err.message}`
    );
  }

  let cacheKey: string | undefined = undefined;
  if (hash !== undefined) {
    oss.info(`Hash key for ${parsedPaths.join("\n")} is: ${hash}`);
    try {
      cacheKey = await cache.restoreCache(parsedPaths, hash);
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

  if (cacheKey === undefined) {
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

    if (hash !== undefined) {
      try {
        const cacheId = await cache.saveCache(parsedPaths, hash);
        oss.info(`Cache properly created with id ${cacheId}`);
      } catch (error) {
        throw new Error(
          `The cache could not be saved because ${error.message}`
        );
      }
    }
  } else {
    oss.info(
      `A build was found in cache with cacheKey: ${cacheKey}\nskipping building...`
    );
  }

  return cacheKey;
}
