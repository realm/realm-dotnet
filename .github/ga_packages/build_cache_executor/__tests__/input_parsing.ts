import { assert } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import * as input from "../src/utils/input_parsing";
import * as path from "path";

// I can't get this to work
//*eslint no-unused-vars: ["error", { "varsIgnorePattern": "@suite\\s+\\n?class\\s+\\w+\\s*\\{" }]*/

@suite
class InputParsing {
  @test
  Paths() {
    const pwd = __dirname;
    const oneUp = path.resolve(path.join(pwd, "../"));
    const twoUp = path.resolve(path.join(pwd, "../.."));
    const pathsToParse = `${pwd}\n${oneUp}\n${twoUp}`;

    const paths = input.parsePaths(pathsToParse);

    assert.notEqual(paths, undefined);
    assert.equal(paths.length, 3);
    assert.equal(paths[0], pwd);
    assert.equal(paths[1], oneUp);
    assert.equal(paths[2], twoUp);
  }

  @test
  Cmd() {
    const unparsedCmds = "echo 1\necho 2\necho 3";
    const cmds = input.parseCmds(unparsedCmds);
    assert.equal(cmds.length, 3);
  }
}
