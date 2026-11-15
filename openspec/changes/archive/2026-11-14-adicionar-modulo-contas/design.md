## Context

Primeiro módulo de negócio sobre o esqueleto em camadas (ver `CLAUDE.md`, seção Arquitetura). Hoje:
`Entidade` (Id `Guid` v7) em `Money.Domain/Comum`, `IUnidadeDeTrabalho` e `IInicializadorBanco` em
`Money.Services/Abstracoes`, `MoneyDbContext` carregando mapeamentos por assembly, nenhuma migration,
e `JanelaPrincipal` só com `TitleBar` e um texto. Motivação e escopo em `proposal.md`; requisitos
nas specs deste change.

## Goals / Non-Goals

**Goals:**
- Modelo de conta que já comporte moeda e o vínculo investimento → caixa sem retrabalho quando
  vierem lançamentos e investimentos.
- Casca reaproveitável pelos próximos módulos: API por módulo, página com barra de ferramentas,
  pop-up, menus e avisos.

**Non-Goals:**
- Página de Configurações e as próximas contas a pagar no painel inicial (entram com os seus módulos).
- Índice único no banco para nomes (ver Decisões, 5).
- Data de abertura ou de encerramento da conta.

## Decisions

### 1. Entidades em `Money.Domain/Contas`
- `EntidadeNomeada` (em `Domain/Comum`): `Nome` validado e `Renomear`. Base de `Conta`,
  `InstituicaoFinanceira` e `GrupoConta`, que repetiriam a mesma validação, e permite verificar nome
  único num só lugar.
- `Conta`: `Tipo` (`TipoConta`), `CodigoMoeda`, `SaldoInicial`, `InstituicaoFinanceiraId?`,
  `GrupoContaId?`, `ContaCaixaId?`, `Favorita`, `Encerrada`. Setters privados; alterações por método
  (`DefinirMoedaESaldo`, `VincularCaixa`, `Encerrar`, `Reabrir`...), cada um validando sua regra.
- `InstituicaoFinanceira` e `GrupoConta`: só o nome herdado.
- `TipoConta`: enum com os sete tipos. A regra "representa dívida" (Cartão de crédito, Passivo) e
  "pode ser caixa" (Corrente, Poupança, Dinheiro) fica numa extensão do enum no Domain, para não
  espalhar `switch`.
- Regra violada lança `RegraNegocioException` (nova, em `Domain/Comum`) com mensagem em pt-BR pronta
  para exibir. Alternativa descartada: objeto `Resultado`. Exceção é mais simples com poucos fluxos e
  a API só precisa de um filtro que a converte em resposta 400.

### 2. Saldo com sinal guardado; conversão na entidade
`SaldoInicial` é guardado **com sinal** (dívida negativa), para que somar saldos dê o patrimônio sem
saber o tipo. `Conta.DefinirMoedaESaldo(codigoMoeda, valorInformado)` recebe o valor como o usuário
digita (dívida positiva) e inverte o sinal nos tipos de dívida; valida Ativo ≥ 0, Investimento = 0 e
as casas decimais da moeda. Moeda e saldo vão juntos porque o saldo é validado nas casas da nova
moeda (US$ 10,50 → ¥ 10 numa só alteração). O DTO devolve o saldo com sinal (exibição) e o valor informado (formulário).

### 3. Moeda: catálogo fixo no Domain
`Moeda` (código ISO 4217, nome em pt-BR, símbolo, casas decimais) e um catálogo estático: BRL, USD,
EUR, GBP, CHF, JPY, CAD, AUD, ARS, CLP, MXN, CNY. A conta guarda só o código, validado contra o
catálogo. Moeda nova = uma linha no catálogo.
Alternativas descartadas: cadastro de moedas (entidade e tela sem uso real agora) e lista gerada
pelo `RegionInfo` do .NET (nomes em inglês e códigos sem uso).

### 4. Vínculo investimento → caixa
`ContaCaixaId` na própria conta de investimento (auto-relacionamento). `Conta.VincularCaixa(Conta
caixa)` valida tipo, moeda, conta aberta e que não é a própria conta. Regras que dependem de outras
contas (caixa em uso não pode ser encerrada nem trocar de moeda) ficam em `ContaServico`,
que consulta o repositório. "Criar caixa junto" é feito por `ContaServico` numa única
`SalvarAlteracoesAsync`.

### 5. Nomes únicos verificados no serviço
O serviço carrega os nomes e compara com `StringComparison.InvariantCultureIgnoreCase`. O SQLite
compara maiúsculas e minúsculas só em ASCII ("Água" × "água" falharia num índice `NOCASE`), e
`UseCollation` amarraria o `Money.Data` ao provedor. Uso individual, sem concorrência, e volume
pequeno: a verificação em memória basta. Alternativa descartada: coluna `NomeNormalizado` com índice
único, que traria um conceito a mais sem dor concreta.

### 6. Serviços e DTOs em `Money.Services/Contas`
- `ContaServico`: gerenciador (contas e totais por moeda num `GerenciadorContasDto`), obter, criar,
  atualizar, favoritar, encerrar, reabrir, caixas compatíveis, tipos e moedas. Conta não se exclui.
  A instituição e o grupo chegam **por nome**; o serviço reaproveita o existente ou cria um novo.
- `InstituicaoFinanceiraServico` e `GrupoContaServico`: listar (com quantidade de contas), renomear
  e excluir.
- Um repositório genérico `IRepositorio<T>` (listar, obter por Id, adicionar, remover) em
  `Services/Abstracoes`, implementado por `Repositorio<T>` em `Money.Data/Comum`. Os três
  repositórios seriam idênticos; consulta específica, quando surgir, ganha interface própria.
- `InstituicaoFinanceiraServico` e `GrupoContaServico` devolvem o mesmo `CadastroSimplesDto` (Id, nome,
  quantidade de contas), o que também permite um único diálogo de manutenção para os dois.
- O total por moeda é regra de negócio e fica no serviço, não na tela.
- Nomes e descrições dos tipos (texto exibido) ficam no Services, para servir também a uma futura
  interface web.

### 7. Persistência
- Tabelas `Contas`, `InstituicoesFinanceiras` e `GruposConta`. `Tipo` gravado como texto
  (`HasConversion<string>()`): legível e imune à reordenação do enum.
- FK conta → instituição `Restrict` (a regra impede excluir em uso); conta → grupo `SetNull` (excluir
  grupo solta as contas); conta → caixa `Restrict`.
- O SQLite guarda `decimal` como texto, sem soma e ordenação no banco; totais são calculados em
  memória, o que basta para o volume de contas.
- Migration `CriarContas`, gerada com `dotnet ef migrations add` e mostrada para revisão.

### 8. Interface web: API local, janela e telas
As telas WPF/WPF-UI foram feitas e descartadas: o visual ficou aquém do pedido e a IA produz telas
mais ricas, com mais referência, em React. Decisão do usuário: React numa janela de desktop.

- `Money.Api` (novo, `net10.0`, sem Windows): rotas `/api` por módulo em minimal API
  (`MapMoneyApi`), cada uma chamando um serviço. `FiltroRegraNegocio` converte
  `RegraNegocioException` em 400 com `{ mensagem }`. Enum trafega como texto.
  Alternativa descartada: rotas dentro do `Money.Desktop`, que não seriam testáveis sem janela.
- `Money.Desktop`: composition root. Sobe um `WebApplication` no mesmo processo em
  `http://127.0.0.1:<porta livre>` (`Porta` no `appsettings.json`; 0 = livre), que serve a API e os
  arquivos das telas em `wwwroot`, aplica as migrations e abre `JanelaPrincipal`, uma janela WPF só
  com o `WebView2` apontado para esse endereço. O escopo de injeção por requisição substitui o
  escopo por operação de tela.
- `Money.Web` (novo): React 19 + TypeScript + Vite + Tailwind CSS 4, Radix UI para pop-ups e menus,
  Lucide para ícones, Sonner para avisos. O `Money.Desktop.csproj` roda `npm run build` (com
  `npm ci` na primeira vez) e copia `dist/` para `wwwroot`.
- Sem biblioteca de rotas nem de estado por enquanto: duas páginas e `useDados` (carregar e
  recarregar) bastam; entram quando houver dor.
- O WebView2 fica sem menu, atalhos e barra de status do navegador, e guarda seus dados em
  `%LOCALAPPDATA%\MeuMoney\WebView2`.

### 9. Telas
O usuário pediu telas parecidas com as do Money 99; a primeira versão em React (menu lateral e
listas, por força das regras antigas do `CLAUDE.md`) foi refeita na organização do Money.

- `App`: barra de módulos no topo, com os módulos futuros desativados e a troca de tema. Sem
  biblioteca de rotas: o estado guarda o módulo e, em Contas, a conta aberta.
- `comum/ui.tsx`: `Pagina` monta o arranjo de toda tela (faixa de título com comandos, painel de
  tarefas, barra de opções, conteúdo e barra de situação); `BlocoPainel` e `Tarefa` formam o painel;
  `Secao` é o bloco com faixa colorida. Completam: `Botao`, `Campo`, `Cartao`, `Popup`,
  `Confirmacao`, `AvisoErro`, `MenuContexto` e `MenuBotao`.
- `Contas`: gerenciador em ícones grandes ou em lista, agrupado em seções; busca, agrupamento e
  "Mostrar encerradas" na barra de opções; saldos por moeda no rodapé. Duplo clique ou Enter abre a
  conta; menu de contexto e botão "Mais ações" trazem editar, favoritar e encerrar ou reabrir
  (`contas/acoes.ts`, compartilhado com a página da conta).
- `ContaPagina`: detalhes da conta (rota `GET /api/contas/{id}`) e os mesmos comandos; Movimentação e
  Histórico ficam desativados até o módulo Lançamentos.
- `ContaDialogo`: tipo em botões com ícone e a descrição do escolhido; instituição e grupo em campo
  com lista de sugestões (`datalist`), o que permite escolher ou digitar um novo; bloco de conta
  caixa só para Investimento. O valor é digitado em pt-BR e convertido na tela; o erro da API aparece
  dentro do pop-up, que continua aberto.
- `CadastrosDialogo`: um pop-up para instituições e para grupos, com renomear na própria linha e
  confirmação de exclusão.
- Cores em variáveis CSS (`estilo.css`), com tema claro e escuro: barras em azul-petróleo, conteúdo
  claro, âmbar na ação principal, cores de tipo dessaturadas. Saldo negativo em vermelho, com sinal.
- Tema (`comum/tema.ts`): o botão na barra de módulos alterna Windows → claro → escuro. A escolha
  fica num cookie, e não no `localStorage`, porque a porta do servidor muda a cada abertura e, com
  ela, a origem da página; o cookie vale para qualquer porta.

### 9A. Página Início
- Usa a mesma rota do gerenciador: os totais vêm prontos; dívidas, distribuição por tipo e favoritas
  são recortes de exibição feitos na tela sobre as contas abertas.
- A data por extenso é a do relógio do navegador embutido; nenhuma regra depende dela.
- A favorita abre a página da conta. O painel traz a data e o atalho para o gerenciador.

### 10. Testes
- Domain: regras da `Conta` (nome, sinal, casas decimais, vínculo, encerrar).
- Services: com repositórios `Fake` em memória no projeto de teste (unicidade, criar caixa junto,
  bloqueios por vínculo, totais por moeda, instituição e grupo por nome).
- Api: de ponta a ponta em `Money.Api.Tests`, com Kestrel em porta livre, `HttpClient` e SQLite
  temporário, sem pacote de teste extra.
- Telas: sem teste automatizado por enquanto; verificação de tipos no build e conferência por captura.
- Data: repositórios e migration com SQLite em arquivo temporário (padrão de `ProvedorSqliteTestes`).

## Risks / Trade-offs

- [Unicidade sem índice no banco] → única via de escrita é o serviço, coberta por teste.
- [Investimento exibe saldo zero até existirem posições] → o cartão mostra o rótulo do tipo e o
  saldo da conta caixa aparece no cartão dela; resolvido no módulo de investimentos.
- [Catálogo fixo de moedas] → acrescentar moeda é uma linha; não cobre cripto (decidir no módulo de
  investimentos).
- [Primeira migration aplicada no banco real ao abrir o app] → é o comportamento previsto
  (`IInicializadorBanco`). A IA não roda o app contra o banco real; o usuário valida.
- [API local alcançável por outros programas da máquina] → escuta só em `127.0.0.1`, em porta que
  muda a cada abertura; sem cabeçalhos de CORS, páginas de outros sites não leem as respostas nem
  enviam JSON. Um segredo por sessão fica como reforço possível.
- [Segunda linguagem e ferramentas Node no projeto] → custo aceito pelo usuário em troca do visual;
  o `dotnet build` continua sendo o único comando necessário.
- [Dinheiro como número JSON] → exato até 15 dígitos significativos, suficiente para valores
  pessoais; a regra de casas decimais continua validada no domínio.

## Migration Plan

Migration `CriarContas` aplicada automaticamente na próxima abertura do aplicativo. Para desfazer
antes de aplicar: apagar os arquivos da migration e restaurar o snapshot pelo git.
