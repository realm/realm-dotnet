import { expect } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import { getPayload } from "../src/helpers";

const changelog = `### Fixed
* Fixed a regression that would prevent the SDK from working on older Linux versions. (Issue [#2602](realm/realm-dotnet/issues/2602))
* Fixed an issue that manifested in circumventing the check for changing a primary key when using the dynamic API - i.e. \`myObj.DynamicApi.Set("Id", "some-new-value")\` will now correctly throw a \`NotSupportedException\` if \`"some-new-value"\` is different from \`myObj\`'s primary key value. (PR [#2601](realm/realm-dotnet/pull/2601))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.3.1.
* Started uploading code coverage to coveralls. (Issue [#2586](realm/realm-dotnet/issues/2586))
* Removed the \`[Serializable]\` attribute from RealmObjectBase inheritors. (PR [#2600](realm/realm-dotnet/pull/2600))`;

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class helpersTests {
    @test
    testPayloadGenerator(): void {
        const payload = getPayload(changelog, ".NET", "https://github.com/realm/realm-dotnet", "10.4.1");

        expect(payload.blocks).to.have.lengthOf(9);
    }
}
