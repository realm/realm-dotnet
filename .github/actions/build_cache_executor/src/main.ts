import * as core from "@actions/core";
import * as actionCore from "../../../ga_packages/build_cache_executor/dist/build_cache_executor";

async function run(): Promise<void> {
    try {
        const path = core.getInput("cachePath", { required: true });
        const cmd = core.getInput("cmd", { required: true });
        const outputVar = core.getInput("outputVar");

        const cacheKey = await actionCore.actionCore(path, cmd, core);

        core.setOutput(outputVar || "hashKey", cacheKey);
    } catch (error) {
        core.setFailed(error.message);
    }
}

run();

export default run;
