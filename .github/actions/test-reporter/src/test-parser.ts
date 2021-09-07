import {TestRunResult} from './test-results'

export interface ParseOptions {
  parseErrors: boolean
  workDir?: string
  trackedFiles: string[]
}

export interface TestParser {
  parse(path: string, content: string): Promise<TestRunResult>
}
