import * as core from "@actions/core";
import * as fs from "fs";
import { configureRealmCli, createCluster, deployApplication, waitForClusterDeployment } from "./helpers";
import { EnvironmentConfig } from "./config";
import path from "path";

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

        const appsPath = core.getInput("appsPath", { required: true });

        const deployedApps: { [key: string]: string } = {};
        for (const appPath of fs.readdirSync(appsPath)) {
            const deployInfo = await deployApplication(path.join(appsPath, appPath), clusterInfo.name);
            deployedApps[deployInfo.name] = deployInfo.id;
        }

        core.setOutput("deployedApps", deployedApps);

        await waitForClusterDeployment(clusterInfo.name, config);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
