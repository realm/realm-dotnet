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
const cache = __importStar(require("@actions/cache"));
const core = __importStar(require("@actions/core"));
const folderHash = __importStar(require("folder-hash"));
const crypto = __importStar(require("crypto"));
const common_1 = require("./utils/common");
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
