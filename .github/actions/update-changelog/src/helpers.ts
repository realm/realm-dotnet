import * as semver from "semver";
import moment from "moment";
import * as fs from "fs";

const changelogRegex = /^## (?<currentVersion>[^\n]*)[^#]*(?<sections>.*?)\n## (?<prevVersion>[^ ]*)/gms;
const sectionsRegex = /### (?<sectionName>[^\n]*)(?<sectionContent>[^#]*)/gm;

export function processChangelog(changelog: string): { updatedChangelog: string; newVersion: string } {
    changelogRegex.lastIndex = 0;
    sectionsRegex.lastIndex = 0;

    const changelogMatch = changelogRegex.exec(changelog);
    if (!changelogMatch || !changelogMatch.groups) {
        throw new Error("Failed to match changelog");
    }

    const prevVersion = changelogMatch.groups["prevVersion"];

    let newVersion = prevVersion;
    let sectionMatch: RegExpExecArray | null;
    while ((sectionMatch = sectionsRegex.exec(changelogMatch.groups["sections"]))) {
        if (!sectionMatch.groups) {
            throw new Error("Failed to match sections");
        }

        if (!hasActualChanges(sectionMatch)) {
            changelog = changelog.replace(sectionMatch[0], "");
            continue;
        }

        let inferredNewVersion: string;
        switch (sectionMatch.groups["sectionName"]) {
            case "Fixed":
                inferredNewVersion = semver.inc(prevVersion, "patch")!;
                break;
            case "Enhancements":
                inferredNewVersion = semver.inc(prevVersion, "minor")!;
                break;
            case "Breaking Changes":
                inferredNewVersion = semver.inc(prevVersion, "major")!;
                break;
            default:
                continue;
        }

        if (semver.gt(inferredNewVersion, newVersion)) {
            newVersion = inferredNewVersion;
        }
    }

    const versionToReplace = changelogMatch.groups["currentVersion"];
    const todaysDate = moment().format("YYYY-MM-DD");
    changelog = changelog.replace(`## ${versionToReplace}\n`, `## ${newVersion} (${todaysDate})\n`);
    return {
        updatedChangelog: changelog,
        newVersion,
    };
}

export async function updateChangelogContent(path: string): Promise<{ newVersion: string }> {
    const changelog = await fs.promises.readFile(path, { encoding: "utf-8" });

    const changelogUpdate = processChangelog(changelog);
    await fs.promises.writeFile(path, changelogUpdate.updatedChangelog, { encoding: "utf-8" });

    return {
        newVersion: changelogUpdate.newVersion,
    };
}

function hasActualChanges(sectionMatch: RegExpExecArray): boolean {
    const content = sectionMatch.groups!["sectionContent"];
    return content.includes("*") && !content.includes("* None\n");
}
