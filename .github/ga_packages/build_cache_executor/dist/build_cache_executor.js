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
const folderHash = __importStar(require("folder-hash"));
const fs = __importStar(require("fs-extra"));
/**
 * Builds and caches the resulting artifacts. In order to store the artifacts in a cache, a hash (cacheKey) is calculated over paths and the result is used as key in the cache dictionary.
 * The function can throw exceptions.
 * @param inputPath Path that needs to be cached after the build (same paths used to create a hash)
 * @param cmd Cmd to execute to obtain a build
 * @param logger Output stream where to print the messages
 * @returns CacheKey necessary to recover the cached build later on. Undefined is returned if something went wrong.
 */
function actionCore(inputPath, outputPath, cmd, logger) {
    return __awaiter(this, void 0, void 0, function* () {
        yield validateInput(inputPath, outputPath, cmd);
        const pathHash = (yield folderHash.hashElement(inputPath)).hash;
        const hashKey = `${cmd}${pathHash}`;
        logger.info(`Hash key for ${inputPath} is: ${hashKey}`);
        if (!(yield existsInCache(outputPath, hashKey))) {
            logger.info("No cache was found, so the command will be executed...");
            yield executeCommand(cmd);
            yield cache.saveCache([outputPath], hashKey);
        }
        else {
            logger.info(`A build was found in cache for hashKey ${hashKey}\nskipping building...`);
        }
        return hashKey;
    });
}
exports.actionCore = actionCore;
function validateInput(inputPath, outputPath, cmd) {
    return __awaiter(this, void 0, void 0, function* () {
        if (cmd.length === 0) {
            throw new Error(`No command was supplied, nothing to do.`);
        }
        if (inputPath.length === 0) {
            throw new Error("No inputPath was supplied");
        }
        if (!(yield fs.pathExists(inputPath))) {
            throw new Error(`inputPath '${inputPath}' doesn't exist`);
        }
        if (outputPath.length === 0) {
            throw new Error("No outputPath was supplied");
        }
    });
}
function existsInCache(outputPath, key) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const cacheHit = yield cache.restoreCache([outputPath], key);
            return cacheHit === key;
        }
        catch (_a) { }
        return false;
    });
}
function executeCommand(cmd) {
    return __awaiter(this, void 0, void 0, function* () {
        let returnCode = 0;
        try {
            returnCode = yield exec.exec(cmd);
        }
        catch (err) {
            throw new Error(`Something went wrong while executing the command '${cmd}': ${err.message}`);
        }
        if (returnCode !== 0) {
            throw Error(`Executing a command '${cmd}' failed with non-zero code: ${returnCode}`);
        }
    });
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
//# sourceMappingURL=build_cache_executor.js.map