## Why

A conta Dinheiro já tem lançamentos (change B), mas não há como ver a evolução do saldo: a visão
Histórico da página da conta continua "Em breve". Este é o terceiro dos quatro changes do roteiro da
conta Dinheiro. O seguinte é o dos detalhes editáveis (D).

## What Changes

- **Visão Histórico da conta Dinheiro.** Gráfico "Movimentação do saldo", em degrau, como no Money 99.
  - O saldo fica constante entre lançamentos e muda no dia de cada um.
  - O eixo vertical é o saldo, na moeda da conta; o horizontal são as datas.
  - Lançamentos nulos não contam, como no saldo da Movimentação.
  - Sob o título, o período mostrado ("01/09/2026 até 22/11/2026").
- **Período**, numa lista junto ao título do gráfico: último mês, últimos 3 meses (padrão, como no print do Money),
  últimos 6 meses, últimos 12 meses e desde o primeiro lançamento.
  - O gráfico começa no saldo do primeiro dia do período e vai até hoje.
  - Se houver lançamento com data futura, o gráfico vai até ele.
- **Resumo acima do gráfico:** saldo no início do período, maior saldo, menor saldo e saldo final,
  no formato contábil (`ValorSaldo`). Passar o mouse sobre o gráfico mostra a data e o saldo do
  ponto.
- **Saldo negativo:** quando o gráfico cruza o zero, uma linha de referência marca o zero.
- **Conta sem lançamentos:** o Histórico mostra o estado vazio, com a orientação de registrar
  lançamentos na Movimentação.
- **Outros tipos de conta:** o Histórico continua desativado com "Em breve", como a Movimentação.
- **Pacote novo:** `recharts`, aprovado pelo usuário. Entra no `package.json` do `Money.Web` e na
  lista de pacotes aprovados do CLAUDE.md, onde já era o previsto para gráficos.

## Capabilities

### New Capabilities

- `historico-saldo`: a visão Histórico da conta, com o gráfico do saldo no tempo, a escolha do
  período, o resumo e o estado vazio.

### Modified Capabilities

- `contas`: o requisito "Página da conta" passa a ter o Histórico ativo na conta Dinheiro; nos
  demais tipos, continua "Em breve".

## Impact

- `Money.Services`: `HistoricoSaldoDto` e o cálculo dos pontos no `LancamentoServico` (saldo ao fim
  de cada dia com lançamento, a partir do saldo inicial da conta).
- `Money.Api`: rota `GET /api/contas/{id}/historico?de=&ate=`.
- `Money.Web`:
  - `lancamentos/Historico.tsx`;
  - `ContaPagina.tsx`, para a visão nova;
  - `api.ts`;
  - `package.json` (`recharts`).
- `CLAUDE.md`: `recharts` na lista de pacotes npm aprovados.
- Testes: serviço, API e um teste de tela.
- Sem mudança de banco.
