import * as core from "@actions/core";
import * as fs from "fs";
import { configureRealmCli, deleteApplication, deleteCluster } from "./helpers";
import { EnvironmentConfig } from "./config";

async function run(): Promise<void> {
    try {
        const config: EnvironmentConfig = {
            atlasUrl: core.getInput("atlasUrl", { required: true }),
            realmUrl: core.getInput("realmUrl", { required: true }),
            projectId: core.getInput("projectId", { required: true }),
            apiKey: core.getInput("apiKey", { required: true }),
            privateApiKey: core.getInput("privateApiKey", { required: true }),
        };

        const appsPath = core.getInput("appsPath", { required: true });

        const clusterName = `GHA-${process.env.GITHUB_RUN_ID}`;

        await configureRealmCli(config);

        for (const appName of fs.readdirSync(appsPath)) {
            await deleteApplication(appName);
        }

        await deleteCluster(clusterName, config);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
