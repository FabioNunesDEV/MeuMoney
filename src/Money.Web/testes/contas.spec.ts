import { expect, test } from './janela';

test('criar conta em passos', async ({ janela }) => {
  await janela.getByRole('navigation', { name: 'Módulos' }).getByRole('button', { name: 'Contas', exact: true }).click();
  await janela.getByRole('button', { name: 'Nova conta', exact: true }).first().click();

  const assistente = janela.getByRole('dialog', { name: 'Nova conta' });
  await assistente.getByRole('radio', { name: 'Não é mantida em banco, corretora ou outra instituição' }).check();
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  // O rádio do tipo é escondido; o usuário clica no rótulo, com o selo e o nome.
  await assistente.locator('label').filter({ hasText: 'Dinheiro' }).click();
  await expect(assistente.getByRole('radio', { name: 'Dinheiro' })).toBeChecked();
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await assistente.getByLabel('Nome').fill('Carteira');
  await assistente.getByRole('button', { name: 'Avançar' }).click();

  await assistente.getByLabel('Saldo inicial').fill('50,00');
  await assistente.getByRole('button', { name: 'Concluir' }).click();

  await expect(assistente).toBeHidden();
  await expect(janela.getByText('"Carteira" foi criada.')).toBeVisible();
  await expect(janela.getByRole('main').getByRole('listitem').filter({ hasText: 'Carteira' })).toContainText('R$ 50,00');
});
