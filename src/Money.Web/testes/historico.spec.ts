import type { Page } from '@playwright/test';
import { enderecoDoAplicativo, expect, test } from './janela';

async function postar<T>(janela: Page, caminho: string, corpo: unknown): Promise<T> {
  const resposta = await fetch(`${enderecoDoAplicativo(janela)}/api${caminho}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(corpo),
  });
  expect(resposta.ok).toBe(true);
  return (await resposta.json()) as T;
}

// Data local, sem hora, a tantos dias de hoje: yyyy-MM-dd para a API e dd/MM/yyyy para a tela.
function dia(deslocamento: number) {
  const data = new Date();
  data.setDate(data.getDate() + deslocamento);
  const iso = `${data.getFullYear()}-${String(data.getMonth() + 1).padStart(2, '0')}-${String(data.getDate()).padStart(2, '0')}`;
  return { iso, texto: iso.split('-').reverse().join('/') };
}

async function abrirHistorico(janela: Page, nome: string) {
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  await janela.getByRole('main').getByRole('listitem').filter({ hasText: nome }).dblclick();
  await janela.getByRole('button', { name: 'Histórico' }).click();
}

test('histórico mostra o resumo, o gráfico e troca de período', async ({ janela }) => {
  const conta = await postar<{ id: string }>(janela, '/contas', { tipo: 'Dinheiro', nome: 'Carteira', saldoInformado: 100 });
  const base = { contaDestinoId: null, nomeSubcategoria: null, memo: null, confirmarSaldoNegativo: true, contaId: conta.id };
  const antigo = dia(-40);
  await postar(janela, '/lancamentos', { ...base, natureza: 'Recebimento', data: antigo.iso, valor: 50, nomeFavorecido: 'Salário', nomeCategoria: 'Renda' });
  await postar(janela, '/lancamentos', { ...base, natureza: 'Gasto', data: dia(0).iso, valor: 80, nomeFavorecido: 'Mercado', nomeCategoria: 'Alimentação' });

  await abrirHistorico(janela, 'Carteira');

  await expect(janela.getByRole('heading', { name: 'Movimentação do saldo' })).toBeVisible();
  await expect(janela.getByText('Início do período').locator('..')).toContainText('R$ 100,00');
  await expect(janela.getByText('Maior saldo').locator('..')).toContainText('R$ 150,00');
  await expect(janela.getByText('Saldo final').locator('..')).toContainText('R$ 70,00');
  await expect(janela.locator('.recharts-line-curve')).toHaveCount(1);

  await janela.getByLabel('Período', { exact: true }).selectOption({ label: 'Desde o primeiro lançamento' });
  await expect(janela.getByText(`${antigo.texto} até`)).toBeVisible();
  await expect(janela.getByText('Início do período').locator('..')).toContainText('R$ 100,00');
});

test('conta sem lançamento mostra o estado vazio e leva à Movimentação', async ({ janela }) => {
  await postar(janela, '/contas', { tipo: 'Dinheiro', nome: 'Cofre', saldoInformado: 0 });

  await abrirHistorico(janela, 'Cofre');
  await expect(janela.getByText('Ainda não há histórico')).toBeVisible();
  await janela.getByRole('button', { name: 'Ir para a Movimentação' }).click();

  await expect(janela.getByRole('tab', { name: 'Gasto' })).toBeVisible();
});
