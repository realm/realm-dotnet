import * as path from 'path'
import {ParseOptions, TestParser} from '../../test-parser'
import {parseStringPromise} from 'xml2js'

import {JunitReport, SingleSuiteReport, TestCase, TestSuite} from './java-junit-types'
import {normalizeFilePath} from '../../utils/path-utils'

import {
  TestExecutionResult,
  TestRunResult,
  TestSuiteResult,
  TestGroupResult,
  TestCaseResult,
  TestCaseError
} from '../../test-results'

export class JavaJunitParser implements TestParser {
  readonly trackedFiles: {[fileName: string]: string[]}

  constructor(readonly options: ParseOptions) {
    // Map to efficient lookup of all paths with given file name
    this.trackedFiles = {}
    for (const filePath of options.trackedFiles) {
      const fileName = path.basename(filePath)
      const files = this.trackedFiles[fileName] ?? (this.trackedFiles[fileName] = [])
      files.push(normalizeFilePath(filePath))
    }
  }

  async parse(filePath: string, content: string): Promise<TestRunResult> {
    const reportOrSuite = await this.getJunitReport(filePath, content)
    const isReport = (reportOrSuite as JunitReport).testsuites !== undefined

    // XML might contain:
    // - multiple suites under <testsuites> root node
    // - single <testsuite> as root node
    let ju: JunitReport
    if (isReport) {
      ju = reportOrSuite as JunitReport
    } else {
      // Make it behave the same way as if suite was inside <testsuites> root node
      const suite = (reportOrSuite as SingleSuiteReport).testsuite
      ju = {
        testsuites: {
          $: {time: suite.$.time},
          testsuite: [suite]
        }
      }
    }

    return this.getTestRunResult(filePath, ju)
  }

  private async getJunitReport(filePath: string, content: string): Promise<JunitReport | SingleSuiteReport> {
    try {
      return await parseStringPromise(content)
    } catch (e) {
      throw new Error(`Invalid XML at ${filePath}\n\n${e}`)
    }
  }

  private getTestRunResult(filePath: string, junit: JunitReport): TestRunResult {
    const suites =
      junit.testsuites.testsuite === undefined
        ? []
        : junit.testsuites.testsuite.map(ts => {
            const name = ts.$.name.trim()
            const time = parseFloat(ts.$.time) * 1000
            const sr = new TestSuiteResult(name, this.getGroups(ts), time)
            return sr
          })

    const seconds = parseFloat(junit.testsuites.$?.time)
    const time = isNaN(seconds) ? undefined : seconds * 1000
    return new TestRunResult(filePath, suites, time)
  }

  private getGroups(suite: TestSuite): TestGroupResult[] {
    if (suite.testcase === undefined) {
      return []
    }

    const groups: {name: string; tests: TestCase[]}[] = []
    for (const tc of suite.testcase) {
      // Normally classname is same as suite name - both refer to same Java class
      // Therefore it doesn't make sense to process it as a group
      // and tests will be added to default group with empty name
      const className = tc.$.classname === suite.$.name ? '' : tc.$.classname
      let grp = groups.find(g => g.name === className)
      if (grp === undefined) {
        grp = {name: className, tests: []}
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
      return new TestGroupResult(grp.name, tests)
    })
  }

  private getTestCaseResult(test: TestCase): TestExecutionResult {
    if (test.failure || test.error) return 'failed'
    if (test.skipped) return 'skipped'
    return 'success'
  }

  private getTestCaseError(tc: TestCase): TestCaseError | undefined {
    if (!this.options.parseErrors) {
      return undefined
    }

    // We process <error> and <failure> the same way
    const failures = tc.failure ?? tc.error
    if (!failures) {
      return undefined
    }

    const failure = failures[0]
    const details = typeof failure === 'object' ? failure._ : failure
    let filePath
    let line

    const src = this.exceptionThrowSource(details)
    if (src) {
      filePath = src.filePath
      line = src.line
    }

    return {
      path: filePath,
      line,
      details,
      message: typeof failure === 'object' ? failure.message : undefined
    }
  }

  private exceptionThrowSource(stackTrace: string): {filePath: string; line: number} | undefined {
    const lines = stackTrace.split(/\r?\n/)
    const re = /^at (.*)\((.*):(\d+)\)$/

    for (const str of lines) {
      const match = str.match(re)
      if (match !== null) {
        const [_, tracePath, fileName, lineStr] = match
        const filePath = this.getFilePath(tracePath, fileName)
        if (filePath !== undefined) {
          const line = parseInt(lineStr)
          return {filePath, line}
        }
      }
    }
  }

  // Stacktrace in Java doesn't contain full paths to source file.
  // There are only package, file name and line.
  // Assuming folder structure matches package name (as it should in Java),
  // we can try to match tracked file.
  private getFilePath(tracePath: string, fileName: string): string | undefined {
    // Check if there is any tracked file with given name
    const files = this.trackedFiles[fileName]
    if (files === undefined) {
      return undefined
    }

    // Remove class name and method name from trace.
    // Take parts until first item with capital letter - package names are lowercase while class name is CamelCase.
    const packageParts = tracePath.split(/\./g)
    const packageIndex = packageParts.findIndex(part => part[0] <= 'Z')
    if (packageIndex !== -1) {
      packageParts.splice(packageIndex, packageParts.length - packageIndex)
    }

    if (packageParts.length === 0) {
      return undefined
    }

    // Get right file
    // - file name matches
    // - parent folders structure must reflect the package name
    for (const filePath of files) {
      const dirs = path.dirname(filePath).split(/\//g)
      if (packageParts.length > dirs.length) {
        continue
      }
      // get only N parent folders, where N = length of package name parts
      if (dirs.length > packageParts.length) {
        dirs.splice(0, dirs.length - packageParts.length)
      }
      // check if parent folder structure matches package name
      const isMatch = packageParts.every((part, i) => part === dirs[i])
      if (isMatch) {
        return filePath
      }
    }

    return undefined
  }
}
