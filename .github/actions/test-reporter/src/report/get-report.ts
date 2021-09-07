import * as core from '@actions/core'
import {TestExecutionResult, TestRunResult, TestSuiteResult} from '../test-results'
import {Align, formatTime, Icon, link, table} from '../utils/markdown-utils'
import {getFirstNonEmptyLine} from '../utils/parse-utils'
import {slug} from '../utils/slugger'

const MAX_REPORT_LENGTH = 65535

export interface ReportOptions {
  listSuites: 'all' | 'failed'
  listTests: 'all' | 'failed' | 'none'
  baseUrl: string
  onlySummary: boolean
}

const defaultOptions: ReportOptions = {
  listSuites: 'all',
  listTests: 'all',
  baseUrl: '',
  onlySummary: false
}

export function getReport(results: TestRunResult[], options: ReportOptions = defaultOptions): string {
  core.info('Generating check run summary')

  applySort(results)

  const opts = {...options}
  let lines = renderReport(results, opts)
  let report = lines.join('\n')

  if (getByteLength(report) <= MAX_REPORT_LENGTH) {
    return report
  }

  if (opts.listTests === 'all') {
    core.info("Test report summary is too big - setting 'listTests' to 'failed'")
    opts.listTests = 'failed'
    lines = renderReport(results, opts)
    report = lines.join('\n')
    if (getByteLength(report) <= MAX_REPORT_LENGTH) {
      return report
    }
  }

  core.warning(`Test report summary exceeded limit of ${MAX_REPORT_LENGTH} bytes and will be trimmed`)
  return trimReport(lines)
}

function trimReport(lines: string[]): string {
  const closingBlock = '```'
  const errorMsg = `**Report exceeded GitHub limit of ${MAX_REPORT_LENGTH} bytes and has been trimmed**`
  const maxErrorMsgLength = closingBlock.length + errorMsg.length + 2
  const maxReportLength = MAX_REPORT_LENGTH - maxErrorMsgLength

  let reportLength = 0
  let codeBlock = false
  let endLineIndex = 0
  for (endLineIndex = 0; endLineIndex < lines.length; endLineIndex++) {
    const line = lines[endLineIndex]
    const lineLength = getByteLength(line)

    reportLength += lineLength + 1
    if (reportLength > maxReportLength) {
      break
    }

    if (line === '```') {
      codeBlock = !codeBlock
    }
  }

  const reportLines = lines.slice(0, endLineIndex)
  if (codeBlock) {
    reportLines.push('```')
  }
  reportLines.push(errorMsg)
  return reportLines.join('\n')
}

function applySort(results: TestRunResult[]): void {
  results.sort((a, b) => a.path.localeCompare(b.path))
  for (const res of results) {
    res.suites.sort((a, b) => a.name.localeCompare(b.name))
  }
}

function getByteLength(text: string): number {
  return Buffer.byteLength(text, 'utf8')
}

function renderReport(results: TestRunResult[], options: ReportOptions): string[] {
  const sections: string[] = []
  const badge = getReportBadge(results)
  sections.push(badge)

  const runs = getTestRunsReport(results, options)
  sections.push(...runs)

  return sections
}

function getReportBadge(results: TestRunResult[]): string {
  const passed = results.reduce((sum, tr) => sum + tr.passed, 0)
  const skipped = results.reduce((sum, tr) => sum + tr.skipped, 0)
  const failed = results.reduce((sum, tr) => sum + tr.failed, 0)
  return getBadge(passed, failed, skipped)
}

function getBadge(passed: number, failed: number, skipped: number): string {
  const text = []
  if (passed > 0) {
    text.push(`${passed} passed`)
  }
  if (failed > 0) {
    text.push(`${failed} failed`)
  }
  if (skipped > 0) {
    text.push(`${skipped} skipped`)
  }
  const message = text.length > 0 ? text.join(', ') : 'none'

  let color = 'success'
  if (failed > 0) {
    color = 'critical'
  } else if (passed === 0 && failed === 0) {
    color = 'yellow'
  }
  const hint = failed > 0 ? 'Tests failed' : 'Tests passed successfully'
  const uri = encodeURIComponent(`tests-${message}-${color}`)
  return `![${hint}](https://img.shields.io/badge/${uri})`
}

function getTestRunsReport(testRuns: TestRunResult[], options: ReportOptions): string[] {
  const sections: string[] = []

  if (testRuns.length > 1 || options.onlySummary) {
    const tableData = testRuns.map((tr, runIndex) => {
      const time = formatTime(tr.time)
      const name = tr.path
      const addr = options.baseUrl + makeRunSlug(runIndex).link
      const nameLink = link(name, addr)
      const passed = tr.passed > 0 ? `${tr.passed}${Icon.success}` : ''
      const failed = tr.failed > 0 ? `${tr.failed}${Icon.fail}` : ''
      const skipped = tr.skipped > 0 ? `${tr.skipped}${Icon.skip}` : ''
      return [nameLink, passed, failed, skipped, time]
    })

    const resultsTable = table(
      ['Report', 'Passed', 'Failed', 'Skipped', 'Time'],
      [Align.Left, Align.Right, Align.Right, Align.Right, Align.Right],
      ...tableData
    )
    sections.push(resultsTable)
  }

  if (options.onlySummary === false) {
    const suitesReports = testRuns.map((tr, i) => getSuitesReport(tr, i, options)).flat()
    sections.push(...suitesReports)
  }
  return sections
}

function getSuitesReport(tr: TestRunResult, runIndex: number, options: ReportOptions): string[] {
  const sections: string[] = []

  const trSlug = makeRunSlug(runIndex)
  const nameLink = `<a id="${trSlug.id}" href="${options.baseUrl + trSlug.link}">${tr.path}</a>`
  const icon = getResultIcon(tr.result)
  sections.push(`## ${icon}\xa0${nameLink}`)

  const time = formatTime(tr.time)
  const headingLine2 =
    tr.tests > 0
      ? `**${tr.tests}** tests were completed in **${time}** with **${tr.passed}** passed, **${tr.failed}** failed and **${tr.skipped}** skipped.`
      : 'No tests found'
  sections.push(headingLine2)

  const suites = options.listSuites === 'failed' ? tr.failedSuites : tr.suites
  if (suites.length > 0) {
    const suitesTable = table(
      ['Test suite', 'Passed', 'Failed', 'Skipped', 'Time'],
      [Align.Left, Align.Right, Align.Right, Align.Right, Align.Right],
      ...suites.map((s, suiteIndex) => {
        const tsTime = formatTime(s.time)
        const tsName = s.name
        const skipLink = options.listTests === 'none' || (options.listTests === 'failed' && s.result !== 'failed')
        const tsAddr = options.baseUrl + makeSuiteSlug(runIndex, suiteIndex).link
        const tsNameLink = skipLink ? tsName : link(tsName, tsAddr)
        const passed = s.passed > 0 ? `${s.passed}${Icon.success}` : ''
        const failed = s.failed > 0 ? `${s.failed}${Icon.fail}` : ''
        const skipped = s.skipped > 0 ? `${s.skipped}${Icon.skip}` : ''
        return [tsNameLink, passed, failed, skipped, tsTime]
      })
    )
    sections.push(suitesTable)
  }

  if (options.listTests !== 'none') {
    const tests = suites.map((ts, suiteIndex) => getTestsReport(ts, runIndex, suiteIndex, options)).flat()

    if (tests.length > 1) {
      sections.push(...tests)
    }
  }

  return sections
}

function getTestsReport(ts: TestSuiteResult, runIndex: number, suiteIndex: number, options: ReportOptions): string[] {
  if (options.listTests === 'failed' && ts.result !== 'failed') {
    return []
  }
  const groups = ts.groups
  if (groups.length === 0) {
    return []
  }

  const sections: string[] = []

  const tsName = ts.name
  const tsSlug = makeSuiteSlug(runIndex, suiteIndex)
  const tsNameLink = `<a id="${tsSlug.id}" href="${options.baseUrl + tsSlug.link}">${tsName}</a>`
  const icon = getResultIcon(ts.result)
  sections.push(`### ${icon}\xa0${tsNameLink}`)

  sections.push('```')
  for (const grp of groups) {
    if (grp.name) {
      sections.push(grp.name)
    }
    const space = grp.name ? '  ' : ''
    for (const tc of grp.tests) {
      const result = getResultIcon(tc.result)
      sections.push(`${space}${result} ${tc.name}`)
      if (tc.error) {
        const lines = (tc.error.message ?? getFirstNonEmptyLine(tc.error.details)?.trim())
          ?.split(/\r?\n/g)
          .map(l => '\t' + l)
        if (lines) {
          sections.push(...lines)
        }
      }
    }
  }
  sections.push('```')

  return sections
}

function makeRunSlug(runIndex: number): {id: string; link: string} {
  // use prefix to avoid slug conflicts after escaping the paths
  return slug(`r${runIndex}`)
}

function makeSuiteSlug(runIndex: number, suiteIndex: number): {id: string; link: string} {
  // use prefix to avoid slug conflicts after escaping the paths
  return slug(`r${runIndex}s${suiteIndex}`)
}

function getResultIcon(result: TestExecutionResult): string {
  switch (result) {
    case 'success':
      return Icon.success
    case 'skipped':
      return Icon.skip
    case 'failed':
      return Icon.fail
    default:
      return ''
  }
}
