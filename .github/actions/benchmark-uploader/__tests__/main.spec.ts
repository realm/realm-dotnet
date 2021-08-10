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
        results.Commit = "Test #3";
        results.RunId = 3;
        await uploadBenchmarkResults(apiKey, results);
    });
});
