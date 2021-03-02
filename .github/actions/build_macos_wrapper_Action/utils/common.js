"use strict";
// export interface output {
//     debug(text: string): void;
//     info(text: string): void;
//     warning(text: string): void;
//     error(text: string): void;
// }
Object.defineProperty(exports, "__esModule", { value: true });
exports.execShellCommand = void 0;
function execShellCommand(cmd) {
    const exec = require('child_process').exec;
    return new Promise((resolve, reject) => {
        exec(cmd, (error, stdout, stderr) => {
            if (error) {
                throw new Error(error);
            }
            resolve([stdout, stderr]);
        });
    });
}
exports.execShellCommand = execShellCommand;
