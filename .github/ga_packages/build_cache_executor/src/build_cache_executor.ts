import * as cache from "@actions/cache";
import * as exec from "@actions/exec";
import * as folderHash from "folder-hash";
import * as fs from "fs-extra";

/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param inputPath Path that needs to be cached after the build (same paths used to create a hash)
 * @param cmd Cmd to execute to obtain a build
 * @param logger Output stream where to print the messages
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
export async function actionCore(inputPath: string, outputPath: string, cmd: string, logger: Logger): Promise<string | undefined> {
    await validateInput(inputPath, outputPath, cmd);

    const pathHash = (await folderHash.hashElement(inputPath)).hash;
    const hashKey = `${cmd}${pathHash}`;

    logger.info(`Hash key for ${inputPath} is: ${hashKey}`);

    if (!await existsInCache(outputPath, hashKey)) {
        logger.info("No cache was found, so the command will be executed...");

        await executeCommand(cmd);

        await cache.saveCache([outputPath], hashKey);
    } else {
        logger.info(`A build was found in cache for hashKey ${hashKey}\nskipping building...`);
    }

    return hashKey;
}

async function validateInput(inputPath: string, outputPath: string, cmd: string): Promise<void> {
    if (cmd.length === 0) {
        throw new Error(`No command was supplied, nothing to do.`);
    }

    if (inputPath.length === 0) {
        throw new Error("No inputPath was supplied");
    }

    if (!(await fs.pathExists(inputPath))) {
        throw new Error(`inputPath '${inputPath}' doesn't exist`);
    }

    if (outputPath.length === 0) {
        throw new Error("No outputPath was supplied");
    }
}

async function existsInCache(outputPath: string, key: string): Promise<boolean> {
    try {
        const cacheHit = await cache.restoreCache([outputPath], key);
        return cacheHit === key;
    } catch { }

    return false;
}

async function executeCommand(cmd: string): Promise<void> {
    let returnCode = 0;
    try {
        returnCode = await exec.exec(cmd);
    } catch (err) {
        throw new Error(`Something went wrong while executing the command '${cmd}': ${err.message}`);
    }

    if (returnCode !== 0) {
        throw Error(`Executing a command '${cmd}' failed with non-zero code: ${returnCode}`);
    }
}

export interface Logger {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}

//// DEBUG!: uncomment to debug
// class logger implements iLogger {
//     debug(message: string): void {
//         console.debug(message);
//     }
//     info(message: string): void {
//         console.log(message);
//     }
//     warning(message: string): void {
//         console.warn(message);
//     }
//     error(message: string): void {
//         console.error(message);
//     }
// }
// actionCore(".", "echo 1", new logger());
