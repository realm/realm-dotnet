import { assert } from "chai";
import "mocha";
import { suite, test } from '@testdeck/mocha';
import * as input from "../src/utils/input_parsing";
import * as utils from "../src/utils/common";
import * as path from "path"

class outputStream implements utils.outputStream
{
    debug(message: string): void {
        console.debug(message);
    }
    info(message: string): void {
        console.info(message);
    }
    warning(message: string): void {
        console.warn(message);
    }
    error(message: string): void {
        console.error(message);
    }
}

@suite
class InputParsing 
{
    @test
    JsonCmds()
    { 
        const cmdJsonObjet = "[ { \"cmd\": \"echo\", \"cmdParams\": [ \"hello \", \">\", \"test\"] }, { \"cmd\": \"echo\", \"cmdParams\": [ \"world\" ] }]"
        const oss = new outputStream();
        const parsedCmds = input.tryParseCmdInputArray(cmdJsonObjet, oss);

        assert.notEqual(parsedCmds, undefined);
        assert.equal(parsedCmds.length, 2);
        assert.equal(parsedCmds[0].cmd, "echo");
        assert.notEqual(parsedCmds[0].cmdParams, undefined);
        assert.equal(parsedCmds[0].cmdParams?.length, 3);
        if (parsedCmds[0].cmdParams !== undefined)
        {
            assert.equal(parsedCmds[0].cmdParams[0], "hello ");
            assert.equal(parsedCmds[0].cmdParams[1], ">");
            assert.equal(parsedCmds[0].cmdParams[2], "test");
        }
        assert.equal(parsedCmds[1].cmd, "echo");
        assert.notEqual(parsedCmds[1].cmdParams, undefined);
        assert.equal(parsedCmds[1].cmdParams?.length, 1);
        if (parsedCmds[1].cmdParams !== undefined)
        {
            assert.equal(parsedCmds[1].cmdParams[0], "world");
        }
    }

    @test
    Paths()
    {
        const pwd = __dirname
        const oneUp = path.resolve(path.join(pwd, "../"));
        const twoUp = path.resolve(path.join(pwd, "../.."));
        const pathsToParse = `${pwd} ${oneUp} ${twoUp}`;

        const oss = new outputStream();
        const paths = input.parsePaths(pathsToParse);

        assert.notEqual(paths, undefined);
        assert.equal(paths.length, 3);
        assert.equal(paths[0], pwd);
        assert.equal(paths[1], oneUp);
        assert.equal(paths[2], twoUp);
    }
}

