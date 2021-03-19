import * as core from "@actions/core";
import * as actionCore from "../../../ga_packages/build_cache_executor/dist/build_cache_executor";

async function run(): Promise<void> {
  try {
    const paths = core.getInput("cachePaths", { required: true });
    const cmds = core.getInput("cmds", { required: true });
    let hashPrefix: string | undefined = core.getInput("hashPrefix", {
      required: false,
    });
    hashPrefix = hashPrefix != "" ? hashPrefix : undefined;
    const cacheKey = await actionCore.actionCore(paths, cmds, core, hashPrefix);

    if (cacheKey === undefined) {
      core.setFailed(
        `Action aborted because either artifacts could not be built or they could not be cached`
      );
      return;
    }
    core.setOutput("hashKey", cacheKey);
  } catch (error) {
    core.setFailed(
      `Something went terribly wrong while retrieving the cache and or building: ${error.message}`
    );
  }
}

run();

export default run;
