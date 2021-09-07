import * as core from '@actions/core'
import exec from './exec'

export async function listFiles(): Promise<string[]> {
  core.startGroup('Listing all files tracked by git')
  let output = ''
  try {
    output = (await exec('git', ['ls-files', '-z'])).stdout
  } finally {
    fixStdOutNullTermination()
    core.endGroup()
  }

  return output.split('\u0000').filter(s => s.length > 0)
}

function fixStdOutNullTermination(): void {
  // Previous command uses NULL as delimiters and output is printed to stdout.
  // We have to make sure next thing written to stdout will start on new line.
  // Otherwise things like ::set-output wouldn't work.
  core.info('')
}
