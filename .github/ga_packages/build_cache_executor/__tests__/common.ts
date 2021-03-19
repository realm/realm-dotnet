import { assert } from "chai";
import "mocha";
import { suite, test, timeout } from "@testdeck/mocha";
import * as utils from "../src/utils/common";
import * as path from "path";

@suite
class CommonUtils {
  @test
  @timeout(3500)
  async VerifyConsistentHashing() {
    const pwd = __dirname;
    const oneUp = path.resolve(path.join(pwd, "../"));

    const hashMap = new Map<string, string>();
    let hash = await utils.tryGetHash([pwd]);
    if (hash !== undefined) {
      hashMap.set(hash, pwd);
    } else {
      assert.fail(`It was impossible to calculate the hash for ${pwd}`);
    }
    hash = await utils.tryGetHash([oneUp]);
    if (hash !== undefined) {
      hashMap.set(hash, oneUp);
    } else {
      assert.fail(`It was impossible to calculate the hash for ${oneUp}`);
    }

    // recalculate to verify consistency
    hash = await utils.tryGetHash([pwd]);
    if (hash !== undefined) {
      assert.equal(hashMap.get(hash), pwd);
    } else {
      assert.fail(`It was impossible to re-calculate the hash for ${pwd}`);
    }
    hash = await utils.tryGetHash([oneUp]);
    if (hash !== undefined) {
      assert.equal(hashMap.get(hash), oneUp);
    } else {
      assert.fail(`It was impossible to re-calculate the hash for ${oneUp}`);
    }
  }

  @test
  async VerifyHashPrefix() {
    const pwd = __dirname;
    let hash = await utils.tryGetHash([pwd]);
    assert.isTrue(hash?.startsWith(`cache-${process.platform}-`));

    const hashPrefix = "prefix";
    hash = await utils.tryGetHash([pwd], undefined, hashPrefix);
    assert.isTrue(hash?.startsWith(hashPrefix));
  }
}
