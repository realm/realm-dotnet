export function normalizeDirPath(path: string, addTrailingSlash: boolean): string {
  if (!path) {
    return path
  }

  path = normalizeFilePath(path)
  if (addTrailingSlash && !path.endsWith('/')) {
    path += '/'
  }
  return path
}

export function normalizeFilePath(path: string): string {
  if (!path) {
    return path
  }

  return path.trim().replace(/\\/g, '/')
}

export function getBasePath(path: string, trackedFiles: string[]): string | undefined {
  if (trackedFiles.includes(path)) {
    return ''
  }

  let max = ''
  for (const file of trackedFiles) {
    if (path.endsWith(file) && file.length > max.length) {
      max = file
    }
  }

  if (max === '') {
    return undefined
  }

  const base = path.substr(0, path.length - max.length)
  return base
}
