## 1. Serviço e API

- [x] 1.1 Criar `HistoricoSaldoDto` e `PontoSaldoDto` em `Money.Services/Lancamentos/` e o `LancamentoServico.ObterHistoricoAsync(contaId, de, ate, cancellationToken)` conforme o design (decisão 1); build sem avisos
- [x] 1.2 Testes do `ObterHistoricoAsync`, passando:
  - degraus nos dias dos lançamentos (cenário 100 → 70 → 120);
  - nulo ignorado;
  - saldo no início com lançamentos anteriores ao período;
  - vários lançamentos no mesmo dia num só ponto;
  - lançamento futuro estende o `ate`;
  - `de` vazio começa no primeiro lançamento;
  - resumo (maior, menor, final);
  - conta sem lançamento com `TemLancamentos = false`.
- [x] 1.3 Mapear `GET /api/contas/{id}/historico` em `LancamentosEndpoints` (`de` opcional, `ate` obrigatório) e testar de ponta a ponta em `LancamentosApiTestes` (pontos e resumo de uma conta com gasto e recebimento); testes passando

## 2. Telas

- [x] 2.1 Instalar `recharts` (3.11) em `dependencies` do `Money.Web` e acrescentá-lo aos pacotes npm aprovados do `CLAUDE.md`; `npm run build --prefix src/Money.Web` sem erro
- [x] 2.2 Espelhar `HistoricoSaldo` e a rota `historico(contaId, de, ate)` no `api.ts`; build sem erro
- [x] 2.3 Criar `lancamentos/Historico.tsx`:
  - título "Movimentação do saldo", período dd/MM/yyyy até dd/MM/yyyy e o seletor das cinco opções (padrão 3 meses);
  - resumo (início, maior, menor, final) com `ValorSaldo`;
  - gráfico em degrau com eixo de datas proporcional, rótulos dd/MM e saldo curto;
  - dica com data e `ValorSaldo`;
  - linha do zero quando houver negativo;
  - cores das variáveis do `estilo.css`;
  - estado vazio com "Ir para a Movimentação".

  Build sem erro.
- [x] 2.4 Na `ContaPagina.tsx`:
  - visão Histórico ativa na Dinheiro ("Em breve" nos outros tipos), com `React.lazy` e `Suspense`;
  - botão Histórico na troca de visão da janela estreita;
  - rodapé "Saldo:".

  `npm run build` sem erro e com o `recharts` num arquivo separado do principal (conferir a saída do Vite).

## 3. Teste de tela

- [x] 3.1 Criar `testes/historico.spec.ts`, com conta e lançamentos criados pela API:
  - abrir o Histórico e conferir o resumo e a presença da linha do gráfico;
  - trocar o período para "Desde o primeiro lançamento" e conferir a data inicial;
  - conta sem lançamento mostra o estado vazio e "Ir para a Movimentação" leva à Movimentação.

  `npm run testar --prefix src/Money.Web` passando.

## 4. Documentação e verificação

- [x] 4.1 Conferir o `CLAUDE.md` contra o entregue (pacote `recharts`, carregamento sob demanda de telas pesadas) e corrigir o que divergir
- [x] 4.2 `dotnet build MeuMoney.slnx` sem avisos, `dotnet test MeuMoney.slnx` passando e `npm run testar --prefix src/Money.Web` passando
- [x] 4.3 Conferir por captura, com banco temporário, o Histórico nos temas claro e escuro, com saldo positivo, com saldo negativo e na janela estreita; entregar ao usuário a conferência visual
