import * as core from "@actions/core";
import * as github from "@actions/github";
import * as fs from "fs";
import * as Realm from "realm-web";
import * as path from "path";
import {execCmd} from "./helpers";

async function run(): Promise<void> {
    try {
        const token = core.getInput("realm-token", {required: true});
        const resultsFile = core.getInput("file", {required: true});

        const parsedResults = JSON.parse(fs.readFileSync(resultsFile, {encoding: "utf8"}));

        await updateBenchmarkResults(parsedResults);

        const dashboardPath = core.getInput("dashboard-path", {required: false});
        if (dashboardPath) {
            generateChartsDashboard(parsedResults, path.join(process.env.GITHUB_WORKSPACE || "", dashboardPath));
        }

        await uploadBenchmarkResults(token, parsedResults);
    } catch (error) {
        core.setFailed(error.message);
    }
}

export async function updateBenchmarkResults(results: any): Promise<void> {
    results._id = github.context.runNumber;
    results.RunId = github.context.runNumber;
    results.Commit = await execCmd("git rev-parse HEAD");

    const revListResponse = await execCmd("git rev-list --format=%B --max-count=1 HEAD");
    results.CommitMessage = revListResponse.substring(revListResponse.indexOf("\n") + 1);
    results.Branch = process.env.GITHUB_HEAD_REF || process.env.GITHUB_REF;

    core.info(
        `Inferred git information:\nCommit: ${results.Commit}\nMessage: ${results.CommitMessage}\nBranch: ${results.Branch}`,
    );

    for (const benchmark of results.Benchmarks) {
        if (!benchmark.Parameters) {
            benchmark.Parameters = "N/A";
        }
    }

    core.info(`Updated results id to: ${results._id}`);
}

export function generateChartsDashboard(results: any, dashboardPath: string): void {
    const dashboard = JSON.parse(fs.readFileSync(`${__dirname}/../charts-template.json`, {encoding: "utf8"}));
    const layouts = dashboard.dashboards.dashboard.layout;
    const layoutTemplate = JSON.stringify(layouts[0]);
    layouts.length = 0;

    const items = dashboard.items;
    const itemTemplate = JSON.stringify(items.template);
    delete items.template;

    let currentY = 0;
    for (const benchmark of results.Benchmarks) {
        const newLayout = JSON.parse(layoutTemplate);
        const newItem = JSON.parse(itemTemplate);

        const type = benchmark.Type;
        const method = benchmark.Method;

        const benchmarkId = `${type}-${method}`;

        if (items[benchmarkId]) {
            continue;
        }

        newLayout.i = benchmarkId;
        newLayout.y = currentY;
        layouts.push(newLayout);

        newItem.queryCache.filter = newItem.queryCache.filter.replace(/%METHOD%/g, method).replace(/%TYPE%/g, type);
        newItem.title = `${type}.${method}`;

        // Benchmark.NET inserts ' at the beginning and the end of MethodTitle, so let's trim those
        newItem.description = benchmark.MethodTitle.substring(1, benchmark.MethodTitle.length - 1);
        items[benchmarkId] = newItem;

        currentY = currentY + (newLayout.h as number);
    }

    fs.writeFileSync(dashboardPath, JSON.stringify(dashboard));

    core.info(`Generated dashboard containing ${layouts.length} charts at: ${path}`);
}

export async function uploadBenchmarkResults(apiKey: string, results: any): Promise<void> {
    const app = new Realm.App("sdkbenchmarks-rolry");
    const credentials = Realm.Credentials.apiKey(apiKey);

    core.info("Authenticating Realm user...");

    const user = await app.logIn(credentials);

    const mongodb = user.mongoClient("mongodb-atlas");

    core.info(`Inserting benchmark results with Id: ${results._id}`);

    const response = await mongodb.db("benchmarks").collection("results").insertOne(results);

    core.info(`Inserted results: ${response.insertedId}`);
}

void run();
