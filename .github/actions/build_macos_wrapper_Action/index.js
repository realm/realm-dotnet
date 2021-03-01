var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
// export interface output {
//     debug(text: string): void;
//     info(text: string): void;
//     warning(text: string): void;
//     error(text: string): void;
// }
define("utils/common", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    exports.execShellCommand = void 0;
    function execShellCommand(cmd) {
        const exec = require('child_process').exec;
        return new Promise((resolve, reject) => {
            exec(cmd, (error, stdout, stderr) => {
                if (error) {
                    throw new Error(error);
                }
                resolve([stdout, stderr]);
            });
        });
    }
    exports.execShellCommand = execShellCommand;
});
define("build_macos_wrapper", ["require", "exports", "@actions/cache", "@actions/core", "folder-hash", "crypto", "utils/common"], function (require, exports, cache, core, folderHash, crypto, common_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    function run() {
        return __awaiter(this, void 0, void 0, function* () {
            const paths = ["./wrappers/build/"];
            const hashOptions = {
                files: { include: ["*.dll"] },
            };
            let finalHash;
            try {
                finalHash = hash(yield hashFolders(paths, hashOptions));
            }
            catch (error) {
                core.error("hashing failed:" + error);
            }
            //TODO see if it could be of use
            // const restoreKeys = [
            //     openingHashSignature
            // ]
            let cacheKey;
            if (finalHash !== null) {
                cacheKey = yield cache.restoreCache(paths, finalHash);
            }
            if (cacheKey === undefined) {
                let cmdOutput;
                try {
                    cmdOutput = yield common_1.execShellCommand("REALM_CMAKE_CONFIGURATION=Release ./wrappers/build-macos.sh");
                }
                catch (err) {
                    core.setFailed("Error while building: " + err.message);
                    return;
                }
                if (cmdOutput[0] != null) {
                    core.setFailed(cmdOutput[0]);
                    return;
                }
                else {
                    if (cmdOutput[1] !== null) {
                        core.info(cmdOutput[1]);
                    }
                    const key = hash(yield hashFolders(paths, hashOptions));
                    const cacheId = yield cache.saveCache(paths, key);
                }
            }
            else {
                // IS IT ALREADY RESTORED IN PLACE??? INVESTIGATE
            }
        });
    }
    // Result: signature-"hashOfStr"
    function hash(str) {
        const openingHashSignature = ["cache-hash-", process.platform, "-"].join("");
        return openingHashSignature.concat(crypto.createHash("sha256").update(str).digest("base64"));
    }
    function hashFolders(paths, hashOptions) {
        return __awaiter(this, void 0, void 0, function* () {
            let hashes;
            for (let path of paths) {
                yield folderHash.hashElement(path, hashOptions)
                    .then(hash => { hashes.push(hash.hash); })
                    .catch(err => { "Error creating hash for " + path + ":\n" + err; });
            }
            return hashes === null || hashes === void 0 ? void 0 : hashes.join("");
        });
    }
    run();
    exports.default = run;
});
//# sourceMappingURL=index.js.map