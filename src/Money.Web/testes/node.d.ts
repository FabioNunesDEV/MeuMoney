// Tipos mínimos do Node usados por testes/janela.ts. O pacote @types/node não está entre os aprovados;
// se entrar, este arquivo sai.

declare module 'node:child_process' {
  interface Processo {
    readonly pid?: number;
    readonly exitCode: number | null;
  }
  export function spawn(comando: string, argumentos: string[], opcoes: { env: Record<string, string | undefined>; stdio: 'ignore' }): Processo;
  export function execFileSync(comando: string, argumentos: string[], opcoes: { stdio: 'ignore' }): unknown;
}

declare module 'node:net' {
  interface Servidor {
    once(evento: 'error', aoFalhar: (erro: Error) => void): Servidor;
    listen(porta: number, endereco: string, aoOuvir: () => void): Servidor;
    address(): { port: number } | string | null;
    close(aoFechar: () => void): Servidor;
  }
  export function createServer(): Servidor;
}

declare module 'node:fs' {
  export function existsSync(caminho: string): boolean;
  export function mkdirSync(caminho: string, opcoes: { recursive: true }): unknown;
  export function rmSync(caminho: string, opcoes: { recursive: true; force: true }): void;
}

declare module 'node:path' {
  export function resolve(...partes: string[]): string;
}

declare const process: { env: Record<string, string | undefined> };

interface ImportMeta {
  readonly dirname: string;
}
