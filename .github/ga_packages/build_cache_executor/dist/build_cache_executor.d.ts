/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param path Path that needs to be cached after the build (same paths used to create a hash)
 * @param cmd Cmd to execute to obtain a build
 * @param logger Output stream where to print the messages
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
export declare function actionCore(path: string, cmd: string, logger: iLogger): Promise<string | undefined>;
export interface iLogger {
    debug(message: string): void;
    info(message: string): void;
    warning(message: string): void;
    error(message: string): void;
}
