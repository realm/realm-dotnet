import * as fs from 'fs'
import * as path from 'path'

import {DotnetTrxParser} from '../src/parsers/dotnet-trx/dotnet-trx-parser'
import {ParseOptions} from '../src/test-parser'
import {getReport} from '../src/report/get-report'
import {normalizeFilePath} from '../src/utils/path-utils'

describe('dotnet-trx tests', () => {
  it('produces empty test run result when there are no test cases', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'empty', 'dotnet-trx.trx')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      parseErrors: true,
      trackedFiles: []
    }

    const parser = new DotnetTrxParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result.tests).toBe(0)
    expect(result.result).toBe('success')
  })

  it('matches report snapshot', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'dotnet-trx.trx')
    const outputPath = path.join(__dirname, '__outputs__', 'dotnet-trx.md')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      parseErrors: true,
      trackedFiles: ['DotnetTests.Unit/Calculator.cs', 'DotnetTests.XUnitTests/CalculatorTests.cs']
      //workDir: 'C:/Users/Michal/Workspace/dorny/test-check/reports/dotnet/'
    }

    const parser = new DotnetTrxParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result).toMatchSnapshot()

    const report = getReport([result])
    fs.mkdirSync(path.dirname(outputPath), {recursive: true})
    fs.writeFileSync(outputPath, report)
  })

  it('report from FluentValidation test results matches snapshot', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'external', 'FluentValidation.Tests.trx')
    const outputPath = path.join(__dirname, '__outputs__', 'fluent-validation-test-results.md')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      trackedFiles: [],
      parseErrors: true
    }

    const parser = new DotnetTrxParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result).toMatchSnapshot()

    const report = getReport([result])
    fs.mkdirSync(path.dirname(outputPath), {recursive: true})
    fs.writeFileSync(outputPath, report)
  })

  it('report from SilentNotes test results matches snapshot', async () => {
    const fixturePath = path.join(__dirname, 'fixtures', 'external', 'SilentNotes.trx')
    const outputPath = path.join(__dirname, '__outputs__', 'silent-notes-test-results.md')
    const filePath = normalizeFilePath(path.relative(__dirname, fixturePath))
    const fileContent = fs.readFileSync(fixturePath, {encoding: 'utf8'})

    const opts: ParseOptions = {
      trackedFiles: [],
      parseErrors: true
    }

    const parser = new DotnetTrxParser(opts)
    const result = await parser.parse(filePath, fileContent)
    expect(result).toMatchSnapshot()

    const report = getReport([result])
    fs.mkdirSync(path.dirname(outputPath), {recursive: true})
    fs.writeFileSync(outputPath, report)
  })
})
