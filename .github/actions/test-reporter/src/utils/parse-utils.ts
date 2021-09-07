export function parseNetDuration(str: string): number {
  const durationRe = /^(\d\d):(\d\d):(\d\d(?:\.\d+)?)$/
  const durationMatch = str.match(durationRe)
  if (durationMatch === null) {
    throw new Error(`Invalid format: "${str}" is not NET duration`)
  }

  const [_, hourStr, minStr, secStr] = durationMatch
  return (parseInt(hourStr) * 3600 + parseInt(minStr) * 60 + parseFloat(secStr)) * 1000
}

export function parseIsoDate(str: string): Date {
  const isoDateRe = /^\d{4}-[01]\d-[0-3]\dT[0-2]\d:[0-5]\d:[0-5]\d\.\d+([+-][0-2]\d:[0-5]\d|Z)$/
  if (str === undefined || !isoDateRe.test(str)) {
    throw new Error(`Invalid format: "${str}" is not ISO date`)
  }

  return new Date(str)
}

export function getFirstNonEmptyLine(stackTrace: string): string | undefined {
  const lines = stackTrace.split(/\r?\n/g)
  return lines.find(str => !/^\s*$/.test(str))
}
