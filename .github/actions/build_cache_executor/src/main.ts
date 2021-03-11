import * as core from "@actions/core";

// this will need to change when the code will have its own repo
import * as build_cache_executor from "@realm/build_cache_executor/lib/build_cache_executor";
import * as utils from "@realm/build_cache_executor/lib/utils/common";
import * as input from "@realm/build_cache_executor/lib/utils/input_parsing";
// import * as utils from "../../../packages/build_cache_executor/src/utils/common";
// import * as input from "../../../packages/build_cache_executor/src/utils/input_parsing";
// import * as actionCore from "../../../packages/build_cache_executor/src/build_cache_executor";

async function run(): Promise<void>
{
    try
    {
        const paths = input.parsePaths( core.getInput("cachePaths", { required: true }) );
        const cmds: utils.cmdObj[] = input.tryParseCmdInputArray( core.getInput("cmds", { required: true }) , core);
        const buildResult = await build_cache_executor.actionCore(paths, cmds, utils.tryGetHash, core);
        if (buildResult.error !== undefined)
        {
            core.setFailed(`This action is aborted because ${buildResult.error.message}`);
        }
    }
    catch (error)
    {
        core.setFailed(`Something went terribly wrong while retrieving the cache and or building: ${error.message}`);
    }
}

run();

export default run;