import { assert } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import * as path from "path";
import * as fs from "fs-extra";
import * as utils from "../src/utils/common";

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class HashingFunctionalities {
    @test
    async basicHashing(): Promise<void> {
        try {
            await utils.getHash("gibberish");
        } catch (err) {
            assert.equal(err.message, "gibberish path doesn't exist");
        }
        try {
            await utils.getHash("\n");
        } catch (err) {
            assert.equal(err.message, "\n path doesn't exist");
        }
        try {
            await utils.getHash("");
        } catch (err) {
            assert.equal(err.message, "There is no path supplied");
        }

        const tempFolder = await this.makeTempDir("tempDir");
        const hash = await utils.getHash(tempFolder);
        assert.notEqual(hash, "");
        assert.notEqual(hash, undefined);

        await this.cleanUp(tempFolder);
    }

    @test
    async verifyConsistentHashing(): Promise<void> {
        const pwd = __dirname;
        const tempFolder = await this.makeTempDir("tempDir");

        const hashMap = new Map<string, string>();
        let hash = await utils.getHash(pwd);
        if (hash !== undefined) {
            hashMap.set(hash, pwd);
        } else {
            await this.cleanUp(tempFolder);
            assert.fail(`It was impossible to calculate the hash for ${pwd}`);
        }
        hash = await utils.getHash(tempFolder);
        if (hash !== undefined) {
            hashMap.set(hash, tempFolder);
        } else {
            await this.cleanUp(tempFolder);
            assert.fail(`It was impossible to calculate the hash for ${tempFolder}`);
        }

        // recalculate to verify consistency
        hash = await utils.getHash(pwd);
        if (hash !== undefined) {
            assert.equal(hashMap.get(hash), pwd);
        } else {
            await this.cleanUp(tempFolder);
            assert.fail(`It was impossible to re-calculate the hash for ${pwd}`);
        }
        hash = await utils.getHash(tempFolder);
        if (hash !== undefined) {
            assert.equal(hashMap.get(hash), tempFolder);
        } else {
            await this.cleanUp(tempFolder);
            assert.fail(`It was impossible to re-calculate the hash for ${tempFolder}`);
        }
        await this.cleanUp(tempFolder);
    }

    async makeTempDir(dirName: string): Promise<string> {
        const tempFolder = path.join(__dirname, dirName);

        if (!(await fs.pathExists(tempFolder))) {
            fs.mkdir(tempFolder);
            await fs.writeFile(path.join(tempFolder, "testFile"), "junkContent");
        }
        return tempFolder;
    }

    async cleanUp(tempFolderPath: string): Promise<void> {
        await fs.remove(tempFolderPath);
    }
}
