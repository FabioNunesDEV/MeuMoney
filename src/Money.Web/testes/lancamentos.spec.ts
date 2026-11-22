import type { Page } from '@playwright/test';
import { enderecoDoAplicativo, expect, test } from './janela';

// As contas nascem pela API: o assistente de criação já tem teste próprio.
async function criarContaDinheiro(janela: Page, nome: string, saldo: number) {
  const resposta = await fetch(`${enderecoDoAplicativo(janela)}/api/contas`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ tipo: 'Dinheiro', nome, saldoInformado: saldo }),
  });
  expect(resposta.ok).toBe(true);
}

async function abrirConta(janela: Page, nome: string) {
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  await janela.getByRole('main').getByRole('listitem').filter({ hasText: nome }).dblclick();
  await expect(janela.getByRole('heading', { name: nome })).toBeVisible();
}

async function lancar(janela: Page, aba: 'Gasto' | 'Recebimento', favorecido: string, categoria: string, montante: string) {
  await janela.getByRole('tab', { name: aba }).click();
  await janela.getByRole('button', { name: 'Novo', exact: true }).click();
  await janela.getByLabel('Pagar a').fill(favorecido);
  await janela.getByLabel('Categoria', { exact: true }).fill(categoria);
  await janela.getByLabel('Montante').fill(montante);
  await janela.getByRole('button', { name: 'Inserir' }).click();
  await expect(janela.getByRole('button', { name: 'Novo', exact: true })).toBeEnabled();
}

const rodape = (janela: Page) => janela.locator('footer');

test('duas contas Dinheiro, lançamentos nas duas e transferência entre elas', async ({ janela }) => {
  await criarContaDinheiro(janela, 'Carteira', 100);
  await criarContaDinheiro(janela, 'Cofre', 0);

  await abrirConta(janela, 'Carteira');
  await lancar(janela, 'Gasto', 'Padaria', 'Alimentação', '30,00');
  await lancar(janela, 'Recebimento', 'Salário', 'Renda', '50,00');
  await expect(rodape(janela)).toContainText('R$ 120,00');

  await janela.getByRole('tab', { name: 'Transferência' }).click();
  await janela.getByRole('button', { name: 'Novo', exact: true }).click();
  await expect(janela.getByLabel('Pagar a')).toHaveValue('Transferência');
  await janela.getByLabel('Para', { exact: true }).selectOption({ label: 'Cofre' });
  await janela.getByLabel('Montante').fill('40,00');
  await janela.getByRole('button', { name: 'Inserir' }).click();

  const grade = janela.getByRole('table');
  await expect(grade.getByRole('row')).toHaveCount(4);
  await expect(grade.getByRole('row').nth(3)).toContainText('Para: Cofre');
  await expect(rodape(janela)).toContainText('R$ 80,00');

  await abrirConta(janela, 'Cofre');
  await expect(grade.getByRole('row').nth(1)).toContainText('De: Carteira');
  await lancar(janela, 'Recebimento', 'Venda', 'Renda', '10,00');
  await expect(rodape(janela)).toContainText('R$ 50,00');

  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  const contas = janela.getByRole('main').getByRole('listitem');
  await expect(contas.filter({ hasText: 'Carteira' })).toContainText('R$ 80,00');
  await expect(contas.filter({ hasText: 'Cofre' })).toContainText('R$ 50,00');
  await expect(rodape(janela)).toContainText('R$ 130,00');
});

test('saldo negativo pede confirmação: Cancelar não grava, Confirmar grava', async ({ janela }) => {
  await criarContaDinheiro(janela, 'Carteira', 20);
  await abrirConta(janela, 'Carteira');

  await janela.getByRole('button', { name: 'Novo', exact: true }).click();
  await janela.getByLabel('Pagar a').fill('Mercado');
  await janela.getByLabel('Categoria', { exact: true }).fill('Alimentação');
  await janela.getByLabel('Montante').fill('50,00');
  await janela.getByRole('button', { name: 'Inserir' }).click();

  const aviso = janela.getByRole('alertdialog', { name: 'Saldo negativo' });
  await expect(aviso).toContainText('"Carteira" ficará em (R$ 30,00)');
  await aviso.getByRole('button', { name: 'Cancelar' }).click();
  await expect(aviso).toBeHidden();
  await expect(janela.getByLabel('Montante')).toHaveValue('50,00');
  await expect(janela.getByRole('table').getByRole('row')).toHaveCount(1);
  await expect(rodape(janela)).toContainText('R$ 20,00');

  await janela.getByRole('button', { name: 'Inserir' }).click();
  await aviso.getByRole('button', { name: 'Confirmar' }).click();
  await expect(janela.getByRole('table').getByRole('row')).toHaveCount(2);
  await expect(rodape(janela)).toContainText('(R$ 30,00)');
});
