![Tests failed](https://img.shields.io/badge/tests-5%20passed%2C%205%20failed%2C%201%20skipped-critical)
## ❌ <a id="user-content-r0" href="#r0">fixtures/dotnet-trx.trx</a>
**11** tests were completed in **1s** with **5** passed, **5** failed and **1** skipped.
|Test suite|Passed|Failed|Skipped|Time|
|:---|---:|---:|---:|---:|
|[DotnetTests.XUnitTests.CalculatorTests](#r0s0)|5✔️|5❌|1✖️|118ms|
### ❌ <a id="user-content-r0s0" href="#r0s0">DotnetTests.XUnitTests.CalculatorTests</a>
```
✔️ Custom Name
❌ Exception_In_TargetTest
	System.DivideByZeroException : Attempted to divide by zero.
❌ Exception_In_Test
	System.Exception : Test
❌ Failing_Test
	Assert.Equal() Failure
	Expected: 3
	Actual:   2
✔️ Is_Even_Number(i: 2)
❌ Is_Even_Number(i: 3)
	Assert.True() Failure
	Expected: True
	Actual:   False
✔️ Passing_Test
✔️ Should be even number(i: 2)
❌ Should be even number(i: 3)
	Assert.True() Failure
	Expected: True
	Actual:   False
✖️ Skipped_Test
✔️ Timeout_Test
```