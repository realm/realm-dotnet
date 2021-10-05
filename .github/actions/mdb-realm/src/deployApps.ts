import * as core from "@actions/core";
import * as fs from "fs";
import { configureRealmCli, publishApplication, createCluster, waitForClusterDeployment, getConfig } from "./helpers";
import path from "path";

async function run(): Promise<void> {
    try {
        const config = getConfig();

        const appsPath = core.getInput("appsPath", { required: true });

        await createCluster(config);
        await waitForClusterDeployment(config);

        await configureRealmCli(config);

        const deployedApps: { [key: string]: string } = {};
        for (const appPath of fs.readdirSync(appsPath)) {
            const deployInfo = await publishApplication(path.join(appsPath, appPath), config);
            deployedApps[appPath] = deployInfo.id;
        }

        const deployedAppsOutput = Buffer.from(JSON.stringify(deployedApps)).toString("base64");
        core.setOutput("deployedApps", deployedAppsOutput);
    } catch (error: any) {
        core.setFailed(`An unexpected error occurred: ${error.message}\n${error.stack}`);
    }
}

run();

export default run;
