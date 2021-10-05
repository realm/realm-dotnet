import * as core from "@actions/core";
import * as fs from "fs";
import { configureRealmCli, publishApplication, getSuffix } from "./helpers";
import { EnvironmentConfig } from "./config";
import path from "path";

async function run(): Promise<void> {
    try {
        const config: EnvironmentConfig = {
            projectId: core.getInput("projectId", { required: true }),
            apiKey: core.getInput("apiKey", { required: true }),
            privateApiKey: core.getInput("privateApiKey", { required: true }),
        };

        const atlasUrl = core.getInput("atlasUrl", { required: false }) || "https://cloud-dev.mongodb.com";
        const realmUrl = core.getInput("realmUrl", { required: false }) || "https://realm-dev.mongodb.com";

        const appSuffix = getSuffix(core.getInput("differentiator", { required: true }));
        const appsPath = core.getInput("appsPath", { required: true });

        await configureRealmCli(atlasUrl, realmUrl, config);

        const deployedApps: { [key: string]: string } = {};
        for (const appPath of fs.readdirSync(appsPath)) {
            const deployInfo = await publishApplication(path.join(appsPath, appPath), appSuffix);
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
