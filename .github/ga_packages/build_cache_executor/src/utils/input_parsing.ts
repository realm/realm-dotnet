/** @internal */
export function parsePaths(str: string): string[] {
  return str.split("\n").filter((x) => x);
}

/** @internal */
export function parseCmds(str: string): string[] {
  return str.split("\n").filter((x) => x);
}
