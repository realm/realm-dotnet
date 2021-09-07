![Tests failed](https://img.shields.io/badge/tests-1%20passed%2C%204%20failed%2C%201%20skipped-critical)
## ❌ <a id="user-content-r0" href="#r0">fixtures/jest-junit.xml</a>
**6** tests were completed in **1s** with **1** passed, **4** failed and **1** skipped.
|Test suite|Passed|Failed|Skipped|Time|
|:---|---:|---:|---:|---:|
|[__tests__\main.test.js](#r0s0)|1✔️|3❌||486ms|
|[__tests__\second.test.js](#r0s1)||1❌|1✖️|82ms|
### ❌ <a id="user-content-r0s0" href="#r0s0">__tests__\main.test.js</a>
```
Test 1
  ✔️ Passing test
Test 1 › Test 1.1
  ❌ Failing test
	Error: expect(received).toBeTruthy()
  ❌ Exception in target unit
	Error: Some error
Test 2
  ❌ Exception in test
	Error: Some error
```
### ❌ <a id="user-content-r0s1" href="#r0s1">__tests__\second.test.js</a>
```
❌ Timeout test
	: Timeout - Async callback was not invoked within the 1 ms timeout specified by jest.setTimeout.Timeout - Async callback was not invoked within the 1 ms timeout specified by jest.setTimeout.Error:
✖️ Skipped test
```