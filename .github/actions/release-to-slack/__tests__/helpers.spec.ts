import { expect } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
import { getPayload } from "../src/helpers";

const changelog = `
## 10.4.1 (2021-09-12)

### Enhancements
* Added two extension methods on \`IList\` to get an \`IQueryable\` collection wrapping the list:
  * \`list.AsRealmQueryable()\` allows you to get a \`IQueryable<T>\` from \`IList<T>\` that can be then treated as a regular queryable collection and filtered/ordered with LINQ or \`Filter(string)\`.
  * \`list.Filter(query, arguments)\` will filter the list and return the filtered collection. It is roughly equivalent to \`list.AsRealmQueryable().Filter(query, arguments)\`.

  The resulting queryable collection will behave identically to the results obtained by calling \`realm.All<T>()\`, i.e. it will emit notifications when it changes and automatically update itself. (Issue [#1499](https://github.com/realm/realm-dotnet/issues/1499))
* Added a cache for the Realm schema. This will speed up \`Realm.GetInstance\` invocations where \`RealmConfiguration.ObjectClasses\` is explicitly set. The speed gains will depend on the number and complexity of your model classes. A reference benchmark that tests a schema containing all valid Realm property types showed a 25% speed increase of Realm.GetInstance. (Issue [#2194](https://github.com/realm/realm-dotnet/issues/2194))

### Fixed
* Fixed a regression that would prevent the SDK from working on older Linux versions. (Issue [#2602](https://github.com/realm/realm-dotnet/issues/2602))
* Fixed an issue that manifested in circumventing the check for changing a primary key when using the dynamic API - i.e. \`myObj.DynamicApi.Set("Id", "some-new-value")\` will now correctly throw a \`NotSupportedException\` if \`"some-new-value"\` is different from \`myObj\`'s primary key value. (PR [#2601](https://github.com/realm/realm-dotnet/pull/2601))

### Compatibility
* Realm Studio: 11.0.0 or later.

### Internal
* Using Core 11.3.1.
* Started uploading code coverage to coveralls. (Issue [#2586](https://github.com/realm/realm-dotnet/issues/2586))
* Removed the \`[Serializable]\` attribute from RealmObjectBase inheritors. (PR [#2600](https://github.com/realm/realm-dotnet/pull/2600))`;

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class helpersTests {
    @test
    testPayloadGenerator(): void {
        const payload = getPayload(changelog, ".NET", "https://github.com/realm/realm-dotnet");

        expect(payload.blocks).to.have.lengthOf(11);
    }
}
