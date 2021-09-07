import {ParseOptions, TestParser} from '../../test-parser'

import {getBasePath, normalizeFilePath} from '../../utils/path-utils'

import {
  ReportEvent,
  Suite,
  Group,
  TestStartEvent,
  TestDoneEvent,
  ErrorEvent,
  isSuiteEvent,
  isGroupEvent,
  isTestStartEvent,
  isTestDoneEvent,
  isErrorEvent,
  isDoneEvent,
  isMessageEvent,
  MessageEvent
} from './dart-json-types'

import {
  TestExecutionResult,
  TestRunResult,
  TestSuiteResult,
  TestGroupResult,
  TestCaseResult,
  TestCaseError
} from '../../test-results'

class TestRun {
  constructor(readonly path: string, readonly suites: TestSuite[], readonly success: boolean, readonly time: number) {}
}

class TestSuite {
  constructor(readonly suite: Suite) {}
  readonly groups: {[id: number]: TestGroup} = {}
}

class TestGroup {
  constructor(readonly group: Group) {}
  readonly tests: TestCase[] = []
}

class TestCase {
  constructor(readonly testStart: TestStartEvent) {
    this.groupId = testStart.test.groupIDs[testStart.test.groupIDs.length - 1]
  }
  readonly groupId: number
  readonly print: MessageEvent[] = []
  testDone?: TestDoneEvent
  error?: ErrorEvent

  get result(): TestExecutionResult {
    if (this.testDone?.skipped) {
      return 'skipped'
    }
    if (this.testDone?.result === 'success') {
      return 'success'
    }

    if (this.testDone?.result === 'error' || this.testDone?.result === 'failure') {
      return 'failed'
    }

    return undefined
  }

  get time(): number {
    return this.testDone !== undefined ? this.testDone.time - this.testStart.time : 0
  }
}

export class DartJsonParser implements TestParser {
  assumedWorkDir: string | undefined

  constructor(readonly options: ParseOptions, readonly sdk: 'dart' | 'flutter') {}

  async parse(path: string, content: string): Promise<TestRunResult> {
    const tr = this.getTestRun(path, content)
    const result = this.getTestRunResult(tr)
    return Promise.resolve(result)
  }

  private getTestRun(path: string, content: string): TestRun {
    const lines = content.split(/\n\r?/g)
    const events = lines
      .map((str, i) => {
        if (str.trim() === '') {
          return null
        }
        try {
          return JSON.parse(str)
        } catch (e) {
          const col = e.columnNumber !== undefined ? `:${e.columnNumber}` : ''
          throw new Error(`Invalid JSON at ${path}:${i + 1}${col}\n\n${e}`)
        }
      })
      .filter(evt => evt != null) as ReportEvent[]

    let success = false
    let totalTime = 0
    const suites: {[id: number]: TestSuite} = {}
    const tests: {[id: number]: TestCase} = {}

    for (const evt of events) {
      if (isSuiteEvent(evt)) {
        suites[evt.suite.id] = new TestSuite(evt.suite)
      } else if (isGroupEvent(evt)) {
        suites[evt.group.suiteID].groups[evt.group.id] = new TestGroup(evt.group)
      } else if (isTestStartEvent(evt) && evt.test.url !== null) {
        const test: TestCase = new TestCase(evt)
        const suite = suites[evt.test.suiteID]
        const group = suite.groups[evt.test.groupIDs[evt.test.groupIDs.length - 1]]
        group.tests.push(test)
        tests[evt.test.id] = test
      } else if (isTestDoneEvent(evt) && !evt.hidden && tests[evt.testID]) {
        tests[evt.testID].testDone = evt
      } else if (isErrorEvent(evt) && tests[evt.testID]) {
        tests[evt.testID].error = evt
      } else if (isMessageEvent(evt) && tests[evt.testID]) {
        tests[evt.testID].print.push(evt)
      } else if (isDoneEvent(evt)) {
        success = evt.success
        totalTime = evt.time
      }
    }

    return new TestRun(path, Object.values(suites), success, totalTime)
  }

  private getTestRunResult(tr: TestRun): TestRunResult {
    const suites = tr.suites.map(s => {
      return new TestSuiteResult(this.getRelativePath(s.suite.path), this.getGroups(s))
    })

    return new TestRunResult(tr.path, suites, tr.time)
  }

  private getGroups(suite: TestSuite): TestGroupResult[] {
    const groups = Object.values(suite.groups).filter(grp => grp.tests.length > 0)
    groups.sort((a, b) => (a.group.line ?? 0) - (b.group.line ?? 0))

    return groups.map(group => {
      group.tests.sort((a, b) => (a.testStart.test.line ?? 0) - (b.testStart.test.line ?? 0))
      const tests = group.tests.map(tc => {
        const error = this.getError(suite, tc)
        const testName =
          group.group.name !== undefined && tc.testStart.test.name.startsWith(group.group.name)
            ? tc.testStart.test.name.slice(group.group.name.length).trim()
            : tc.testStart.test.name.trim()
        return new TestCaseResult(testName, tc.result, tc.time, error)
      })
      return new TestGroupResult(group.group.name, tests)
    })
  }

  private getError(testSuite: TestSuite, test: TestCase): TestCaseError | undefined {
    if (!this.options.parseErrors || !test.error) {
      return undefined
    }

    const {trackedFiles} = this.options
    const stackTrace = test.error?.stackTrace ?? ''
    const print = test.print
      .filter(p => p.messageType === 'print')
      .map(p => p.message)
      .join('\n')
    const details = [print, stackTrace].filter(str => str !== '').join('\n')
    const src = this.exceptionThrowSource(details, trackedFiles)
    const message = this.getErrorMessage(test.error?.error ?? '', print)
    let path
    let line

    if (src !== undefined) {
      path = src.path
      line = src.line
    } else {
      const testStartPath = this.getRelativePath(testSuite.suite.path)
      if (trackedFiles.includes(testStartPath)) {
        path = testStartPath
        line = test.testStart.test.root_line ?? test.testStart.test.line ?? undefined
      }
    }

    return {
      path,
      line,
      message,
      details
    }
  }

  private getErrorMessage(message: string, print: string): string {
    if (this.sdk === 'flutter') {
      const uselessMessageRe = /^Test failed\. See exception logs above\.\nThe test description was:/m
      const flutterPrintRe = /^══╡ EXCEPTION CAUGHT BY FLUTTER TEST FRAMEWORK ╞═+\s+(.*)\s+When the exception was thrown, this was the stack:/ms
      if (uselessMessageRe.test(message)) {
        const match = print.match(flutterPrintRe)
        if (match !== null) {
          return match[1]
        }
      }
    }

    return message || print
  }

  private exceptionThrowSource(ex: string, trackedFiles: string[]): {path: string; line: number} | undefined {
    const lines = ex.split(/\r?\n/g)

    // regexp to extract file path and line number from stack trace
    const dartRe = /^(?!package:)(.*)\s+(\d+):\d+\s+/
    const flutterRe = /^#\d+\s+.*\((?!package:)(.*):(\d+):\d+\)$/
    const re = this.sdk === 'dart' ? dartRe : flutterRe

    for (const str of lines) {
      const match = str.match(re)
      if (match !== null) {
        const [_, pathStr, lineStr] = match
        const path = normalizeFilePath(this.getRelativePath(pathStr))
        if (trackedFiles.includes(path)) {
          const line = parseInt(lineStr)
          return {path, line}
        }
      }
    }
  }

  private getRelativePath(path: string): string {
    const prefix = 'file://'
    if (path.startsWith(prefix)) {
      path = path.substr(prefix.length)
    }

    path = normalizeFilePath(path)
    const workDir = this.getWorkDir(path)
    if (workDir !== undefined && path.startsWith(workDir)) {
      path = path.substr(workDir.length)
    }
    return path
  }

  private getWorkDir(path: string): string | undefined {
    return (
      this.options.workDir ??
      this.assumedWorkDir ??
      (this.assumedWorkDir = getBasePath(path, this.options.trackedFiles))
    )
  }
}
