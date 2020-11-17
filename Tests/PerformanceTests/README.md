# About Synthetic Benchmarks

The Realm .NET SDK uses [Benchmark.NET](http://benchmarkdotnet.org) to run synthetic benchmarks against the database. These are typically not intended to cover every possible scenario, but rather measure the performance of most common operations and provide historical data that will help us uncover performance regressions.

## Running the benchmarks

To run all tests, use `dotnet run -c Release -f net5.0 -- -f *`. To run only a subset of the tests, replace `*` with a regex that will match the tests you want to run. To list all benchmarks, use `dotnet run -c Release -f net5.0 -- --list`. Refere to [the docs](https://benchmarkdotnet.org/articles/guides/console-args.html) for a complete list of the supported arguments.

## Best practices

The .NET Performance project has an [amazing guide](https://github.com/dotnet/performance/blob/master/docs/microbenchmark-design-guidelines.md) on writing quality microbenchmarks (most Realm .NET benchmarks can be qualified as micro-). One area where we don't follow it to the letter is related to the "no side effects" rule. Since we're measuring the performance of a database, that inherently introduces side effects every time we're committing a transaction. So we take the less compelling "try to avoid side effects" stance.

## (WIP) Historical performance

This is still not implemented. Ultimately, we would like to have a historical overview of the performance of various commits, using a tool like [GitHub Action for Continuous Benchmarking](https://github.com/rhysd/github-action-benchmark).

## (WIP) Running tests on different platforms

This is still not implemented. Currently, only .NET 5 is supported as a target framework. We would like to run benchmark tests at least on .NET 5 and Xamarin.

