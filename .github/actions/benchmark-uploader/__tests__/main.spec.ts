import {generateChartsDashboard, uploadBenchmarkResults} from "../src/main";

describe("generateChartsDashboard", () => {
    it("Generates charts for all items", () => {
        const results = require("./bench-tests.json");
        generateChartsDashboard(results, "dashboard.charts");
    });

    it.skip("Uploads benchmarks to MDB Realm", async () => {
        const apiKey = "%ADD_API_KEY%";
        const results = require("./bench-tests.json");
        results.Branch = "master";
        results.Commit = "64ad0b0dc8a834a5172681e452e2562d5195f7fd";
        results.CommitMessage = "Delete me";
        results.RunId = 1006;
        results._id = 1006;
        await uploadBenchmarkResults(apiKey, results);
    });
});
