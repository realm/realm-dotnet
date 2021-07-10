import { expect } from "chai";
import "mocha";
import { suite, params } from "@testdeck/mocha";
import { processChangelog, updateChangelogContent } from "../src/helpers";
import moment from "moment";
import * as fs from "fs";
import * as tmp from "tmp";

const patchBumpChangelog = `
## vNext (TBD)

### Fixed
* Something important was fixed

### Enhancements
* None

### Compatibility
* Foo bar

### Internal
* This is super internal

## 1.2.3 (2021-07-07)

### Fixed
* Something

### Enhancements
* Something else

### Compatibility
* Foo bar

### Internal
* This is super internal`;

const minorBumpChangelog = `
## vNext (TBD)

### Breaking Changes
* None

### Fixed
* Something important was fixed

### Enhancements
* This added an amazing enhancmement

### Compatibility
* Foo bar

### Internal
* This is super internal

## 10.2.1 (2021-01-07)

### Fixed
* Something

### Enhancements
* Something else

### Compatibility
* Foo bar

### Internal
* This is super internal`;

const majorBumpChangelog = `
## vNext (TBD)

### Breaking Changes
* Broke some API

### Fixed
* None

### Enhancements
* This added an amazing enhancmement

### Compatibility
* Foo bar

### Internal
* This is super internal

## 10.2.1 (2021-01-07)

### Fixed
* Something

### Enhancements
* Something else

### Compatibility
* Foo bar

### Internal
* This is super internal`;

const preReleaseChangelog = `
## vNext (TBD)

### Breaking Changes
* Broke some API

### Fixed
* Fixed some stuff

### Enhancements
* This added an amazing enhancmement

### Compatibility
* Foo bar

### Internal
* This is super internal

## 3.0.0-beta.2 (2019-02-03)

### Fixed
* Something

### Enhancements
* Something else

### Compatibility
* Foo bar

### Internal
* This is super internal`;


@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class helpersTests {
    @params({ input: patchBumpChangelog, expected: "1.2.4" }, "processor: patch")
    @params({ input: minorBumpChangelog, expected: "10.3.0" }, "processor: minor")
    @params({ input: majorBumpChangelog, expected: "11.0.0" }, "processor: major")
    @params({ input: preReleaseChangelog, expected: "3.0.0-beta.3" }, "processor: pre")
    testChangelogProcessor(args: { input: string, expected: string }): void {
        const result = processChangelog(args.input);
        expect(result.newVersion).to.equal(args.expected);

        this.validateUpdatedChangelogContents(result.updatedChangelog, args.expected);
    }

    @params({ input: patchBumpChangelog, expected: "1.2.4" }, "updater: patch")
    @params({ input: minorBumpChangelog, expected: "10.3.0" }, "updater: minor")
    @params({ input: majorBumpChangelog, expected: "11.0.0" }, "updater: major")
    @params({ input: preReleaseChangelog, expected: "3.0.0-beta.3" }, "updater: pre")
    async testChangelogUpdater(args: { input: string, expected: string }): Promise<void> {
        const tempFile = tmp.tmpNameSync();
        try {
            await fs.promises.writeFile(tempFile, args.input);
            const result = await updateChangelogContent(tempFile);

            expect(result.newVersion).to.equal(args.expected);

            const changelog = await fs.promises.readFile(tempFile, { encoding: "utf-8" });
            this.validateUpdatedChangelogContents(changelog, args.expected);
        } finally {
            await fs.promises.unlink(tempFile);
        }
    }

    validateUpdatedChangelogContents(changelog: string, expectedVersion: string) {
        const todaysDate = moment();
        const year = todaysDate.year().toString().padStart(4, "0");
        const month = (todaysDate.month() + 1).toString().padStart(2, "0");
        const day = todaysDate.date().toString().padStart(2, "0");
        expect(changelog).to.contain(`## ${expectedVersion} (${year}-${month}-${day})\n`);

        expect(changelog).not.to.contain("* None\n");
    }
}
