import {
    generateChartsDashboard,
    uploadBenchmarkResults,
    updateBenchmarkResults,
    extractPackageSizes,
} from "../src/main";
import * as fs from "fs";
import {expect} from "chai";
import * as github from "@actions/github";
import * as path from "path";

const nugetPackagePath = path.resolve(__filename, "..", "FakeRealm.nupkg");

describe("generateChartsDashboard", () => {
    it("contains all items", () => {
        const results = require("./bench-tests.json");
        generateChartsDashboard(results, "dashboard.charts");

        const parsedDashboard = JSON.parse(fs.readFileSync("dashboard.charts", {encoding: "utf8"}));
        const layoutElement = parsedDashboard.dashboards.dashboard.layout;

        expect(layoutElement).to.have.length.greaterThan(1);

        const itemsElement = parsedDashboard.items;

        expect(Object.getOwnPropertyNames(itemsElement)).to.have.length(layoutElement.length);
    });
});

describe("updateBenchmarkResults", () => {
    it("enhances results with git information", async () => {
        const results = require("./bench-tests.json");

        expect(results._id).to.be.undefined;
        expect(results.Commit).to.be.undefined;
        expect(results.CommitMessage).to.be.undefined;
        expect(results.Branch).to.be.undefined;
        expect(results.RunId).to.be.undefined;

        if (!process.env.GITHUB_HEAD_REF) {
            process.env.GITHUB_HEAD_REF = "my-super-branch";
        }

        await updateBenchmarkResults(results, nugetPackagePath);

        if (!Number.isNaN(github.context.runId)) {
            expect(results.RunId).to.equal(github.context.runId);
            expect(results._id).to.equal(results.RunId);
        } else {
            expect(results._id).to.be.NaN;
            expect(results.RunId).to.be.NaN;
        }

        expect(results.Branch).to.equal(process.env.GITHUB_HEAD_REF);
        expect(results.CommitMessage).to.not.be.undefined;
        expect(results.Commit).to.not.be.undefined;
        expect(results.FileSizes).to.have.length.greaterThan(1);
    });
});

// Not really a test, but can be used to upload a one-off benchmark result.
describe("uploadBenchmarkResults", () => {
    it.skip("succeeds", async () => {
        const apiKey = "%ADD_API_KEY%";
        const results = require("./bench-tests.json");
        results.Branch = "main";
        results.Commit = "64ad0b0dc8a834a5172681e452e2562d5195f7fd";
        results.CommitMessage = "Delete me";
        results.RunId = 1006;
        results._id = 1006;
        await uploadBenchmarkResults(apiKey, results);
    });
});

describe("extractPackageSizes", () => {
    it("returns expected results", async () => {
        const sizes = await extractPackageSizes(nugetPackagePath);
        expect(sizes).to.have.length(14);
        for (let sizeInfo of sizes) {
            expect(sizeInfo.size).to.be.greaterThan(1);
        }
    });
});
