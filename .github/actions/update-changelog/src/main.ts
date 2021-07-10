import * as core from "@actions/core";
import { updateChangelogContent } from "./helpers";

async function run(): Promise<void> {
    try {
        const changelogPath = core.getInput("changelog");
        const result = await updateChangelogContent(changelogPath);
        core.setOutput("new-version", result.newVersion);
    } catch (error) {
        core.setFailed(error.message);
    }
}

run();
