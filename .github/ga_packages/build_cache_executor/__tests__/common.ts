import { assert } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import * as path from "path";
import * as fs from "fs-extra";
import * as utils from "../src/utils/common";

@suite
class CommonUtils {
  @test
  async VerifyConsistentHashing(): Promise<void> {
    const pwd = __dirname;
    const tempFolder = path.join(pwd, "tempFolder");

    if (!(await fs.pathExists(tempFolder))) {
      fs.mkdir(tempFolder);
      await fs.writeFile(path.join(tempFolder, "testFile"), "junkContent");
    }

    const hashMap = new Map<string, string>();
    let hash = await utils.tryGetHash([pwd]);
    if (hash !== undefined) {
      hashMap.set(hash, pwd);
    } else {
      assert.fail(`It was impossible to calculate the hash for ${pwd}`);
    }
    hash = await utils.tryGetHash([tempFolder]);
    if (hash !== undefined) {
      hashMap.set(hash, tempFolder);
    } else {
      await this.CleanUp(tempFolder);
      assert.fail(`It was impossible to calculate the hash for ${tempFolder}`);
    }

    // recalculate to verify consistency
    hash = await utils.tryGetHash([pwd]);
    if (hash !== undefined) {
      assert.equal(hashMap.get(hash), pwd);
    } else {
      await this.CleanUp(tempFolder);
      assert.fail(`It was impossible to re-calculate the hash for ${pwd}`);
    }
    hash = await utils.tryGetHash([tempFolder]);
    if (hash !== undefined) {
      assert.equal(hashMap.get(hash), tempFolder);
    } else {
      await this.CleanUp(tempFolder);
      assert.fail(
        `It was impossible to re-calculate the hash for ${tempFolder}`
      );
    }
    await this.CleanUp(tempFolder);
  }

  async CleanUp(tempFolderPath: string): Promise<void> {
    await fs.remove(tempFolderPath);
  }

  @test
  async VerifyHashPrefix(): Promise<void> {
    const pwd = __dirname;
    let hash = await utils.tryGetHash([pwd]);
    assert.isTrue(hash?.startsWith(`cache-${process.platform}-`));

    const hashPrefix = "prefix";
    hash = await utils.tryGetHash([pwd], undefined, hashPrefix);
    assert.isTrue(hash?.startsWith(hashPrefix));
  }
}
