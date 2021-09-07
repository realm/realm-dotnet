import * as fs from 'fs'
import * as path from 'path'

import {JestJunitParser} from '../src/parsers/jest-junit/jest-junit-parser'
import {ParseOptions} from '../src/test-parser'
import {getReport} from '../src/report/get-report'
import {normalizeFilePath} from '../src/utils/path-utils'

describe('jest-junit tests', () => {
  it('produces empty test run result when there are no test cases', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'empty', 'jest-junit.xml')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      parseErrors: true,
      trackedFiles: []
    }

    const parser = new JestJunitParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result.tests).toBe(0)
    expect(result.result).toBe('success')
  })

  it('report from ./reports/jest test results matches snapshot', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'jest-junit.xml')
    const outputPath = path.join(__dirname, '__outputs__', 'jest-junit.md')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      parseErrors: true,
      trackedFiles: ['__tests__/main.test.js', '__tests__/second.test.js', 'lib/main.js']
      //workDir: 'C:/Users/Michal/Workspace/dorny/test-check/reports/jest/'
    }

    const parser = new JestJunitParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result).toMatchSnapshot()

    const report = getReport([result])
    fs.mkdirSync(path.dirname(outputPath), {recursive: true})
    fs.writeFileSync(outputPath, report)
  })

  it('report from facebook/jest test results matches snapshot', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'external', 'jest', 'jest-test-results.xml')
    const trackedFilesPath = path.join(__dirname, 'fixtures', 'external', 'jest', 'files.txt')
    const outputPath = path.join(__dirname, '__outputs__', 'jest-test-results.md')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const trackedFiles = fs.readFileSync(trackedFilesPath, {encoding: 'utf8'}).split(/\n\r?/g)
    const opts: ParseOptions = {
      parseErrors: true,
      trackedFiles
      //workDir: '/home/dorny/dorny/jest/'
    }

    const parser = new JestJunitParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result).toMatchSnapshot()

    const report = getReport([result])
    fs.mkdirSync(path.dirname(outputPath), {recursive: true})
    fs.writeFileSync(outputPath, report)
  })
})
