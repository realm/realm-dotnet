import {generateChartsDashboard} from "../src/main";

describe("generateChartsDashboard", () => {
    it("Generates charts for all items", () => {
        const results = require("./bench-tests.json");
        generateChartsDashboard(results, "dashboard.charts");
    });
});
