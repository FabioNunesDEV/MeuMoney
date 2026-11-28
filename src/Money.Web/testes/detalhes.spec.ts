import type { Page } from '@playwright/test';
import { enderecoDoAplicativo, expect, test } from './janela';

// As contas nascem pela API: o assunto aqui é a edição nos Detalhes.
async function criarConta(janela: Page, dados: Record<string, unknown>) {
  const resposta = await fetch(`${enderecoDoAplicativo(janela)}/api/contas`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(dados),
  });
  expect(resposta.ok).toBe(true);
}

async function abrirConta(janela: Page, nome: string) {
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  await janela.getByRole('main').getByRole('listitem').filter({ hasText: nome }).dblclick();
  await expect(janela.getByRole('heading', { name: nome })).toBeVisible();
}

const detalhes = (janela: Page) => janela.getByRole('form', { name: 'Detalhes da conta' });

test('Editar leva aos Detalhes, que gravam os campos complementares', async ({ janela }) => {
  await criarConta(janela, { tipo: 'Corrente', nome: 'Banco', saldoInformado: 100 });
  await abrirConta(janela, 'Banco');

  await janela.getByRole('toolbar').first().getByRole('button', { name: 'Editar' }).click();
  const form = detalhes(janela);
  await expect(form.getByLabel('Nome', { exact: true })).toBeFocused();
  await expect(form.getByRole('button', { name: 'Salvar' })).toBeDisabled();

  await form.getByLabel('Número da conta').fill('1234-5');
  await form.getByLabel('Abreviação').fill('Bco');
  await form.getByLabel('Saldo mínimo').fill('50,00');
  await form.getByLabel('Comentário').fill('Uso diário');
  await form.getByRole('button', { name: 'Salvar' }).click();

  await expect(janela.getByText('"Banco" foi alterada.')).toBeVisible();
  await expect(form.getByRole('button', { name: 'Salvar' })).toBeDisabled();

  // Reabrir a conta lê de novo o que foi gravado.
  await abrirConta(janela, 'Banco');
  await expect(form.getByLabel('Número da conta')).toHaveValue('1234-5');
  await expect(form.getByLabel('Abreviação')).toHaveValue('Bco');
  await expect(form.getByLabel('Saldo mínimo')).toHaveValue('50,00');
  await expect(form.getByLabel('Comentário')).toHaveValue('Uso diário');
});

test('Detalhes da conta Dinheiro não têm saldo mínimo', async ({ janela }) => {
  await criarConta(janela, { tipo: 'Dinheiro', nome: 'Carteira', saldoInformado: 100 });
  await abrirConta(janela, 'Carteira');

  await janela.getByRole('button', { name: 'Detalhes', exact: true }).click();
  const form = detalhes(janela);
  await expect(form.getByLabel('Número da conta')).toBeVisible();
  await expect(form.getByLabel('Saldo mínimo')).toHaveCount(0);
});

test('Detalhes mostram a recusa no formulário e Desfazer volta ao gravado', async ({ janela }) => {
  await criarConta(janela, { tipo: 'Corrente', nome: 'Banco', saldoInformado: 0, comentario: 'Original' });
  await abrirConta(janela, 'Banco');
  const form = detalhes(janela);

  await form.getByLabel('Nome', { exact: true }).fill('');
  await form.getByRole('button', { name: 'Salvar' }).click();
  await expect(form.getByRole('alert')).toContainText('O nome é obrigatório.');
  await expect(form.getByLabel('Comentário')).toHaveValue('Original');

  await form.getByRole('button', { name: 'Desfazer' }).click();
  await expect(form.getByLabel('Nome', { exact: true })).toHaveValue('Banco');
  await expect(form.getByRole('alert')).toHaveCount(0);

  await form.getByLabel('Comentário').fill('Outro');
  await expect(form.getByRole('button', { name: 'Salvar' })).toBeEnabled();
  await form.getByRole('button', { name: 'Desfazer' }).click();
  await expect(form.getByLabel('Comentário')).toHaveValue('Original');
  await expect(form.getByRole('button', { name: 'Salvar' })).toBeDisabled();
  await expect(form.getByRole('button', { name: 'Desfazer' })).toBeDisabled();
});

test('Editar no gerenciador abre o pop-up com os campos complementares', async ({ janela }) => {
  await criarConta(janela, { tipo: 'CartaoCredito', nome: 'Cartão', saldoInformado: 0, abreviacao: 'Cc' });
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();

  await janela.getByRole('button', { name: 'Mais ações de Cartão' }).click();
  await janela.getByRole('menuitem', { name: 'Editar' }).click();

  const popup = janela.getByRole('dialog', { name: 'Editar conta' });
  await expect(popup.getByLabel('Abreviação')).toHaveValue('Cc');
  await expect(popup.getByLabel('Número da conta')).toBeVisible();
  // Cartão de crédito não tem saldo mínimo.
  await expect(popup.getByLabel('Saldo mínimo')).toHaveCount(0);
});
