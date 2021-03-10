import { assert } from "chai";
import "mocha";
import { suite, test } from '@testdeck/mocha';
import * as input from "../src/utils/input_parsing";
import * as impl from "./class_implementations";
import * as path from "path"

@suite
class InputParsing 
{
    private oss = new impl.outputStream();

    @test
    JsonCmds()
    { 
        const cmdJsonObjet = "[ { \"cmd\": \"echo\", \"cmdParams\": [ \"hello \", \">\", \"test\"] }, { \"cmd\": \"echo\", \"cmdParams\": [ \"world\" ] }]"
        const parsedCmds = input.tryParseCmdInputArray(cmdJsonObjet, this.oss);

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

        const paths = input.parsePaths(pathsToParse);

        assert.notEqual(paths, undefined);
        assert.equal(paths.length, 3);
        assert.equal(paths[0], pwd);
        assert.equal(paths[1], oneUp);
        assert.equal(paths[2], twoUp);
    }
}

