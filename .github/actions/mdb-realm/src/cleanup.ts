import * as core from "@actions/core";
import { configureRealmCli, deleteApplications, deleteCluster } from "./helpers";
import { EnvironmentConfig } from "./config";

async function run(): Promise<void> {
    try {
        const config: EnvironmentConfig = {
            projectId: core.getInput("projectId", { required: true }),
            apiKey: core.getInput("apiKey", { required: true }),
            privateApiKey: core.getInput("privateApiKey", { required: true }),
        };

        const atlasUrl = core.getInput("atlasUrl", { required: false }) || "https://cloud-dev.mongodb.com/";
        const realmUrl = core.getInput("realmUrl", { required: false }) || "https://realm-dev.mongodb.com/";

        await configureRealmCli(atlasUrl, realmUrl, config);
        await deleteApplications();

        await deleteCluster(atlasUrl, config);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
