## Why

O MeuMoney ainda não tem nenhum módulo de negócio: a janela abre vazia. Contas são a base de tudo o
que vem depois (lançamentos, contas a pagar, investimentos, painel), e o usuário precisa cadastrar
contas em reais e em moeda estrangeira, incluindo contas de investimento ligadas a uma conta caixa,
como opera hoje nas corretoras.

## What Changes

- Interface refeita com tecnologia web: as telas passam a ser em React, servidas por uma API local
  dentro do próprio aplicativo e exibidas numa janela de desktop. As telas WPF/WPF-UI saem.
- Casca do aplicativo: menu lateral, barra de ferramentas no topo de cada página, menus de contexto,
  pop-ups e avisos passageiros.
- Gerenciador de contas: página com as contas em cartões (nome, instituição, saldo na moeda da
  conta), agrupadas por tipo, instituição ou grupo; total por moeda no topo; contas encerradas
  ocultas por padrão; estado vazio com ação de criar.
- Cadastro de conta num diálogo único (sem assistente de várias janelas): tipo, nome, instituição,
  grupo, moeda, saldo inicial e favorita.
- Tipos de conta com comportamento fixo: Corrente, Poupança, Dinheiro, Cartão de crédito,
  Investimento, Ativo e Passivo, cada um com descrição curta.
- Moeda por conta (código ISO 4217), real como padrão.
- Conta de investimento ligada a uma conta caixa da mesma moeda: criada junto, escolhida entre as
  existentes ou nenhuma.
- Editar, marcar como favorita, encerrar (oculta e reabrível) e reabrir. Conta nunca é excluída.
- Página Início, a tela de entrada: data, saldo total por moeda e contas favoritas.
- Instituições financeiras e grupos de contas em cadastro simples (só nome), criados na hora no
  formulário da conta, com renomear e excluir.

Fora deste change: lançamentos e saldo corrente, cotação e total consolidado em reais, posições e
operações de investimento, dados de cartão (limite, fechamento, vencimento), próximas contas a
pagar no painel inicial e página de Configurações.

## Capabilities

### New Capabilities
- `contas`: cadastro, edição, encerramento e gerenciador de contas; tipos, moeda, saldo
  inicial, favorita e vínculo entre conta de investimento e conta caixa.
- `instituicoes-financeiras`: cadastro simples das instituições que mantêm as contas.
- `grupos-de-contas`: grupos definidos pelo usuário para organizar o gerenciador.
- `painel-inicial`: página Início com o saldo total por moeda e as contas favoritas.
- `navegacao`: casca do aplicativo com navegação lateral entre módulos e retorno ao usuário.

### Modified Capabilities

(nenhuma)

## Impact

- `Money.Domain`: entidades `Conta`, `InstituicaoFinanceira`, `GrupoConta`, o enum `TipoConta` e o
  catálogo de moedas, em `Contas/`.
- `Money.Services`: `ContaServico`, `InstituicaoFinanceiraServico`, `GrupoContaServico`, DTOs e
  interfaces de repositório, em `Contas/`.
- `Money.Data`: mapeamentos EF e repositórios, em `Contas/`.
- `Money.Data.Sqlite`: primeira migration (tabelas de contas, instituições e grupos).
- `Money.Api` (novo): rotas HTTP do módulo sobre os serviços.
- `Money.Web` (novo): telas em React + TypeScript + Tailwind (Início, Contas, pop-ups).
- `Money.Desktop`: deixa de ter telas; sobe o servidor local e abre a janela com WebView2.
- Testes novos em `Money.Domain.Tests`, `Money.Services.Tests`, `Money.Data.Tests` e `Money.Api.Tests`.
- Pacotes: saem `WPF-UI`, `CommunityToolkit.Mvvm` e `Microsoft.Extensions.Hosting`; entram
  `Microsoft.Web.WebView2`, o framework ASP.NET Core e os pacotes npm das telas (lista no `CLAUDE.md`).
