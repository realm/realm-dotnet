import * as core from "@actions/core";
import * as exec from "@actions/exec";
import * as fs from "fs";
import * as path from "path";
import * as urllib from "urllib";
import { EnvironmentConfig } from "./config";

async function execCmd(cmd: string, args?: string[]): Promise<string> {
    let stdout = "";
    let stderr = "";
    const options: exec.ExecOptions = {
        listeners: {
            stdout: (data: Buffer) => {
                stdout += data.toString();
            },
            stderr: (data: Buffer) => {
                stderr += data.toString();
            },
        },
        failOnStdErr: false,
        ignoreReturnCode: true,
    };

    const exitCode = await exec.exec(cmd, args, options);
    if (exitCode !== 0) {
        throw new Error(`"${cmd}" failed with code ${exitCode} giving error:\n ${stderr.trim()}`);
    }

    return stdout.trim();
}

async function execCliCmd(cmd: string[]): Promise<any[]> {
    try {
        const response = await execCmd("realm-cli", ["--profile", "local", "-f", "json", ...cmd]);
        return response
            .split(/\r?\n/)
            .filter(s => s && s.trim() && !s.includes("Deploying app changes..."))
            .map(s => JSON.parse(s));
    } catch (error: any) {
        if (error.message.indexOf("503") > -1) {
            return await execCliCmd(cmd);
        }

        throw error;
    }
}

async function execAtlasRequest(
    method: urllib.HttpMethod,
    route: string,
    config: EnvironmentConfig,
    payload?: any,
): Promise<any> {
    const url = `${config.atlasUrl}/api/atlas/v1.0/groups/${config.projectId}/${route}`;

    const request: urllib.RequestOptions = {
        digestAuth: `${config.apiKey}:${config.privateApiKey}`,
        method,
        headers: {
            "content-type": "application/json",
            accept: "application/json",
        },
    };

    if (payload) {
        request.data = JSON.stringify(payload);
    }

    const response = await urllib.request(url, request);

    if (response.status < 200 || response.status > 300) {
        throw new Error(`Failed to execute ${request.method} ${route}: ${response.status}: ${response.data}`);
    }

    return JSON.parse(response.data);
}

export async function createCluster(name: string, config: EnvironmentConfig): Promise<void> {
    const payload = {
        name,
        providerSettings: {
            instanceSizeName: "M5",
            providerName: "TENANT",
            regionName: "US_EAST_1",
            backingProviderName: "AWS",
        },
    };

    core.info(`Creating Atlas cluster: ${name}`);

    const response = await execAtlasRequest("POST", "clusters", config, payload);

    core.info(`Cluster created: ${response}`);
}

export async function deleteCluster(name: string, config: EnvironmentConfig): Promise<void> {
    core.info(`Deleting Atlas cluster: ${name}`);

    await execAtlasRequest("DELETE", `clusters/${name}`, config);

    core.info(`Deleted Atlas cluster: ${name}`);
}

export async function waitForClusterDeployment(clusterName: string, config: EnvironmentConfig): Promise<void> {
    const pollDelay = 10;
    let attempt = 0;
    while (attempt++ < 100) {
        try {
            const response = await execAtlasRequest("GET", `clusters/${clusterName}`, config);

            if (response.stateName === "IDLE") {
                return;
            }

            core.info(
                `Cluster state is: ${response.stateName} after ${
                    attempt * pollDelay
                } seconds. Waiting ${pollDelay} seconds for IDLE`,
            );
        } catch (error: any) {
            core.info(`Failed to check cluster status: ${error.message}`);
        }

        await delay(pollDelay * 1000);
    }

    throw new Error(`Cluster failed to deploy after ${100 * pollDelay} seconds`);
}

export async function configureRealmCli(config: EnvironmentConfig): Promise<void> {
    await execCmd("npm i -g mongodb-realm-cli");

    await execCliCmd([
        "login",
        "--api-key",
        config.apiKey,
        "--private-api-key",
        config.privateApiKey,
        "--atlas-url",
        config.atlasUrl,
        "--realm-url",
        config.realmUrl,
    ]);
}

export async function publishApplication(appPath: string, clusterName: string): Promise<{ id: string }> {
    const appName = `${path.basename(appPath)}-${process.env.GITHUB_RUN_ID}`;
    core.info(`Creating app ${appName}`);

    const createResponse = await execCliCmd(["apps", "create", "--name", appName]);

    const appId = createResponse.map(r => r.doc).find(d => d && d.client_app_id).client_app_id;

    core.info(`Created app ${appName} with Id: ${appId}`);

    const secrets = readJson(path.join(appPath, "secrets.json"));

    for (const secret in secrets) {
        core.info(`Importing secret ${secret}`);
        await execCliCmd(["secrets", "create", "--app", appId, "--name", secret, "--value", secrets[secret]]);
    }

    await deployApplication(appPath, clusterName, appId);

    core.info(`Imported ${appName} successfully`);

    return {
        id: appId,
    };
}

export async function deleteApplication(name: string): Promise<void> {
    const appName = `${name}-${process.env.GITHUB_RUN_ID}`;
    const listResponse = await execCliCmd(["apps", "list"]);
    const allApps: string[] = listResponse[0].data;

    const existingApp = allApps.find(a => a.startsWith(appName));

    if (!existingApp) {
        core.info(`Could not find an existing app with name ${appName}. Found apps: ${JSON.stringify(allApps)}`);
        return;
    }

    const appId = existingApp.split(" ")[0];

    core.info(`Deleting ${appName} with id: ${appId}`);

    await execCliCmd(["apps", "delete", "-a", appId]);

    core.info(`Deleted ${appName}`);
}

async function deployApplication(
    appPath: string,
    clusterName: string,
    appId: string,
    syncEnabled = true,
): Promise<void> {
    const backingDBConfigPath = path.join(appPath, "services", "BackingDB", "config.json");
    const backingDBConfig = readJson(backingDBConfigPath);
    backingDBConfig.type = "mongodb-atlas";
    backingDBConfig.config.clusterName = clusterName;

    if (!syncEnabled) {
        delete backingDBConfig.config.sync;
    }

    writeJson(backingDBConfigPath, backingDBConfig);

    core.info(`Updated BackingDB config with cluster: ${clusterName}, sync enabled: ${syncEnabled}`);

    await execCliCmd(["push", "--local", appPath, "--remote", appId, "-y"]);
}

function readJson(filePath: string): any {
    const content = fs.readFileSync(filePath, { encoding: "utf8" });
    return JSON.parse(content);
}

function writeJson(filePath: string, contents: any): void {
    fs.writeFileSync(filePath, JSON.stringify(contents));
}

async function delay(ms: number): Promise<void> {
    return new Promise(resolve => {
        setTimeout(resolve, ms);
    });
}
