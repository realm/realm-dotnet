import * as core from "@actions/core";

import * as utils from "../../../ga_packages/build_cache_executor/build/utils/common";
import * as input from "../../../ga_packages/build_cache_executor/build/utils/input_parsing";
import * as actionCore from "../../../ga_packages/build_cache_executor/build/build_cache_executor";

async function run(): Promise<void>
{
    try
    {
        const paths = input.parsePaths( core.getInput("cachePaths", { required: true }) );
        const cmds: utils.cmdObj[] = input.tryParseCmdInputArray( core.getInput("cmds", { required: true }) , core);
        const buildResult = await actionCore.actionCore(paths, cmds, utils.tryGetHash, core);
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