// export interface output {
//     debug(text: string): void;
//     info(text: string): void;
//     warning(text: string): void;
//     error(text: string): void;
// }
import * as cp from 'child_process';

export function execShellCommand(cmd: string): Promise<[string, string]>
{
    return new Promise((resolve, reject) => {
        cp.exec(cmd, (error, stdout, stderr) => {
            if (error) {
                throw new Error(error.message);
            }
            resolve([stdout, stderr]);
        });
    });
}