import { assert } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import * as input from "../src/utils/input_parsing";
import * as path from "path";

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class InputParsing {
  @test
  pathsElements(): void {
    let unparsedPath = "";
    let paths = input.parseCmds(unparsedPath);
    assert.lengthOf(paths, 0);

    unparsedPath = "\n";
    paths = input.parseCmds(unparsedPath);
    assert.lengthOf(paths, 0);

    unparsedPath = "/";
    paths = input.parseCmds(unparsedPath);
    assert.lengthOf(paths, 1);

    unparsedPath = "/\n";
    paths = input.parseCmds(unparsedPath);
    assert.lengthOf(paths, 1);

    unparsedPath = "/a/b/c/\nd/e/f\ng/h/e/";
    paths = input.parseCmds(unparsedPath);
    assert.lengthOf(paths, 3);
  }

  @test
  pathCorrectness(): void {
    const pwd = __dirname;
    const oneUp = path.resolve(path.join(pwd, "../"));
    const twoUp = path.resolve(path.join(pwd, "../.."));
    const pathsToParse = `${pwd}\n${oneUp}\n${twoUp}`;

    const paths = input.parsePaths(pathsToParse);

    assert.notEqual(paths, undefined);
    assert.lengthOf(paths, 3);
    assert.equal(paths[0], pwd);
    assert.equal(paths[1], oneUp);
    assert.equal(paths[2], twoUp);
  }

  @test
  cmdElements(): void {
    let unparsedCmds = "";
    let cmds = input.parseCmds(unparsedCmds);
    assert.lengthOf(cmds, 0);

    unparsedCmds = "\n";
    cmds = input.parseCmds(unparsedCmds);
    assert.lengthOf(cmds, 0);

    unparsedCmds = "echo 1";
    cmds = input.parseCmds(unparsedCmds);
    assert.lengthOf(cmds, 1);

    unparsedCmds = "echo 1\n";
    cmds = input.parseCmds(unparsedCmds);
    assert.lengthOf(cmds, 1);

    unparsedCmds = "echo 1\necho 2\necho 3";
    cmds = input.parseCmds(unparsedCmds);
    assert.lengthOf(cmds, 3);
  }
}
