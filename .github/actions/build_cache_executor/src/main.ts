import * as core from "@actions/core";
import * as actionCore from "../../../ga_packages/build_cache_executor/dist/build_cache_executor";

async function run(): Promise<void> {
  try {
    const path = core.getInput("cachePath", { required: true });
    const cmd = core.getInput("cmd", { required: true });

    const cacheKey = await actionCore.actionCore(path, cmd, core);

    if (cacheKey === undefined) {
      core.setFailed(
        `Action aborted because either artifacts could not be built or they could not be cached`
      );
      return;
    }
    core.setOutput("hashKey", cacheKey);
  } catch (error) {
    core.setFailed(`Hard failure: ${error.message}`);
  }
}

run();

export default run;
