import { assert } from "chai";
import "mocha";
import { suite, test } from '@testdeck/mocha';
import * as utils from "../src/utils/common";
import * as impl from "./class_implementations";
import * as path from "path";
import * as fs from "fs";

@suite
class CommonUtils
{
    private oss = new impl.outputStream();

    @test
    async VerifyShellCommandExecutes()
    {
        const fileName = "tempConsole";
        const fileContent = "hello world";
        const cmd: utils.cmdObj = { cmd: "echo", cmdParams: [ "-n", fileContent, ">",  fileName] };
        assert.equal(await utils.tryExecShellCommand(cmd, this.oss), 0);
        
        fs.readFile(fileName, "utf8" , (err, data) => {
            if (err !== null)
            {
                assert.fail(err.message);
            }
            assert.equal(data, fileContent);
        });

        fs.unlink(fileName, () => {});
    }
    
    @test
    async VerifyConsistentHashing()
    {
        const pwd = __dirname
        const oneUp = path.resolve(path.join(pwd, "../"));

        let hashMap = new Map<string, string>();
        let hash = await utils.tryGetHash([pwd], this.oss);
        if (hash !== undefined)
        {
            hashMap.set(hash, pwd);
        }
        else
        {
            assert.fail(`It was impossible to calculate the hash for ${pwd}`);
        }
        hash = await utils.tryGetHash([oneUp], this.oss);
        if (hash !== undefined)
        {
            hashMap.set(hash, oneUp);
        }
        else
        {
            assert.fail(`It was impossible to calculate the hash for ${oneUp}`);
        }

        // recalculate on purpose to verify consistency
        hash = await utils.tryGetHash([pwd], this.oss);
        if (hash !== undefined)
        {
            assert.equal(hashMap.get(hash), pwd);
        }
        else
        {
            assert.fail(`It was impossible to re-calculate the hash for ${pwd}`);
        }
        hash = await utils.tryGetHash([oneUp], this.oss);
        if (hash !== undefined)
        {
            assert.equal(hashMap.get(hash), oneUp);
        }
        else
        {
            assert.fail(`It was impossible to re-calculate the hash for ${oneUp}`);
        }
    }
}