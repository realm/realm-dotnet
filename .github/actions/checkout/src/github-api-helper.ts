import * as assert from 'assert'
import * as core from '@actions/core'
import * as fs from 'fs'
import * as github from '@actions/github'
import * as io from '@actions/io'
import * as path from 'path'
import * as retryHelper from './retry-helper'
import * as toolCache from '@actions/tool-cache'
import {default as uuid} from 'uuid/v4'
import {Octokit} from '@octokit/rest'

const IS_WINDOWS = process.platform === 'win32'

export async function downloadRepository(
  authToken: string,
  owner: string,
  repo: string,
  ref: string,
  commit: string,
  repositoryPath: string
): Promise<void> {
  // Determine the default branch
  if (!ref && !commit) {
    core.info('Determining the default branch')
    ref = await getDefaultBranch(authToken, owner, repo)
  }

  // Download the archive
  let archiveData = await retryHelper.execute(async () => {
    core.info('Downloading the archive')
    return await downloadArchive(authToken, owner, repo, ref, commit)
  })

  // Write archive to disk
  core.info('Writing archive to disk')
  const uniqueId = uuid()
  const archivePath = path.join(repositoryPath, `${uniqueId}.tar.gz`)
  await fs.promises.writeFile(archivePath, archiveData)
  archiveData = Buffer.from('') // Free memory

  // Extract archive
  core.info('Extracting the archive')
  const extractPath = path.join(repositoryPath, uniqueId)
  await io.mkdirP(extractPath)
  if (IS_WINDOWS) {
    await toolCache.extractZip(archivePath, extractPath)
  } else {
    await toolCache.extractTar(archivePath, extractPath)
  }
  await io.rmRF(archivePath)

  // Determine the path of the repository content. The archive contains
  // a top-level folder and the repository content is inside.
  const archiveFileNames = await fs.promises.readdir(extractPath)
  assert.ok(
    archiveFileNames.length == 1,
    'Expected exactly one directory inside archive'
  )
  const archiveVersion = archiveFileNames[0] // The top-level folder name includes the short SHA
  core.info(`Resolved version ${archiveVersion}`)
  const tempRepositoryPath = path.join(extractPath, archiveVersion)

  // Move the files
  for (const fileName of await fs.promises.readdir(tempRepositoryPath)) {
    const sourcePath = path.join(tempRepositoryPath, fileName)
    const targetPath = path.join(repositoryPath, fileName)
    if (IS_WINDOWS) {
      await io.cp(sourcePath, targetPath, {recursive: true}) // Copy on Windows (Windows Defender may have a lock)
    } else {
      await io.mv(sourcePath, targetPath)
    }
  }
  await io.rmRF(extractPath)
}

/**
 * Looks up the default branch name
 */
export async function getDefaultBranch(
  authToken: string,
  owner: string,
  repo: string
): Promise<string> {
  return await retryHelper.execute(async () => {
    core.info('Retrieving the default branch name')
    const octokit = new github.GitHub(authToken)
    let result: string
    try {
      // Get the default branch from the repo info
      const response = await octokit.repos.get({owner, repo})
      result = response.data.default_branch
      assert.ok(result, 'default_branch cannot be empty')
    } catch (err) {
      // Handle .wiki repo
      if (err['status'] === 404 && repo.toUpperCase().endsWith('.WIKI')) {
        result = 'master'
      }
      // Otherwise error
      else {
        throw err
      }
    }

    // Print the default branch
    core.info(`Default branch '${result}'`)

    // Prefix with 'refs/heads'
    if (!result.startsWith('refs/')) {
      result = `refs/heads/${result}`
    }

    return result
  })
}

async function downloadArchive(
  authToken: string,
  owner: string,
  repo: string,
  ref: string,
  commit: string
): Promise<Buffer> {
  const octokit = new github.GitHub(authToken)
  const params: Octokit.ReposGetArchiveLinkParams = {
    owner: owner,
    repo: repo,
    archive_format: IS_WINDOWS ? 'zipball' : 'tarball',
    ref: commit || ref
  }
  const response = await octokit.repos.getArchiveLink(params)
  if (response.status != 200) {
    throw new Error(
      `Unexpected response from GitHub API. Status: ${response.status}, Data: ${response.data}`
    )
  }

  return Buffer.from(response.data) // response.data is ArrayBuffer
}
