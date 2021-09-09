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

async function execCliCmd(cmd: string): Promise<any[]> {
    try {
        const response = await execCmd(`realm-cli --profile local -f json ${cmd}`);
        return response.split(/\r?\n/).map(s => JSON.parse(s));
    } catch (error: any) {
        if (error.message.indexOf("503") > -1) {
            return await execCliCmd(cmd);
        }

        throw error;
    }
}

async function execAtlasRequest(route: string, payload: any, config: EnvironmentConfig): Promise<any> {
    const url = `${config.atlasUrl}/api/atlas/v1.0/groups/${config.projectId}/${route}`;

    const request: urllib.RequestOptions = {
        digestAuth: `${config.apiKey}:${config.privateApiKey}`,
        method: "GET",
    };

    if (payload) {
        request.method = "POST";
        request.data = JSON.stringify(payload);
        request.headers = {
            "content-type": "application/json",
        };
    }

    const response = await urllib.request(url, request);

    if (response.status < 200 || response.status > 300) {
        throw new Error(`Failed to execute ${request.method} ${route}: ${response.status}: ${response.data}`);
    }

    return response.data;
}

export async function createCluster(config: EnvironmentConfig): Promise<{ name: string; id: string }> {
    const payload = {
        name: `GHA-${process.env.GITHUB_RUN_ID}`,
        providerSettings: {
            instanceSizeName: "M2",
            providerName: "TENANT",
            regionName: "US_EAST_1",
            backingProviderName: "AWS",
        },
    };

    core.info("Creating Atlas cluster");

    const response = await execAtlasRequest("clusters", payload, config);

    core.info(`Cluster created: ${response}`);

    return {
        name: payload.name,
        id: response.id,
    };
}

export async function waitForClusterDeployment(clusterName: string, config: EnvironmentConfig): Promise<void> {
    const pollDelay = 10;
    let attempt = 0;
    while (attempt++ < 100) {
        try {
            const response = await execAtlasRequest(`clusters/${clusterName}`, undefined, config);

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

    await execCliCmd(
        `login --api-key ${config.apiKey} --private-api-key ${config.privateApiKey} --atlas-url ${config.atlasUrl} --realm-url ${config.realmUrl}`,
    );
}

export async function deployApplication(appPath: string, clusterName: string): Promise<{ name: string; id: string }> {
    const appConfig = readAppConfig(appPath);

    const appName = `${appConfig.name}-${process.env.GITHUB_RUN_ID}`;
    core.info(`Creating app ${appName}`);

    const createResponse = await execCliCmd(`apps create --name ${appName}`);

    const appId = createResponse.map(r => r.doc).find(d => d && d.client_app_id).client_app_id;

    core.info(`Created app ${appName} with Id: ${appId}`);

    const secrets = readJson(path.join(appPath, "secrets.json"));

    for (const secret in secrets) {
        core.info(`Importing secret ${secret}`);
        await execCliCmd(`secrets create --app ${appId} --name "${secret}" --value "${secrets[secret]}"`);
    }

    const backingDBConfigPath = path.join(appPath, "services", "BackingDB", "config.json");
    const backingDBConfig = readJson(backingDBConfigPath);
    backingDBConfig.type = "mongodb-atlas";
    backingDBConfig.config.clusterName = clusterName;

    writeJson(backingDBConfigPath, backingDBConfig);

    core.info(`Updated BackingDB config with cluster: ${clusterName}`);

    await execCliCmd(`push --local ${appPath} --remote ${appId} -y`);

    core.info(`Imported ${appName} successfully`);

    return {
        name: appConfig.name,
        id: appId,
    };
}

function readAppConfig(appPath: string): any {
    const legacyConfigPath = path.join(appPath, "config.json");
    if (fs.existsSync(legacyConfigPath)) {
        return readJson(legacyConfigPath);
    }

    return readJson(path.join(appPath, "realm_config.json"));
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
