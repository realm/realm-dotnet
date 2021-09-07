export interface MochaJson {
  stats: MochaJsonStats
  passes: MochaJsonTest[]
  pending: MochaJsonTest[]
  failures: MochaJsonTest[]
}

export interface MochaJsonStats {
  duration: number
}

export interface MochaJsonTest {
  title: string
  fullTitle: string
  file: string
  duration?: number
  err: MochaJsonTestError
}

export interface MochaJsonTestError {
  stack?: string
  message?: string
}
