import * as core from "@actions/core";
import * as cache from "@actions/cache";
import * as folderHash from "folder-hash";
import * as crypto from "crypto";
import { execShellCommand } from "./utils/common";


async function run(): Promise<void>
{
    const paths = [ "./wrappers/build/" ];
    const hashOptions = {
        files: { include: ["*.dll"] },
    };

    let finalHash: string | undefined = undefined;
    try
    {
       finalHash = hash(await hashFolders(paths, hashOptions)); 
    }
    catch (error)
    {
        core.error(`Hashing failed: ${error}`);
    }

    //TODO see if it could be of use
    // const restoreKeys = [
    //     openingHashSignature
    // ]

    let cacheKey: string | undefined = undefined;
    if (finalHash !== undefined)
    {
        core.info(`Hash key for build is: ${finalHash}`);
        try
        {
            cacheKey = await cache.restoreCache(paths, finalHash);
        }
        catch (err)
        {
            core.error(`Impossible to retrieve cache: ${err}`);
        }
    }
    
    if (cacheKey === undefined)
    {
        core.info(`No cache was found, the wrappers will be compiled. Wait while the compilation is carried out...`);

        let returnBulidValue: number;
        try
        {
            core.startGroup(`Build process output`);
            returnBulidValue = await execShellCommand(core, "./wrappers/build-macos.sh", [], ["REALM_CMAKE_CONFIGURATION=Release"]);
            core.endGroup();
        }
        catch (err)
        {
            core.setFailed(`Error while building: ${err.message}`);
            return;
        }

        // failure
        if (returnBulidValue != 0)
        {
            core.setFailed(`The build failed for some reasons`);
            return;
        }
        core.info(`before key`);
        const key = hash(await hashFolders(paths, hashOptions));
        core.info(`after key = ${key}`);
        try
        {
            core.info(`before saveCache`);
            const cacheId = await cache.saveCache(paths, key);
            core.info(`Cache properly created with id ${cacheId}`);
        }
        catch (error)
        {
            core.error(`The cache could not be saved because ${error.message}`);
        }
    }
    else
    {
        core.info(`A build of the wrappers was found in cache with cacheKey: ${cacheKey}\nskipping building...`);
        // IS IT ALREADY RESTORED IN PLACE??? INVESTIGATE
    }
}

// Result: signature-"hashOfStr"
function hash(str: string)
{
    const openingHashSignature = `cache-hash-${process.platform}-`;
    return openingHashSignature.concat(crypto.createHash("sha256").update(str).digest("base64"));
}

async function hashFolders(paths: string[], hashOptions: folderHash.HashElementOptions): Promise<string>
{
    let hashes: string[] = [];
    for (let path of paths)
    {
        try
        {
            const hash = await folderHash.hashElement(path, hashOptions);
            hashes.push(hash.hash);
        }
        catch(err)
        {
            `Error creating hash for ${path}:\n ${err}`;
        }
    }
    return hashes.join("");
}

run();

export default run;