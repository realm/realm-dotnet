import * as core from "@actions/core";
import * as fs from "fs";
import {
    configureRealmCli,
    createCluster,
    deleteApplication,
    deleteCluster,
    publishApplication,
    waitForClusterDeployment,
} from "./helpers";
import { EnvironmentConfig } from "./config";
import path from "path";

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

        const isCleanup = core.getInput("cleanup", { required: false }) === "true";
        const clusterName = `GHA-${process.env.GITHUB_RUN_ID}`;

        await configureRealmCli(config);

        if (isCleanup) {
            for (const appPath of fs.readdirSync(appsPath)) {
                await deleteApplication(path.join(appsPath, appPath), clusterName);
            }

            await deleteCluster(clusterName, config);
        } else {
            await createCluster(clusterName, config);

            const deployedApps: { [key: string]: string } = {};
            for (const appPath of fs.readdirSync(appsPath)) {
                const deployInfo = await publishApplication(path.join(appsPath, appPath), clusterName);
                deployedApps[appPath] = deployInfo.id;
            }

            core.setOutput("deployedApps", deployedApps);

            await waitForClusterDeployment(clusterName, config);
        }
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
