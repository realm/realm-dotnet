import * as core from "@actions/core";
import * as fs from "fs";
import {
    configureRealmCli,
    createCluster,
    publishApplication,
    waitForClusterDeployment,
    deleteApplication,
    deleteCluster,
    getSuffix,
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
            differentitingSuffix: getSuffix(core.getInput("cluster-differentiator", { required: true })),
        };

        const appsPath = core.getInput("appsPath", { required: true });

        await configureRealmCli(config);

        if (core.getInput("cleanup", { required: false }) === "true") {
            for (const appName of fs.readdirSync(appsPath)) {
                await deleteApplication(appName, config);
            }

            await deleteCluster(config);
        } else {
            await createCluster(config);
            await waitForClusterDeployment(config);

            const deployedApps: { [key: string]: string } = {};
            for (const appPath of fs.readdirSync(appsPath)) {
                const deployInfo = await publishApplication(path.join(appsPath, appPath), config);
                deployedApps[appPath] = deployInfo.id;
            }

            const deployedAppsOutput = Buffer.from(JSON.stringify(deployedApps)).toString("base64");
            core.setOutput("deployedApps", deployedAppsOutput);
        }
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
