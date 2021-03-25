import * as folderHash from "folder-hash";
import * as fs from "fs-extra";

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
