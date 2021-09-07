export enum Align {
  Left = ':---',
  Center = ':---:',
  Right = '---:',
  None = '---'
}

export const Icon = {
  skip: '✖️', // ':heavy_multiplication_x:'
  success: '✔️', // ':heavy_check_mark:'
  fail: '❌' // ':x:'
}

export function link(title: string, address: string): string {
  return `[${title}](${address})`
}

type ToString = string | number | boolean | Date
export function table(headers: ToString[], align: ToString[], ...rows: ToString[][]): string {
  const headerRow = `|${headers.map(tableEscape).join('|')}|`
  const alignRow = `|${align.join('|')}|`
  const contentRows = rows.map(row => `|${row.map(tableEscape).join('|')}|`).join('\n')
  return [headerRow, alignRow, contentRows].join('\n')
}

export function tableEscape(content: ToString): string {
  return content.toString().replace('|', '\\|')
}

export function fixEol(text?: string): string {
  return text?.replace(/\r/g, '') ?? ''
}

export function ellipsis(text: string, maxLength: number): string {
  if (text.length <= maxLength) {
    return text
  }

  return text.substr(0, maxLength - 3) + '...'
}

export function formatTime(ms: number): string {
  if (ms > 1000) {
    return `${Math.round(ms / 1000)}s`
  }

  return `${Math.round(ms)}ms`
}
