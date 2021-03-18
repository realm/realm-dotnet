import * as utils from "../src/utils/common";

export class outputStream implements utils.outputStream {
  debug(message: string): void {
    console.debug(message);
  }
  info(message: string): void {
    console.info(message);
  }
  warning(message: string): void {
    console.warn(message);
  }
  error(message: string): void {
    console.error(message);
  }
}
