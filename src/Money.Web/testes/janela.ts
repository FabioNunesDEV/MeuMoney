import { execFileSync, spawn } from 'node:child_process';
import { existsSync, mkdirSync, rmSync } from 'node:fs';
import { createServer } from 'node:net';
import { resolve } from 'node:path';
import { chromium, expect, test as base, type Browser, type Page } from '@playwright/test';

const executavel = resolve(import.meta.dirname, '../../Money.Desktop/bin/Debug/net10.0-windows/Money.Desktop.exe');

/**
 * `test` com a fixture `janela`: a página da janela real do MeuMoney. Cada teste abre o aplicativo
 * compilado com banco, log e dados do WebView2 numa pasta própria do teste, e o fecha no fim.
 */
export const test = base.extend<{ janela: Page }>({
  janela: async ({}, usar, info) => {
    if (!existsSync(executavel)) {
      throw new Error(`Aplicativo não encontrado em ${executavel}. Compile antes: dotnet build MeuMoney.slnx`);
    }

    const pasta = info.outputPath('aplicativo');
    mkdirSync(pasta, { recursive: true });
    const porta = await portaLivre();

    // O runtime do WebView2 lê a porta de depuração do ambiente: o aplicativo não precisa saber do teste.
    const aplicativo = spawn(executavel, [], {
      stdio: 'ignore',
      env: {
        ...process.env,
        Database__ConnectionString: `Data Source=${pasta}\\meumoney.db`,
        PastaDados: pasta,
        WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS: `--remote-debugging-port=${porta}`,
      },
    });

    let navegador: Browser | undefined;
    try {
      await esperarDepuracao(porta, () => aplicativo.exitCode);
      navegador = await chromium.connectOverCDP(`http://127.0.0.1:${porta}`);
      const pagina = await paginaDoAplicativo(navegador);
      await expect(pagina.getByRole('navigation', { name: 'Módulos' })).toBeVisible();

      await usar(pagina);

      if (info.status !== info.expectedStatus) {
        const captura = info.outputPath('janela.png');
        await pagina.screenshot({ path: captura });
        await info.attach('janela', { path: captura, contentType: 'image/png' });
      }
    } finally {
      await navegador?.close().catch(() => undefined);
      // /T leva junto os processos do WebView2, filhos do aplicativo.
      if (aplicativo.pid !== undefined) {
        try {
          execFileSync('taskkill', ['/PID', String(aplicativo.pid), '/T', '/F'], { stdio: 'ignore' });
        } catch {
          // O aplicativo já tinha fechado.
        }
      }
      await apagarPasta(pasta);
    }
  },
});

export { expect };

// Os processos do WebView2 demoram um pouco a soltar os arquivos depois de encerrados.
async function apagarPasta(pasta: string): Promise<void> {
  const limite = Date.now() + 15_000;
  for (;;) {
    try {
      rmSync(pasta, { recursive: true, force: true });
      return;
    } catch (erro) {
      if (Date.now() > limite) {
        throw erro;
      }
      await new Promise((aoPassar) => setTimeout(aoPassar, 250));
    }
  }
}

function portaLivre(): Promise<number> {
  return new Promise((aoObter, aoFalhar) => {
    const servidor = createServer();
    servidor.once('error', aoFalhar);
    servidor.listen(0, '127.0.0.1', () => {
      const endereco = servidor.address();
      const porta = typeof endereco === 'object' && endereco ? endereco.port : 0;
      servidor.close(() => aoObter(porta));
    });
  });
}

async function esperarDepuracao(porta: number, codigoSaida: () => number | null): Promise<void> {
  const limite = Date.now() + 30_000;
  while (Date.now() < limite) {
    const codigo = codigoSaida();
    if (codigo !== null) {
      throw new Error(`O aplicativo fechou ao abrir (código ${codigo}). Veja o log na pasta do teste.`);
    }
    try {
      if ((await fetch(`http://127.0.0.1:${porta}/json/version`)).ok) {
        return;
      }
    } catch {
      // O WebView2 ainda não abriu a porta.
    }
    await new Promise((aoPassar) => setTimeout(aoPassar, 250));
  }
  throw new Error('O WebView2 do aplicativo não abriu a porta de depuração em 30 s.');
}

// A janela navega para o servidor local logo depois de criar o WebView2: espera a página dele.
async function paginaDoAplicativo(navegador: Browser): Promise<Page> {
  const limite = Date.now() + 30_000;
  while (Date.now() < limite) {
    const pagina = navegador
      .contexts()
      .flatMap((contexto) => contexto.pages())
      .find((p) => p.url().startsWith('http://127.0.0.1:'));
    if (pagina) {
      return pagina;
    }
    await new Promise((aoPassar) => setTimeout(aoPassar, 250));
  }
  throw new Error('A janela não abriu as telas do aplicativo em 30 s.');
}
