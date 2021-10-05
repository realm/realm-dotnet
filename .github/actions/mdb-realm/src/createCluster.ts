import * as core from "@actions/core";
import { createCluster, waitForClusterDeployment } from "./helpers";
import { EnvironmentConfig } from "./config";

async function run(): Promise<void> {
    try {
        const atlasUrl = core.getInput("atlasUrl", { required: false }) || "https://cloud-dev.mongodb.com";
        const config: EnvironmentConfig = {
            projectId: core.getInput("projectId", { required: true }),
            apiKey: core.getInput("apiKey", { required: true }),
            privateApiKey: core.getInput("privateApiKey", { required: true }),
        };

        await createCluster(atlasUrl, config);
        await waitForClusterDeployment(atlasUrl, config);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
