"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.parse_paths = void 0;
/** @internal */
function parse_paths(paths) {
    // filter avoids empty elements
    return paths.split("\n").filter((i) => i);
}
exports.parse_paths = parse_paths;
//# sourceMappingURL=input_parsing.js.map