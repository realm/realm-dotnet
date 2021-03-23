import * as folderHash from "folder-hash";
export interface outputStream {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}
export declare type hashOptions = folderHash.HashElementOptions;
export declare type hashFunc = (
    paths: string[],
    oss?: outputStream,
    hashPrefix?: string,
    hashOptions?: hashOptions
) => Promise<string | undefined>;
