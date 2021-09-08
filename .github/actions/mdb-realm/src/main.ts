import * as core from "@actions/core";
import { configureRealmCli, createCluster } from "./helpers";
import { EnvironmentConfig } from "./config";

async function run(): Promise<void> {
    try {
        const config: EnvironmentConfig = {
            atlasUrl: core.getInput("atlasUrl", { required: false }) || "https://cloud-dev.mongodb.com",
            realmUrl: core.getInput("realmUrl", { required: false }) || "https://realm-dev.mongodb.com",
            projectId: core.getInput("projectId", { required: true }),
            apiKey: core.getInput("apiKey", { required: true }),
            privateApiKey: core.getInput("privateApiKey", { required: true }),
        };

        const clusterInfo = await createCluster(config);

        await configureRealmCli(config);

        core.setOutput("clusterName", clusterInfo.name);
    } catch (error) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
