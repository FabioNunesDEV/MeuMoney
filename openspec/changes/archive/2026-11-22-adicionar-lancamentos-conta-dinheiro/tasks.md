## 1. Domínio

- [x] 1.1 Criar em `Money.Domain/Lancamentos/` as entidades `Favorecido`, `Categoria` e `Subcategoria` (`EntidadeNomeada`; a subcategoria com `CategoriaId`) e os enums `TipoLancamento` e `SituacaoLancamento`; testes de criação e nome inválido passando
- [x] 1.2 Criar `Conta.GarantirAceitaLancamento()`, que recusa conta encerrada ou que não seja Dinheiro, com mensagem; testes `GarantirAceitaLancamento_*` passando
- [x] 1.3 Criar `Lancamento`:
  - `CriarGasto`, `CriarRecebimento` e `CriarTransferencia`, esta devolvendo as duas pontas com o mesmo `TransferenciaId`;
  - `Alterar`, `MarcarComo`, `MoverPara` e `EfeitoNoSaldo`;
  - validações de valor (> 0, casas da moeda), memo (≤ 255), contas diferentes e da mesma moeda.

  Testes de cada regra e do efeito no saldo (inclusive Nulo) passando.

## 2. Persistência

- [x] 2.1 Criar `ILancamentoRepositorio` em `Money.Services/Lancamentos/`, com `ListarDaContaAsync`, `SomarEfeitosPorContaAsync` e `ListarDaTransferenciaAsync`, e o `LancamentoRepositorioFake` em `Money.Services.Tests/Fakes`; build sem avisos
- [x] 2.2 Criar em `Money.Data/Lancamentos/`:
  - as configurações EF de `Favorecido`, `Categoria`, `Subcategoria` e `Lancamento` (texto nos enums, `HasPrecision(18, 2)`, `Restrict` nas chaves, índices `(ContaId, Data)`, `TransferenciaId` e `FavorecidoId`);
  - o `LancamentoRepositorio`, registrado em `AddMoneyData`.

  Build sem avisos.
- [x] 2.3 Gerar a migration `AdicionarLancamentos` com `dotnet ef migrations add` e **mostrar o arquivo ao usuário para revisão** antes de seguir. Conferir que só há `CreateTable` e `CreateIndex`, sem alterar a tabela `Contas`.
- [x] 2.4 Testes em `Money.Data.Tests/Lancamentos/` com SQLite temporário, passando:
  - gravar e ler um gasto e uma transferência;
  - `ListarDaContaAsync` na ordem de data e registro;
  - `SomarEfeitosPorContaAsync` ignorando nulos;
  - excluir as duas pontas numa gravação.

## 3. Serviços

- [x] 3.1 Levar `ResolverPorNomeAsync` do `ContaServico` para `Money.Services/Comum/Nomes.cs`, com a variante que filtra subcategorias da categoria; testes existentes de contas continuam passando
- [x] 3.2 Criar os DTOs em `Money.Services/Lancamentos/`:
  - `SalvarLancamentoDto`, `LancamentoDto` (com o saldo após a linha e o nome da outra conta), `MovimentacaoDto`;
  - `OpcoesLancamentoDto` (favorecidos com sugestão, categorias com subcategorias, contas compatíveis);
  - `ResultadoLancamentoDto` e `SaldoNegativoDto`.

  Build sem avisos.
- [x] 3.3 Criar o `LancamentoServico`:
  - `ObterMovimentacaoAsync`, `ObterOpcoesAsync`, `CriarAsync`, `AlterarAsync`, `MarcarAsync`, `MoverAsync` e `ExcluirAsync`, todos com `CancellationToken`;
  - nas transferências, alterar, anular e excluir valem para as duas pontas;
  - o registro fica em `AddMoneyServices`.
- [x] 3.4 Implementar no `CriarAsync`/`AlterarAsync` a verificação de saldo negativo (design, decisão 4): devolver `Gravado: false` com as contas que ficariam negativas quando não há `ConfirmarSaldoNegativo`; gravar com a confirmação
- [x] 3.5 Testes do `LancamentoServico`, passando:
  - criar gasto, recebimento e transferência;
  - categoria obrigatória e subcategoria opcional;
  - nomes reaproveitados sem duplicar;
  - sugestão pelo último uso;
  - alteração e exclusão nas duas pontas;
  - Nulo nas duas pontas, mas C e R só no lado marcado;
  - mover (inclusive a recusa de mover para o outro lado da transferência);
  - conta encerrada e conta não Dinheiro recusadas;
  - saldo negativo: sem confirmação não grava; com confirmação grava; recebimento numa conta negativa não pergunta.
- [x] 3.6 No `ContaServico`, calcular saldo e totais com `SomarEfeitosPorContaAsync` e recusar troca de moeda de conta com lançamentos; testes `ObterGerenciador_ComLancamentos_*` e `Atualizar_MoedaComLancamentos_Recusa` passando

## 4. API

- [x] 4.1 Criar `LancamentosEndpoints` com as rotas do design (decisão 5) e mapeá-lo em `MapMoneyApi`; build sem avisos
- [x] 4.2 Testes de ponta a ponta em `Money.Api.Tests/LancamentosApiTestes.cs`, passando:
  - criar duas contas Dinheiro;
  - gasto, recebimento e transferência pela API;
  - movimentação com saldo linha a linha;
  - saldo no gerenciador;
  - saldo negativo com `Gravado: false` e depois confirmado;
  - 400 com `mensagem` para montante negativo;
  - exclusão da transferência nas duas contas.

## 5. Telas

- [x] 5.1 Espelhar os DTOs e as rotas novas no `api.ts`; `npm run build --prefix src/Money.Web` sem erro
- [x] 5.2 Estender a `AcaoMenu` em `comum/ui.tsx` com `submenu`, `marcado` e `desativado`, sem mudar os menus de conta atuais; build sem erro
- [x] 5.3 Criar o componente `ValorSaldo` em `comum/ui.tsx` e a variável `--saldo-positivo` (temas claro e escuro) no `estilo.css`:
  - positivo em azul, negativo em vermelho entre parênteses, zero na cor normal;
  - negrito opcional.

  Trocar por ele os 8 pontos que repetem a regra (gerenciador: conta e total; Início: favoritas, totais e dívidas; página da conta: rodapé, destaque e detalhes). Build sem erro, e nenhum `saldo < 0 && 'text-negativo'` sobrando (`grep`).
- [x] 5.4 Criar `lancamentos/Movimentacao.tsx`:
  - a grade com Data, Favorecido, Categoria, C, Gastar, Receber e Saldo;
  - Gastar e Receber na cor normal; coluna Saldo com `ValorSaldo` sem negrito;
  - lançamento nulo identificado e com o valor riscado;
  - linha focável, com setas e Enter;
  - menu de contexto e "Mais ações";
  - estado vazio e saldo final no rodapé, com `ValorSaldo` em negrito.

  Build sem erro.
- [x] 5.5 Criar `lancamentos/useFormularioLancamento.ts` e `lancamentos/FormularioLancamento.tsx`:
  - as abas Gasto (padrão), Recebimento e Transferência;
  - Novo, Editar, Inserir e Cancelar com o comportamento da spec, inclusive Inserir com campos vazios;
  - `datalist` para favorecido, categoria e subcategoria, com a sugestão pelo último uso;
  - De e Para na transferência;
  - erro no próprio formulário;
  - Ctrl+N, Enter e Esc.

  Build sem erro.
- [x] 5.6 Pop-up de saldo negativo no Inserir, com `Confirmacao`, a conta e o saldo resultante: Cancelar mantém o formulário, Confirmar reenvia com `confirmarSaldoNegativo`; build sem erro
- [x] 5.7 Criar `lancamentos/acoesLancamento.ts`:
  - Editar, Ir para conta (transferência), Marcar como (submenu com a atual marcada), Mover para conta (pop-up com as contas compatíveis) e Excluir (`Confirmacao`);
  - avisos do Sonner;
  - ações desativadas em conta encerrada.

  Build sem erro.
- [x] 5.8 Na `ContaPagina.tsx`:
  - visões Movimentação (padrão na Dinheiro; "Em breve" nos outros tipos) e Detalhes, com o Histórico em "Em breve";
  - Detalhes com "Saldo inicial" pelo `saldoInformado` e a linha "Saldo";
  - menu "Contas" no painel (gerenciador, abertas, submenu de encerradas);
  - abaixo de 1000 de largura, visões e menu "Contas" na barra de opções.

  Build sem erro.

## 6. Teste de tela

- [x] 6.1 Expor na fixture `janela` o endereço do aplicativo, para criar contas pela API no teste; `npm run verificar --prefix src/Money.Web` sem erro
- [x] 6.2 Criar `testes/lancamentos.spec.ts`, com o roteiro de aceite:
  - "Carteira" (100,00) e "Cofre" (0,00);
  - gasto e recebimento na "Carteira", recebimento no "Cofre";
  - transferência da "Carteira" para o "Cofre";
  - confere grade, saldos finais e total no gerenciador.
- [x] 6.3 Acrescentar o teste do pop-up de saldo negativo: Cancelar não grava; Confirmar grava e mostra o saldo negativo

## 7. Documentação e verificação

- [x] 7.1 Atualizar o `CLAUDE.md`:
  - a regra "Verde e vermelho só para valor" passa a descrever o formato contábil de saldo, com `ValorSaldo` e `--saldo-positivo`; o verde continua para entradas e variações;
  - conferir o restante contra o que foi entregue (pastas `lancamentos/`, rotas, padrões novos da `AcaoMenu`) e corrigir o que divergir.
- [x] 7.2 `dotnet build MeuMoney.slnx` sem avisos, `dotnet test MeuMoney.slnx` passando e `npm run testar --prefix src/Money.Web` passando
- [x] 7.3 Entregar ao usuário a conferência visual, nos temas claro e escuro:
  - Movimentação com as três abas e o fluxo Novo, Inserir, Editar e Cancelar;
  - pop-up de saldo negativo;
  - saldos em azul e em vermelho entre parênteses no gerenciador, no Início (favoritas, totais, dívidas) e na Movimentação;
  - menu de contexto (Marcar como, Mover, Excluir) e menu "Contas";
  - janela estreita (< 1000);
  - o roteiro de duas contas Dinheiro com transferência.
