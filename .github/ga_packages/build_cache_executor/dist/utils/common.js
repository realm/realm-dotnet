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
exports.tryGetHash = void 0;
const folderHash = __importStar(require("folder-hash"));
const crypto = __importStar(require("crypto"));
/** @internal */
// Given an array of paths, it creates a hash from the joined list of hashes of each subfolder and subfile.
// The final hash is prepend with a constant hashPrefix if supplied, otherwise with the "cache-(current OS platform)-".
function tryGetHash(paths, oss, hashPrefix, hashOptions) {
    return __awaiter(this, void 0, void 0, function* () {
        try {
            const prefix = hashPrefix !== null && hashPrefix !== void 0 ? hashPrefix : `cache-${process.platform}-`;
            const folderHash = yield hashFolders(paths, hashOptions);
            return prefix.concat(crypto.createHash("sha256").update(folderHash).digest("base64"));
        }
        catch (error) {
            oss === null || oss === void 0 ? void 0 : oss.error(`Hashing failed: ${error}`);
            return undefined;
        }
    });
}
exports.tryGetHash = tryGetHash;
/** @internal */
// Calculates an array of hashes from all the paths (following recursively all children) and returns 1 string that results from the joined elements of the arrar.
// Can throw exceptions.
function hashFolders(paths, hashOptions) {
    return __awaiter(this, void 0, void 0, function* () {
        let hashes = [];
        for (const path of paths) {
            const pathHash = recursiveHashFolders(yield folderHash.hashElement(path, hashOptions));
            hashes = hashes.concat(pathHash);
        }
        return hashes.join("");
    });
}
/** @internal */
// Recursively parses all hash-nodes from the root to the children returning a flattened list of hashes of all nodes.
function recursiveHashFolders(hashNode) {
    let hashes = [];
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
//# sourceMappingURL=common.js.map