import * as core from "@actions/core";
import * as exec from "@actions/exec";
import * as fs from "fs";
import * as path from "path";
import * as urllib from "urllib";
import { EnvironmentConfig } from "./config";
import { createHash } from "crypto";

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

async function execCliCmd(cmd: string, retries = 5): Promise<any[]> {
    // eslint-disable-next-line no-constant-condition
    while (true) {
        try {
            let actualCmd = `realm-cli --profile local -f json ${cmd} -y`;
            if (process.platform === "win32") {
                actualCmd = `pwsh -Command "${actualCmd.replace(/"/g, '\\"').split("\n").join("`n")}"`;
            }
            const response = await execCmd(actualCmd);
            return response
                .split(/\r?\n/)
                .filter(s => s && s.trim() && !s.includes("Deploying app changes..."))
                .map(s => JSON.parse(s));
        } catch (error: any) {
            if (retries-- < 2) {
                throw error;
            } else {
                core.info(`Failed to execute ${cmd} with ${error}. Retrying ${retries} more time(s)`);
            }
        }
    }
}

async function execAtlasRequest(
    atlasUrl: string,
    method: urllib.HttpMethod,
    route: string,
    config: EnvironmentConfig,
    payload?: any,
): Promise<any> {
    const url = `${atlasUrl}/api/atlas/v1.0/groups/${config.projectId}/${route}`;

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

export function getSuffix(differentiator: string): string {
    return createHash("md5")
        .update(`${getRunId()}-${differentiator}`)
        .digest("base64")
        .replace(/\+/g, "")
        .replace(/\//g, "")
        .toLowerCase()
        .substring(0, 8);
}

function getRunId(): string {
    return process.env.GITHUB_RUN_ID || "";
}

function getClusterName(): string {
    return `GHA-${getRunId()}`;
}

function getAppName(name: string, suffix: string): string {
    return `${name}-${suffix}`;
}

export async function createCluster(atlasUrl: string, config: EnvironmentConfig): Promise<void> {
    const clusterName = getClusterName();
    const payload = {
        name: clusterName,
        providerSettings: {
            instanceSizeName: "M10",
            providerName: "AWS",
            regionName: "US_EAST_1",
        },
    };

    core.info(`Creating Atlas cluster: ${clusterName}`);

    const response = await execAtlasRequest(atlasUrl, "POST", "clusters", config, payload);

    core.info(`Cluster created: ${JSON.stringify(response)}`);
}

export async function deleteCluster(atlasUrl: string, config: EnvironmentConfig): Promise<void> {
    const clusterName = getClusterName();
    core.info(`Deleting Atlas cluster: ${clusterName}`);

    await execAtlasRequest(atlasUrl, "DELETE", `clusters/${clusterName}`, config);

    core.info(`Deleted Atlas cluster: ${clusterName}`);
}

export async function waitForClusterDeployment(atlasUrl: string, config: EnvironmentConfig): Promise<void> {
    const clusterName = getClusterName();
    const pollDelay = 5;
    let attempt = 0;
    while (attempt++ < 200) {
        try {
            const response = await execAtlasRequest(atlasUrl, "GET", `clusters/${clusterName}`, config);

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

export async function configureRealmCli(atlasUrl: string, realmUrl: string, config: EnvironmentConfig): Promise<void> {
    await execCmd("npm i -g mongodb-realm-cli");

    await execCliCmd(
        `login --api-key ${config.apiKey} --private-api-key ${config.privateApiKey} --atlas-url ${atlasUrl} --realm-url ${realmUrl}`,
    );
}

export async function publishApplication(appPath: string, appSuffix: string): Promise<{ id: string }> {
    const clusterName = getClusterName();
    const appName = getAppName(path.basename(appPath), appSuffix);
    core.info(`Creating app ${appName}`);

    const createResponse = await execCliCmd(`apps create --name ${appName}`);

    const appId = createResponse.map(r => r.doc).find(d => d && d.client_app_id).client_app_id;

    core.info(`Created app ${appName} with Id: ${appId}`);

    const secrets = readJson(path.join(appPath, "secrets.json"));

    for (const secret in secrets) {
        if (secret === "BackingDB_uri") {
            continue;
        }

        core.info(`Importing secret ${secret}`);
        await execCliCmd(`secrets create --app ${appId} --name "${secret}" --value "${secrets[secret]}"`);
    }

    // This code does the following:
    // 1. Updates the service type to mongodb-atlas (instead of mongo)
    // 2. Updates the linked cluster to match the one we just created
    // 3. Deletes the secret config since that is only relevant for the docker deployment
    const backingDBConfigPath = path.join(appPath, "services", "BackingDB", "config.json");
    const backingDBConfig = readJson(backingDBConfigPath);
    backingDBConfig.type = "mongodb-atlas";
    backingDBConfig.config.clusterName = clusterName;
    backingDBConfig.config.sync.database_name += `-${getRunId()}`;
    delete backingDBConfig.secret_config;

    writeJson(backingDBConfigPath, backingDBConfig);

    core.info(`Updated BackingDB config with cluster: ${clusterName}`);
    core.info(`Updated BackingDB database to: ${backingDBConfig.config.sync.database_name}`);

    await execCliCmd(`push --local ${appPath} --remote ${appId}`);

    core.info(`Imported ${appName} successfully`);

    return {
        id: appId,
    };
}

export async function deleteApplications(): Promise<void> {
    const clusterName = getClusterName();
    const listResponse = await execCliCmd("apps list");
    const allApps: string[] = listResponse[0].data;

    for (const app of allApps) {
        const appId = app.split(" ")[0];
        const describeResponse = await execCliCmd(`apps describe -a ${appId}`);
        if (describeResponse[0]?.doc.data_sources[0]?.data_source === clusterName) {
            core.info(`Deleting ${appId}`);
            await execCliCmd(`apps delete -a ${appId}`);
            core.info(`Deleted ${appId}`);
        }
    }
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
