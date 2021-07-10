import { expect } from "chai";
import "mocha";
import { suite, test } from "@testdeck/mocha";
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

@suite
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class extractorTests {
    @test
    public patchVersionBump(): void {
        this.testChangelogProcessor(patchBumpChangelog, "1.2.4");
        this.testChangelogUpdater(patchBumpChangelog, "1.2.4");
    }

    @test
    public minorVersionBump(): void {
        this.testChangelogProcessor(minorBumpChangelog, "10.3.0");
        this.testChangelogUpdater(minorBumpChangelog, "10.3.0");
    }

    @test
    public majorVersionBump(): void {
        this.testChangelogProcessor(majorBumpChangelog, "11.0.0");
        this.testChangelogUpdater(majorBumpChangelog, "11.0.0");
    }

    testChangelogProcessor(input: string, expectedVersion: string): void {
        const result = processChangelog(input);
        expect(result.newVersion).to.equal(expectedVersion);

        this.validateUpdatedChangelogContents(result.updatedChangelog, expectedVersion);
    }

    async testChangelogUpdater(input: string, expectedVersion: string): Promise<void> {
        const tempFile = tmp.tmpNameSync();
        try {
            await fs.promises.writeFile(tempFile, input);
            const result = await updateChangelogContent(tempFile);

            expect(result.newVersion).to.equal(expectedVersion);

            const changelog = await fs.promises.readFile(tempFile, { encoding: "utf-8" });
            this.validateUpdatedChangelogContents(changelog, expectedVersion);
        } finally {
            await fs.promises.rm(tempFile);
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
