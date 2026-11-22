## 1. Domínio

- [x] 1.1 Criar `TipoConta.TemSaldoMinimo()` em `TipoContaExtensoes` (Corrente, Poupança e Dinheiro). Na `Conta`:
  - propriedades `NumeroConta`, `Abreviacao`, `Comentario` e `SaldoMinimo`;
  - constantes públicas dos limites (50, 20 e 1.000);
  - `DefinirComplementos` e `DefinirSaldoMinimo`, conforme o design (decisão 1).

  Build sem avisos.
- [x] 1.2 Testes de domínio, passando:
  - espaços das pontas removidos e vazio vira nulo;
  - cada limite de texto recusado com a mensagem do limite;
  - saldo mínimo recusado em Cartão de crédito;
  - saldo mínimo com casas demais recusado;
  - saldo mínimo negativo aceito na Corrente.

## 2. Persistência

- [x] 2.1 Mapear as quatro colunas na `ContaConfiguracao` (tamanhos das constantes e `HasPrecision(18, 2)`).
  Gerar a migration `AdicionarDetalhesConta` com `dotnet ef migrations add` e **mostrar o arquivo ao
  usuário para revisão** antes de seguir. A migration deve ter só quatro colunas anuláveis em `Contas`.
- [x] 2.2 Teste em `Money.Data.Tests` com a ida e volta dos quatro campos no SQLite temporário, passando.

## 3. Serviço e API

- [x] 3.1 Acrescentar os quatro campos ao `SalvarContaDto` e ao `ContaDto`, e `TemSaldoMinimo` ao
  `TipoContaDto`. No `ContaServico`:
  - aplicar os complementos no `AplicarCamposAsync`;
  - aplicar o saldo mínimo depois de `DefinirMoedaESaldo` no `AtualizarAsync`.

  Build sem avisos.
- [x] 3.2 Testes do `ContaServico`, passando:
  - criar e alterar com os quatro campos;
  - troca de moeda com saldo mínimo de casas incompatíveis recusada;
  - `ListarTipos` informando `TemSaldoMinimo`.
- [x] 3.3 Teste de ponta a ponta em `Money.Api.Tests`: `PUT /api/contas/{id}` com os quatro campos e
  `GET` devolvendo-os; recusa com 400 e `mensagem` para saldo mínimo em Cartão de crédito. Passando.

## 4. Telas

- [x] 4.1 Espelhar os campos novos no `api.ts` (`Conta`, `SalvarConta` e o tipo de conta com
  `temSaldoMinimo`). `npm run build --prefix src/Money.Web` sem erro.
- [x] 4.2 No `useFormularioConta`:
  - estado dos quatro campos (saldo mínimo como texto, lido com `lerValor` e com mensagem de valor
    inválido);
  - envio no `salvar`;
  - `alterado`, comparando com a conta original.

  Build sem erro.
- [x] 4.3 Criar a seção `CamposComplementares` em `CamposConta.tsx`:
  - número e abreviação lado a lado;
  - saldo mínimo só quando o tipo tem saldo mínimo, à direita e `tabular-nums`;
  - comentário em área de texto.

  Mostrá-la no `ContaDialogo` depois do saldo. Build sem erro.
- [x] 4.4 Criar `contas/DetalhesConta.tsx`, conforme o design (decisão 4):
  - cartão do tipo, situação e saldo só leitura;
  - seções em `Secao`, com o `AvisoErro` acima;
  - Desfazer e Salvar desativados sem alteração;
  - aviso passageiro ao salvar.

  Build sem erro.
- [x] 4.5 Na `ContaPagina`:
  - usar `DetalhesConta` com a `key` da conta carregada mais o contador de Desfazer;
  - o Editar troca para Detalhes com o foco no Nome, em vez de abrir o pop-up;
  - remover o uso do `ContaDialogo` da página.

  Build sem erro. Conferir que o gerenciador ainda abre o pop-up.

## 5. Teste de tela

- [x] 5.1 Criar `testes/detalhes.spec.ts`, com a conta criada pela API:
  - Editar a partir da Movimentação leva aos Detalhes com o foco no Nome;
  - preencher número, abreviação, comentário e saldo mínimo e salvar; trocar de visão, voltar e
    conferir os valores;
  - apagar o nome e salvar mostra a recusa no formulário;
  - alterar o comentário e Desfazer volta ao gravado, com Salvar desativado.

  `npm run testar --prefix src/Money.Web` passando.

## 6. Documentação e verificação

- [x] 6.1 Conferir o `CLAUDE.md` contra o entregue (tabela "Do Money 99 para o MeuMoney" e regras
  de tela) e corrigir o que divergir.
- [x] 6.2 Verificação completa: `dotnet build MeuMoney.slnx` sem avisos, `dotnet test MeuMoney.slnx`
  passando e `npm run testar --prefix src/Money.Web` passando.
- [x] 6.3 Conferir por captura, com banco temporário:
  - os Detalhes nos temas claro e escuro, numa conta Dinheiro e numa Investimento (sem saldo
    mínimo, com conta caixa);
  - o pop-up do gerenciador com os campos novos;
  - a janela estreita.

  Entregar ao usuário a conferência visual.
