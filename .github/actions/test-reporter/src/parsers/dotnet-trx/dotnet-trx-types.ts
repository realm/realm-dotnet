export interface TrxReport {
  TestRun: TestRun
}

export interface TestRun {
  Times: Times[]
  Results?: Results[]
  TestDefinitions?: TestDefinitions[]
}

export interface Times {
  $: {
    creation: string
    queuing: string
    start: string
    finish: string
  }
}

export interface TestDefinitions {
  UnitTest: UnitTest[]
}

export interface UnitTest {
  $: {
    id: string
  }
  TestMethod: TestMethod[]
}

export interface TestMethod {
  $: {
    className: string
    name: string
  }
}

export interface Results {
  UnitTestResult: UnitTestResult[]
}

export interface UnitTestResult {
  $: {
    testId: string
    testName: string
    duration?: string
    outcome: Outcome
  }
  Output: Output[]
}

export interface Output {
  ErrorInfo: ErrorInfo[]
}
export interface ErrorInfo {
  Message: string[]
  StackTrace: string[]
}

export type Outcome = 'Passed' | 'NotExecuted' | 'Failed'
