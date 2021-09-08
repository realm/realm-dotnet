import { expect } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class helpersTests {
    @test
    testPayloadGenerator(): void {
        expect(true).to.be.true;
    }
}
