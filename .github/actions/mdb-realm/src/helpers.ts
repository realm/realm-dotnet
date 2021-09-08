import * as exec from "@actions/exec";
import * as urllib from "urllib";
import * as uuid from "uuid";
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
    };

    const exitCode = await exec.exec(cmd, args, options);
    if (exitCode !== 0) {
        throw new Error(`"${cmd}" failed with code ${exitCode} giving error:\n ${stderr.trim()}`);
    }

    return stdout.trim();
}

async function execCliCmd(cmd: string): Promise<string> {
    return await execCmd(`realm-cli --profile local ${cmd}`);
}

export async function createCluster(config: EnvironmentConfig): Promise<{ name: string; id: string }> {
    const url = `${config.atlasUrl}/api/atlas/v1.0/groups/${config.projectId}/clusters`;
    const payload = {
        name: uuid.v4(),
        providerSettings: {
            instanceSizeName: "M2",
            providerName: "TENANT",
            regionName: "US_EAST_1",
            backingProviderName: "AWS",
        },
    };

    const response = await urllib.request(url, {
        digestAuth: `${config.apiKey}:${config.privateApiKey}`,
        method: "POST",
        data: payload,
    });

    return {
        name: payload.name,
        id: response.data.id,
    };
}

export async function configureRealmCli(config: EnvironmentConfig): Promise<void> {
    await execCmd("npm i -g mongodb-realm-cli");

    await execCliCmd(
        `login --api-key ${config.apiKey} --private-api-key ${config.privateApiKey} --atlas-url ${config.atlasUrl} --realm-url ${config.realmUrl}`,
    );
}
