import {normalizeFilePath} from './path-utils'

export function getExceptionSource(
  stackTrace: string,
  trackedFiles: string[],
  getRelativePath: (str: string) => string
): {path: string; line: number} | undefined {
  const lines = stackTrace.split(/\r?\n/)
  const re = /\((.*):(\d+):\d+\)$/

  for (const str of lines) {
    const match = str.match(re)
    if (match !== null) {
      const [_, fileStr, lineStr] = match
      const filePath = normalizeFilePath(fileStr)
      if (filePath.startsWith('internal/') || filePath.includes('/node_modules/')) {
        continue
      }
      const path = getRelativePath(filePath)
      if (!path) {
        continue
      }
      if (trackedFiles.includes(path)) {
        const line = parseInt(lineStr)

        return {path, line}
      }
    }
  }
}
