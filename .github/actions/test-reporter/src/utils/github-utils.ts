import {createWriteStream} from 'fs'
import * as core from '@actions/core'
import * as github from '@actions/github'
import {GitHub} from '@actions/github/lib/utils'
import {EventPayloads} from '@octokit/webhooks'
import * as stream from 'stream'
import {promisify} from 'util'
import got from 'got'
const asyncStream = promisify(stream.pipeline)

export function getCheckRunContext(): {sha: string; runId: number} {
  if (github.context.eventName === 'workflow_run') {
    core.info('Action was triggered by workflow_run: using SHA and RUN_ID from triggering workflow')
    const event = github.context.payload as EventPayloads.WebhookPayloadWorkflowRun
    if (!event.workflow_run) {
      throw new Error("Event of type 'workflow_run' is missing 'workflow_run' field")
    }
    return {
      sha: event.workflow_run.head_commit.id,
      runId: event.workflow_run.id
    }
  }

  const runId = github.context.runId
  if (github.context.payload.pull_request) {
    core.info(`Action was triggered by ${github.context.eventName}: using SHA from head of source branch`)
    const pr = github.context.payload.pull_request as EventPayloads.WebhookPayloadPullRequestPullRequest
    return {sha: pr.head.sha, runId}
  }

  return {sha: github.context.sha, runId}
}

export async function downloadArtifact(
  octokit: InstanceType<typeof GitHub>,
  artifactId: number,
  fileName: string,
  token: string
): Promise<void> {
  core.startGroup(`Downloading artifact ${fileName}`)
  try {
    core.info(`Artifact ID: ${artifactId}`)

    const req = octokit.actions.downloadArtifact.endpoint({
      ...github.context.repo,
      artifact_id: artifactId,
      archive_format: 'zip'
    })

    const headers = {
      Authorization: `Bearer ${token}`
    }
    const resp = await got(req.url, {
      headers,
      followRedirect: false
    })

    core.info(`Fetch artifact URL: ${resp.statusCode} ${resp.statusMessage}`)
    if (resp.statusCode !== 302) {
      throw new Error('Fetch artifact URL failed: received unexpected status code')
    }

    const url = resp.headers.location
    if (url === undefined) {
      const receivedHeaders = Object.keys(resp.headers)
      core.info(`Received headers: ${receivedHeaders.join(', ')}`)
      throw new Error('Location header was not found in API response')
    }
    if (typeof url !== 'string') {
      throw new Error(`Location header has unexpected value: ${url}`)
    }

    const downloadStream = got.stream(url, {headers})
    const fileWriterStream = createWriteStream(fileName)

    core.info(`Downloading ${url}`)
    downloadStream.on('downloadProgress', ({transferred}) => {
      core.info(`Progress: ${transferred} B`)
    })
    await asyncStream(downloadStream, fileWriterStream)
  } finally {
    core.endGroup()
  }
}

export async function listFiles(octokit: InstanceType<typeof GitHub>, sha: string): Promise<string[]> {
  core.startGroup('Fetching list of tracked files from GitHub')
  try {
    const commit = await octokit.git.getCommit({
      commit_sha: sha,
      ...github.context.repo
    })
    const files = await listGitTree(octokit, commit.data.tree.sha, '')
    return files
  } finally {
    core.endGroup()
  }
}

async function listGitTree(octokit: InstanceType<typeof GitHub>, sha: string, path: string): Promise<string[]> {
  const pathLog = path ? ` at ${path}` : ''
  core.info(`Fetching tree ${sha}${pathLog}`)
  let truncated = false
  let tree = await octokit.git.getTree({
    recursive: 'true',
    tree_sha: sha,
    ...github.context.repo
  })

  if (tree.data.truncated) {
    truncated = true
    tree = await octokit.git.getTree({
      tree_sha: sha,
      ...github.context.repo
    })
  }

  const result: string[] = []
  for (const tr of tree.data.tree) {
    const file = `${path}${tr.path}`
    if (tr.type === 'blob') {
      result.push(file)
    } else if (tr.type === 'tree' && truncated) {
      const files = await listGitTree(octokit, tr.sha, `${file}/`)
      result.push(...files)
    }
  }

  return result
}
