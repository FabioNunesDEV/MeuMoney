## 1. Domínio

- [x] 1.1 Criar `RegraNegocioException` em `Money.Domain/Comum`; build sem avisos
- [x] 1.2 Criar `Moeda` e o catálogo (BRL, USD, EUR, GBP, CHF, JPY, CAD, AUD, ARS, CLP, MXN, CNY) em `Money.Domain/Contas`; testes de busca por código e de código inexistente passando
- [x] 1.3 Criar `TipoConta` e a extensão com "representa dívida" e "pode ser caixa"; testes cobrindo os sete tipos passando
- [x] 1.4 Criar `EntidadeNomeada` (nome de 1 a 100, sem espaços nas pontas, e `Renomear`) e, sobre ela, `InstituicaoFinanceira` e `GrupoConta`; testes passando
- [x] 1.5 Criar `Conta` com criação, `DefinirMoedaESaldo` (sinal por tipo, Ativo ≥ 0, Investimento = 0, casas decimais da moeda), `VincularCaixa`/`DesvincularCaixa`, favorita, `Encerrar`/`Reabrir`; testes de cada regra passando

## 2. Serviços

- [x] 2.1 Declarar `IRepositorio<T>` em `Money.Services/Abstracoes` e os DTOs (`ContaDto`, `SalvarContaDto`, `ModoContaCaixa`, `GerenciadorContasDto`, `TotalMoedaDto`, `TipoContaDto`, `MoedaDto`, `CadastroSimplesDto`) em `Money.Services/Contas`; build sem avisos
- [x] 2.2 Criar repositórios `Fake` em memória em `Money.Services.Tests` para os testes de serviço
- [x] 2.3 Implementar `ContaServico`: gerenciador com totais por moeda (só contas abertas), obter, tipos com descrição e moedas; testes passando
- [x] 2.4 Implementar criar e atualizar conta em `ContaServico`: nome único sem diferenciar caixa, instituição e grupo por nome (reaproveita ou cria), tipo imutável; testes passando
- [x] 2.5 Implementar o vínculo com conta caixa: criar caixa junto, usar existente, nenhuma, caixas compatíveis e bloqueio de troca de moeda em conta vinculada; testes passando
- [x] 2.6 Implementar favoritar, encerrar e reabrir com o bloqueio de caixa vinculada (conta não se exclui); testes passando
- [x] 2.7 Implementar `InstituicaoFinanceiraServico` (listar com quantidade de contas, renomear com nome único, excluir só sem uso) e `GrupoContaServico` (listar, renomear, excluir soltando as contas); testes passando
- [x] 2.8 Registrar os serviços em `AddMoneyServices`; `InjecaoDependenciaTestes` passando

## 3. Persistência

- [x] 3.1 Criar `ContaConfiguracao`, `InstituicaoFinanceiraConfiguracao` e `GrupoContaConfiguracao` em `Money.Data/Contas` (tipo como texto, FKs Restrict/SetNull/Restrict); build sem avisos
- [x] 3.2 Implementar `Repositorio<T>` em `Money.Data/Comum` e registrá-lo em `AddMoneyData`; build sem avisos
- [x] 3.3 Gerar a migration `CriarContas` com `dotnet ef migrations add` e mostrar o arquivo ao usuário para revisão
- [x] 3.4 Testes em `Money.Data.Tests` (SQLite temporário): gravar e ler conta com instituição, grupo e caixa; excluir grupo solta as contas; excluir caixa referenciada falha; testes passando

## 4. API local e janela

- [x] 4.1 Criar `Money.Api` com as rotas de contas, instituições e grupos, o filtro de regra de negócio e `AddMoneyApi`/`MapMoneyApi`; testes de ponta a ponta em `Money.Api.Tests` passando
- [x] 4.2 Trocar o `Money.Desktop` pela casca: servidor local em `127.0.0.1` com porta livre, migrations ao iniciar e `JanelaPrincipal` com WebView2; remover as telas WPF e os pacotes `WPF-UI`, `CommunityToolkit.Mvvm` e `Microsoft.Extensions.Hosting`; build sem avisos
- [x] 4.3 Fazer o build do `Money.Desktop` compilar o `Money.Web` e copiar `dist/` para `wwwroot`; conferido pela pasta de saída

## 5. Telas em React

- [x] 5.1 Criar o `Money.Web` (Vite, TypeScript estrito, Tailwind) com `estilo.css` (paleta clara e escura), `api.ts` e os componentes de `comum/`; `npm run build` sem erro de tipo
- [x] 5.2 Criar a casca `App` com menu lateral (só ícones abaixo de 1000) e a página `Contas`: barra de ferramentas, busca, agrupamento, encerradas, totais por moeda, lista agrupada e estado vazio; conferido por captura
- [x] 5.3 Criar `ContaDialogo` (tipo, nome sugerido, instituição e grupo com sugestões, moeda, valor em pt-BR, favorita, conta caixa do investimento, erro no pop-up); salvar conferido pela tela
- [x] 5.4 Ligar editar, favoritar e encerrar/reabrir (conta não se exclui) ao menu de contexto, ao botão "Mais ações" e ao teclado, com aviso passageiro e Ctrl+N; menu de contexto conferido por captura
- [x] 5.5 Criar `CadastrosDialogo` para instituições e grupos (listar, renomear, excluir com confirmação); `npm run build` sem erro de tipo
- [x] 5.6 Criar a página `Inicio` (data, saldo total por moeda, dívidas, distribuição por tipo, favoritas, orientação sem favoritas, boas-vindas sem contas) como página de abertura; conferido por captura

## 5B. Ajustes da validação do usuário

- [x] 5B.1 Remover a exclusão de conta do serviço, da API e das telas (só encerrar); testes de serviço e de API passando
- [x] 5B.2 Escolha de tema (Windows, claro, escuro), guardada entre aberturas; conferido por captura
- [x] 5B.3 Refazer a casca e as páginas na organização do Money 99: barra de módulos, faixa de título com comandos, painel de tarefas, seções em faixa, barra de situação, gerenciador em ícones grandes ou lista e página da conta (`GET /api/contas/{id}` com teste); conferido por captura nos dois temas

## 6. Fechamento

- [x] 6.1 `dotnet build MeuMoney.slnx` sem avisos e `dotnet test MeuMoney.slnx` passando
- [x] 6.2 Entregar ao usuário o roteiro de conferência visual (temas claro/escuro, janela estreita, teclado, fluxos de criar/editar/encerrar/excluir, conta em USD, investimento com caixa) e aguardar o retorno
