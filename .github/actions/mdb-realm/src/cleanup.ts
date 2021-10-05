import * as core from "@actions/core";
import { configureRealmCli, deleteApplications, deleteCluster, getConfig } from "./helpers";

async function run(): Promise<void> {
    try {
        const config = getConfig();

        await configureRealmCli(config);
        await deleteApplications(config);

        await deleteCluster(config);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
