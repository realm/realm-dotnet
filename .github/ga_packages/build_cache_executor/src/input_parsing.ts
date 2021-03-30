/** @internal */
export function parse_paths(paths: string): string[] {
    // filter avoids empty elements
    return paths.split("\n").filter((i) => i);
}
