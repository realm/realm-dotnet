import { assert } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import { sumValues } from "../src/main";

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class sumTests {
    @test
    public sumPositives(): void {
        const result = sumValues(1, 2);
        assert.equal(result, 3);
    }

    @test
    public sumNegatives(): void {
        const result = sumValues(-1, -5);
        assert.equal(result, -6);
    }
}