import type { Page } from '@playwright/test';
import { enderecoDoAplicativo, expect, test } from './janela';

async function abrirAssistente(janela: Page) {
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  await janela.getByRole('button', { name: 'Nova conta', exact: true }).first().click();
  const assistente = janela.getByRole('dialog', { name: 'Nova conta' });
  await assistente.getByRole('radio', { name: 'Não é mantida em banco, corretora ou outra instituição' }).check();
  await assistente.getByRole('button', { name: 'Avançar' }).click();
  return assistente;
}

test('criar conta em passos', async ({ janela }) => {
  const assistente = await abrirAssistente(janela);

  // O rádio do tipo é escondido; o usuário clica no rótulo, com o selo e o nome.
  await assistente.locator('label').filter({ hasText: 'Dinheiro' }).click();
  await expect(assistente.getByRole('radio', { name: 'Dinheiro' })).toBeChecked();
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await assistente.getByLabel('Nome').fill('Carteira');
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  // Dinheiro não pede número: do nome vai direto ao saldo, e não tem saldo mínimo.
  await expect(assistente.getByText('Passo 4 de 4')).toBeVisible();
  await expect(assistente.getByLabel('Saldo mínimo')).toHaveCount(0);
  await assistente.getByLabel('Saldo inicial').fill('50,00');
  await assistente.getByRole('button', { name: 'Concluir' }).click();

  await expect(assistente).toBeHidden();
  await expect(janela.getByText('"Carteira" foi criada.')).toBeVisible();
  await expect(janela.getByRole('main').getByRole('listitem').filter({ hasText: 'Carteira' })).toContainText('R$ 50,00');
});

test('criar conta Corrente com número e saldo mínimo', async ({ janela }) => {
  const assistente = await abrirAssistente(janela);

  await assistente.locator('label').filter({ hasText: 'Conta corrente' }).click();
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await assistente.getByLabel('Nome').fill('Conta principal');
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await expect(assistente.getByText('Passo 4 de 5')).toBeVisible();
  await assistente.getByLabel('Número da conta').fill('1234-5');
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await assistente.getByLabel('Saldo inicial').fill('100,00');
  await assistente.getByLabel('Saldo mínimo').fill('-500,00');
  await assistente.getByRole('button', { name: 'Concluir' }).click();

  await expect(assistente).toBeHidden();
  await janela.getByRole('main').getByRole('listitem').filter({ hasText: 'Conta principal' }).dblclick();
  const detalhes = janela.getByRole('form', { name: 'Detalhes da conta' });
  await expect(detalhes.getByLabel('Número da conta')).toHaveValue('1234-5');
  await expect(detalhes.getByLabel('Saldo mínimo')).toHaveValue('-500,00');
});

test('nome repetido barra o passo do nome', async ({ janela }) => {
  // A conta que já existe nasce pela API: o assunto é a conferência no Avançar.
  const resposta = await fetch(`${enderecoDoAplicativo(janela)}/api/contas`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ tipo: 'Corrente', nome: 'Conta principal', saldoInformado: 0 }),
  });
  expect(resposta.ok).toBe(true);

  const assistente = await abrirAssistente(janela);
  await assistente.getByRole('button', { name: 'Avançar' }).click();
  await assistente.getByLabel('Nome').fill('conta principal');
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await expect(assistente.getByRole('alert')).toContainText("Já existe uma conta com o nome 'conta principal'.");
  await expect(assistente.getByText('Passo 3 de 5')).toBeVisible();
});
