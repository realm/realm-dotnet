import * as core from "@actions/core";
import * as actionCore from "../../../ga_packages/build_cache_executor/dist/build_cache_executor";

async function run(): Promise<void> {
    try {
        const inputPath = core.getInput("inputPath", { required: true });
        const outputPath = core.getInput("outputPath", { required: true });
        const cmd = core.getInput("cmd", { required: true });
        const outputVar = core.getInput("outputVar");

        const cacheKey = await actionCore.actionCore(inputPath, outputPath, cmd, core);

        core.setOutput(outputVar, cacheKey);
    } catch (error) {
        core.setFailed(error.message);
    }
}

run();

export default run;
