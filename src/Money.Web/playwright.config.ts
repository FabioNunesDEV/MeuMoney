import { defineConfig } from '@playwright/test';

/**
 * Testes de tela pela janela real do MeuMoney: cada teste abre o aplicativo compilado, com banco
 * temporário, e se liga ao WebView2 da janela (ver testes/janela.ts). Uma janela por vez.
 */
export default defineConfig({
  testDir: 'testes',
  workers: 1,
  fullyParallel: false,
  retries: 0,
  timeout: 60_000,
  reporter: 'list',
});
