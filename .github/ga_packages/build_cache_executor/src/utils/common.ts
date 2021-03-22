import * as folderHash from "folder-hash";
import * as crypto from "crypto";

export interface outputStream {
  debug(message: string): void;
  info(message: string): void;
  warning(message: string): void;
  error(message: string): void;
}

export type hashOptions = folderHash.HashElementOptions;

export type hashFunc = (
  paths: string[],
  oss?: outputStream,
  hashPrefix?: string,
  hashOptions?: hashOptions
) => Promise<string | undefined>;

/** @internal */
// Given an array of paths, it creates a hash from the joined list of hashes of each subfolder and subfile.
// The final hash is prepend with a constant hashPrefix if supplied, otherwise with current the OS platform.
export async function tryGetHash(
  paths: string[],
  oss?: outputStream,
  hashPrefix?: string,
  hashOptions?: hashOptions
): Promise<string | undefined> {
  try {
    const prefix = hashPrefix ?? `cache-${process.platform}-`;
    const folderHash = await hashFolders(paths, hashOptions);
    return prefix.concat(
      crypto.createHash("sha256").update(folderHash).digest("base64")
    );
  } catch (error) {
    oss?.error(`Hashing failed: ${error}`);
    return undefined;
  }
}

/** @internal */
// Calculates an array of hashes from all the paths (followingrecursively from
// Can throw exceptions.
async function hashFolders(
  paths: string[],
  hashOptions?: hashOptions
): Promise<string> {
  let hashes: string[] = [];
  for (const path of paths) {
    const pathHash = recursiveHashFolders(
      await folderHash.hashElement(path, hashOptions)
    );
    hashes = hashes.concat(pathHash);
  }
  return hashes.join("");
}

/** @internal */
// Recursively parse all nodes from the root to the children returning a flattened list of hashes of all nodes
function recursiveHashFolders(hashNode: folderHash.HashElementNode): string[] {
  let hashes: string[] = [];

  if (hashNode === undefined) {
    return hashes;
  }
  hashes.push(hashNode.hash);

  if (hashNode.children !== undefined) {
    for (const child of hashNode.children) {
      hashes = hashes.concat(recursiveHashFolders(child));
    }
  }
  return hashes;
}
