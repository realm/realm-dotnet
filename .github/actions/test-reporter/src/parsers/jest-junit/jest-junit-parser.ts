import {ParseOptions, TestParser} from '../../test-parser'
import {parseStringPromise} from 'xml2js'

import {JunitReport, TestCase, TestSuite} from './jest-junit-types'
import {getExceptionSource} from '../../utils/node-utils'
import {getBasePath, normalizeFilePath} from '../../utils/path-utils'

import {
  TestExecutionResult,
  TestRunResult,
  TestSuiteResult,
  TestGroupResult,
  TestCaseResult,
  TestCaseError
} from '../../test-results'

export class JestJunitParser implements TestParser {
  assumedWorkDir: string | undefined

  constructor(readonly options: ParseOptions) {}

  async parse(path: string, content: string): Promise<TestRunResult> {
    const ju = await this.getJunitReport(path, content)
    return this.getTestRunResult(path, ju)
  }

  private async getJunitReport(path: string, content: string): Promise<JunitReport> {
    try {
      return (await parseStringPromise(content)) as JunitReport
    } catch (e) {
      throw new Error(`Invalid XML at ${path}\n\n${e}`)
    }
  }

  private getTestRunResult(path: string, junit: JunitReport): TestRunResult {
    const suites =
      junit.testsuites.testsuite === undefined
        ? []
        : junit.testsuites.testsuite.map(ts => {
            const name = ts.$.name.trim()
            const time = parseFloat(ts.$.time) * 1000
            const sr = new TestSuiteResult(name, this.getGroups(ts), time)
            return sr
          })

    const time = parseFloat(junit.testsuites.$.time) * 1000
    return new TestRunResult(path, suites, time)
  }

  private getGroups(suite: TestSuite): TestGroupResult[] {
    const groups: {describe: string; tests: TestCase[]}[] = []
    for (const tc of suite.testcase) {
      let grp = groups.find(g => g.describe === tc.$.classname)
      if (grp === undefined) {
        grp = {describe: tc.$.classname, tests: []}
        groups.push(grp)
      }
      grp.tests.push(tc)
    }

    return groups.map(grp => {
      const tests = grp.tests.map(tc => {
        const name = tc.$.name.trim()
        const result = this.getTestCaseResult(tc)
        const time = parseFloat(tc.$.time) * 1000
        const error = this.getTestCaseError(tc)
        return new TestCaseResult(name, result, time, error)
      })
      return new TestGroupResult(grp.describe, tests)
    })
  }

  private getTestCaseResult(test: TestCase): TestExecutionResult {
    if (test.failure) return 'failed'
    if (test.skipped) return 'skipped'
    return 'success'
  }

  private getTestCaseError(tc: TestCase): TestCaseError | undefined {
    if (!this.options.parseErrors || !tc.failure) {
      return undefined
    }

    const details = tc.failure[0]
    let path
    let line

    const src = getExceptionSource(details, this.options.trackedFiles, file => this.getRelativePath(file))
    if (src) {
      path = src.path
      line = src.line
    }

    return {
      path,
      line,
      details
    }
  }

  private getRelativePath(path: string): string {
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
