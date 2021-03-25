"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    Object.defineProperty(o, k2, { enumerable: true, get: function() { return m[k]; } });
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || function (mod) {
    if (mod && mod.__esModule) return mod;
    var result = {};
    if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
    __setModuleDefault(result, mod);
    return result;
};
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
Object.defineProperty(exports, "__esModule", { value: true });
exports.actionCore = void 0;
const cache = __importStar(require("@actions/cache"));
const exec = __importStar(require("@actions/exec"));
const utils = __importStar(require("./utils/common"));
/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param path Path that needs to be cached after the build (same paths used to create a hash)
 * @param cmd Cmd to execute to obtain a build
 * @param logger Output stream where to print the messages
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
function actionCore(path, cmd, logger) {
    return __awaiter(this, void 0, void 0, function* () {
        if (cmd.length === 0) {
            throw new Error(`No command was supplied, nothing to do.`);
        }
        if (path.length === 0) {
            throw new Error(`No path was supplied, nothing to cache.`);
        }
        let hashKey;
        try {
            hashKey = cmd.concat(yield utils.getHash(path));
        }
        catch (err) {
            throw new Error(`While calculating the hash something went terribly wrong: ${err.message}`);
        }
        let cacheHit = undefined;
        if (hashKey !== undefined) {
            logger.info(`Hash key for ${path} is: ${hashKey}`);
            try {
                cacheHit = yield cache.restoreCache([path], hashKey);
            }
            catch (err) {
                logger.error(`Impossible to retrieve cache: ${err}\n The build will start momentarily...`);
            }
        }
        else {
            throw new Error(`No hash could be calculated, so nothing to search in cache. Since what's going to be built now can't be cached, abort!`);
        }
        if (cacheHit === undefined) {
            logger.info(`No cache was found, so the command will be executed...`);
            try {
                const returnCode = yield exec.exec(cmd);
                if (returnCode !== 0) {
                    throw Error(`Executing a command ${cmd} failed with code ${returnCode}. Stopping execution!`);
                }
            }
            catch (err) {
                throw new Error(`Something went terribly wrong while executing a shell command: ${err.message}`);
            }
            if (hashKey !== undefined) {
                try {
                    const cacheId = yield cache.saveCache([path], hashKey);
                    logger.info(`Cache properly created with id ${cacheId}`);
                }
                catch (error) {
                    throw new Error(`The cache could not be saved: ${error.message}`);
                }
            }
            else {
                throw new Error(`HashKey was undefined, so the current build can't be save. This should have never happened!`);
            }
        }
        else {
            logger.info(`A build was found in cache for hashKey ${hashKey}\nskipping building...`);
        }
        return hashKey;
    });
}
exports.actionCore = actionCore;
//# sourceMappingURL=build_cache_executor.js.map