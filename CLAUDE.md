# MeuMoney

Aplicativo desktop pessoal de gestão financeira, com o **Money 99** como referência funcional e o
visual próprio, moderno, feito com tecnologia web. Uso individual, não comercial, mantido por uma pessoa.

Prints de referência do Money 99 ficam em `imagens_tela/<Modulo>/` (ex.: `imagens_tela/Contas/`),
fora do git; em outra máquina a pasta precisa ser copiada à mão. **Os prints mostram dados
financeiros reais:** nunca copiar nomes, contas ou valores deles para código, testes, specs, dados
de exemplo ou mensagens de commit.

## Stack

| Item | Tecnologia |
|---|---|
| Linguagem | C# (`LangVersion latest`), `Nullable` e `ImplicitUsings` ativos |
| Runtime | .NET 10 (`net10.0`; Desktop em `net10.0-windows`) |
| SDK | 10.0.402, fixado no `global.json` (há SDK 11 RC na máquina; não usar) |
| Telas | **React 19 + TypeScript**, Vite, **Tailwind CSS 4**, Radix UI (pop-ups e menus), ícones Lucide, avisos com Sonner. Projeto `src/Money.Web` |
| Janela | WPF só como casca: uma janela com **WebView2** mostrando as telas |
| API local | **ASP.NET Core** (minimal API) dentro do mesmo processo, em `127.0.0.1` com porta livre; serve a API e as telas compiladas |
| Ferramentas web | Node 24 e npm; o `dotnet build` chama `npm run build` sozinho |
| Persistência | **EF Core 10**, sem amarrar a um provedor |
| Banco | **SQLite**, em `%LOCALAPPDATA%\MeuMoney\meumoney.db` |
| Testes | **xUnit 2.9**; telas com **Playwright** (`@playwright/test`) pela janela real |
| Planejamento | **OpenSpec** (fluxo spec-driven, artefatos em pt-BR) |
| Solução | `MeuMoney.slnx`; pacotes com versão central em `Directory.Packages.props` |
| IDE | Visual Studio 2026 (o VS 2022 não carrega projetos .NET 10) |

### Comandos

```
dotnet build MeuMoney.slnx
dotnet test MeuMoney.slnx
dotnet run --project src/Money.Desktop
npm run build --prefix src/Money.Web      # verifica tipos (tsc) e compila as telas
npm run dev --prefix src/Money.Web        # telas com recarga; abrir o aplicativo com Porta=5199
npm run testar --prefix src/Money.Web     # testes de tela pela janela real (compilar antes)
dotnet ef migrations add <Nome> --project src/Money.Data.Sqlite --startup-project src/Money.Data.Sqlite
openspec list
```

A saída do SDK vem em português nesta máquina: decidir por código de saída, não pelo texto.

### Pacotes aprovados

`Microsoft.EntityFrameworkCore` (+ `.Relational`, `.Sqlite`, `.Design`),
`Microsoft.Extensions.DependencyInjection` (+ `.Abstractions`), o framework `Microsoft.AspNetCore.App`,
`Microsoft.Web.WebView2`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`,
`StyleCop.Analyzers`, `Serilog` (+ `.Extensions.Hosting`, `.Sinks.File`).

Pacotes npm aprovados (`src/Money.Web/package.json`): `react`, `react-dom`, `radix-ui`, `lucide-react`,
`sonner`, `tailwindcss`, `@tailwindcss/vite`, `vite`, `@vitejs/plugin-react`, `typescript`,
`@types/react`, `@types/react-dom`, `@playwright/test`, `recharts` (gráficos; traz o `react-is` como
dependência). Tela que usa `recharts` é carregada sob demanda (`React.lazy`), para não pesar na
abertura do aplicativo.

JSON com o `System.Text.Json` do próprio .NET; não usar `Newtonsoft.Json`.

**Pacote fora desta lista: explicar o motivo, o que ele fará e o custo, e esperar autorização.**
Aprovado, entra nesta lista e no `Directory.Packages.props` (versão nunca no `.csproj`) ou no
`package.json`. Ainda sem decisão, a resolver no módulo que precisar: asserções/mocks de teste,
validação de entrada, importação de extrato (OFX/CSV).

## Arquitetura

Camadas num único executável. A dependência aponta sempre para dentro:
`Money.Domain ← Money.Services ← Money.Data / Money.Api ← Money.Desktop`. As telas (`Money.Web`) só
falam com a API, por HTTP.

| Projeto | Responsabilidade | Depende de |
|---|---|---|
| `Money.Domain` | Entidades e regras de negócio | nada |
| `Money.Services` | Serviços por módulo, DTOs e as interfaces de repositório | Domain |
| `Money.Data` | EF Core: `MoneyDbContext`, mapeamentos, repositórios. Não conhece provedor | Services, Domain |
| `Money.Data.Sqlite` | Escolha do SQLite, migrations, fábrica de design-time | Data |
| `Money.Api` | Rotas HTTP `/api` por módulo; chama os serviços e devolve os DTOs. Regra violada vira 400 com `{ mensagem }` | Services |
| `Money.Desktop` | Composition root: sobe o servidor local, escolhe o provedor (`appsettings.json`) e abre a janela com WebView2 | Api, Services, Data.Sqlite |
| `Money.Web` | Telas em React; compiladas para `dist/` e copiadas para o `wwwroot` do Desktop | só a API |

- Cada projeto expõe uma extensão `AddMoney*` (`AddMoneyServices`, `AddMoneyData`, `AddMoneySqlite`,
  `AddMoneyApi` + `MapMoneyApi`) numa classe `InjecaoDependencia`.
- A API não tem regra de negócio: cada rota é uma chamada a um serviço. Enum trafega como texto;
  dinheiro trafega como número JSON e volta a `decimal` no C#.
- Trocar para SQL Server = criar `Money.Data.SqlServer` no mesmo padrão e acrescentar o caso em
  `App.AdicionarBanco`. Nada mais muda.
- **Nenhum tipo de interface** (WPF, HTTP, diálogos, caminhos escolhidos pelo usuário) em Domain, Services
  ou Data: pode haver interface web no futuro.
- Services não conhece UI, devolve **DTOs, nunca entidades**, e todo método assíncrono recebe
  `CancellationToken`.
- Pastas internas **por módulo funcional** (`Contas/`, `Lancamentos/`), não por tipo técnico
  (`Entities/`, `Repositories/`). `Comum/` só para o que é de fato compartilhado.
- Mapeamentos do EF (`IEntityTypeConfiguration<T>`) na pasta do módulo em `Money.Data`; o
  `MoneyDbContext` os carrega por `ApplyConfigurationsFromAssembly`.
- Migrations só em `Money.Data.Sqlite/Migrations/`. O aplicativo aplica as pendentes ao iniciar
  (`IInicializadorBanco`).

## Normas de código

### Simplicidade

- Entre duas formas, escolher a **mais fácil de entender**. Código legível antes de código esperto.
- Solução com muitos arquivos porque há muitos módulos é esperado; muitos conceitos por módulo é defeito.
- **Interface só com inversão de dependência real**: repositório (Services declara, Data implementa)
  e serviço externo. Serviço de aplicação é classe concreta, um por módulo, injetado direto;
  interface só se surgir segunda implementação ou necessidade de dublê em teste.
- Entidade → DTO **na mão**, sem biblioteca de mapeamento.
- Ao propor abstração (padrão, camada, classe base), dizer qual dor concreta ela resolve. Sem dor, não criar.
- Não duplicar código. Não deixar código morto, comentado ou "para depois".

### Idioma e nomes

- Identificadores **em português, sem acento nem cedilha**: `Conta`, `Lancamento`, `SaldoInicial`,
  `ObterPorIdAsync`. Acento só em comentário e texto exibido.
- Ficam em inglês: termos técnicos consagrados (`DbContext`, `Endpoints`, `Dto`, `Id`, `Async`,
  `Fake`), nomes impostos pelo framework (`App`, `OnStartup`, `Dispose`, `CreateDbContext`), o
  parâmetro `cancellationToken`, os nomes de projeto e as extensões `AddMoney*`.
- PascalCase para tipos, métodos, propriedades e constantes; camelCase para locais e parâmetros;
  camelCase **sem sublinhado** para campos privados, acessados com `this.`.
- Interfaces com `I`; métodos assíncronos com sufixo `Async`.
- Sufixos por papel: `ContaServico`, `IContaRepositorio`/`ContaRepositorio`, `ContaDto`,
  `ContaConfiguracao` (mapeamento EF), `ContasEndpoints` (rotas da API).

### Estilo

- **StyleCop.Analyzers** vale em todos os projetos (`Directory.Build.props`), com as regras padrão e a
  configuração em `stylecop.json`. Em conflito com este documento, **o StyleCop vence** e o documento
  se corrige. As migrations ficam fora da análise (`generated_code` no `.editorconfig`). Pontos que
  mais aparecem:
  - cabeçalho no topo de todo `.cs`:
    ```csharp
    // <copyright file="Conta.cs" company="Fábio Nunes">
    // Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
    // </copyright>
    ```
  - `this.` em toda chamada a membro de instância (SA1101);
  - ordem dos membros: campos, construtores, propriedades, métodos; por acesso, público primeiro;
    tipo aninhado por último (SA1201, SA1202).
- Namespace por arquivo (`namespace X;`), `using` fora do namespace, `System` primeiro.
- Chaves sempre, mesmo em `if` de uma linha. Indentação de 4 espaços (2 em XAML, JSON e `.csproj`).
- Um tipo público por arquivo, com o nome do arquivo igual ao do tipo.
- `sealed` em classe que não foi feita para herança; `internal` no que não sai do projeto.
- Construtor primário para injeção de dependência.
- Build **sem avisos**, garantido pelo `TreatWarningsAsErrors` no `Directory.Build.props`: aviso
  quebra a compilação. Aviso de nullable se corrige, não se suprime com `!`.

### Comentários

- Todo tipo e membro, inclusive de teste: `/// <summary>` curto dizendo para que serve.
  Construtor primário: `<param>` no `<summary>` da classe.
- Todo método: `<summary>`, `<param>` para cada argumento e `<returns>` quando retorna valor
  (inclusive `Task`).
  Em implementação de interface ou override, `/// <inheritdoc />`.
- Dentro do método, comentar só o **porquê** que não é evidente, no mínimo de palavras.
- Textos padrão em pt-BR (`documentationCulture`): construtor começa com
  `Inicia uma nova instância da classe <see cref="X"/>`; propriedade com `Obtém` ou `Obtém ou define`.

### Log

- **Serilog** grava só **falhas**, só em **arquivo texto** (`Serilog.Sinks.File`): nível mínimo `Error`,
  valendo também para o que o framework registra (Host, EF Core). Nada de console ou outro destino.
  Um arquivo por dia em `<PastaDados>\logs\` (padrão `%LOCALAPPDATA%\MeuMoney\logs\`), guardando
  30 dias. Configurado no `App` do Desktop, o único projeto que referencia o Serilog; os demais usam
  `ILogger<T>`.
- No código, log só com `LogError` ou `LogCritical`, levando a exceção. Notificação de rotina
  (`LogInformation`, `LogDebug`, `LogTrace`, `LogWarning`) não se registra.

### Regras de domínio

- Dinheiro sempre em **`decimal`**, nunca `double` ou `float`.
- Data e hora sempre por **`TimeProvider`** injetado, nunca `DateTime.Now`, `DateTime.UtcNow` ou
  `DateTimeOffset.Now`. Data sem hora (vencimento, competência) em `DateOnly`.
- Id das entidades: `Guid` v7, gerado na criação por `Entidade`.
- **Conta nunca é excluída**, nem de vez nem logicamente: a única forma de tirá-la de uso é encerrar.
- Entidade protege seu estado: setters privados, alteração por método com a regra
  (`conta.Renomear(nome)`), validação na própria entidade.

### Telas (Money.Web)

- TypeScript estrito; o `npm run build` falha com erro de tipo. Nomes em português como no C#:
  componentes e tipos em PascalCase (`ContaDialogo`), funções e variáveis em camelCase.
- Pastas por módulo (`contas/`, `inicio/`, `lancamentos/`) e `comum/` para o que é compartilhado:
  `ui.tsx` (botão, campo, pop-up, menus, página, cartão, `ValorSaldo`), `formato.ts`,
  `tiposConta.tsx`, `useDados.ts`. Os menus recebem uma lista de `AcaoMenu`, que pode ter submenu,
  item marcado e item desativado.
- Todas as chamadas HTTP passam por `api.ts`, que espelha os DTOs. Tela não faz `fetch` direto.
- Regra de negócio fica em Services. A tela só valida formato de digitação (ex.: valor em pt-BR) e
  mostra a `mensagem` devolvida pela API.
- Usar os componentes de `comum/ui.tsx` e as peças do Radix antes de criar componente novo.
- Comentário `/** ... */` curto em componente e função exportados; dentro, só o porquê.
- A janela WPF (`JanelaPrincipal`) só hospeda o WebView2 e pinta a barra de título com as cores que
  as telas mandam (`comum/tema.ts`): nada de outra lógica nem de tela em XAML.
- Textos exibidos em português, com acento.

### Testes

- Código novo de Domain e Services tem teste unitário. Persistência tem teste em `Money.Data.Tests`
  com SQLite em arquivo temporário (ver `ProvedorSqliteTestes`). Rota nova da API tem teste em
  `Money.Api.Tests`, de ponta a ponta (HTTP real em porta livre + SQLite temporário).
- Fluxo de tela tem teste em `src/Money.Web/testes/`, com Playwright **pela janela real**: a fixture
  `janela` (`testes/janela.ts`) abre o `Money.Desktop.exe` compilado com banco, log e dados do
  WebView2 na pasta do teste, se liga ao WebView2 pela porta de depuração e fecha tudo no fim. A
  janela aparece na tela: não usar o mouse enquanto os testes rodam. Localizadores por papel e
  rótulo (`getByRole`, `getByLabel`). Dado de partida que não é o assunto do teste nasce pela API do
  aplicativo aberto (`enderecoDoAplicativo`). A conferência por captura continua valendo para o visual.
- Nome do teste em português: `Metodo_Cenario_ResultadoEsperado`.
- Teste nunca toca o banco real em `%LOCALAPPDATA%\MeuMoney`.

## Interface visual

O MeuMoney segue a **organização de tela do Money 99, com acabamento atual**. O Money 99 diz o que
cada tela faz **e** como ela se arruma; o que muda é o acabamento: tipografia, espaçamento, ícones,
temas claro e escuro. Os prints em `imagens_tela/` valem também como referência de layout.

### Arranjo de toda tela

| Parte | Como é |
|---|---|
| Barra de módulos (topo) | Faixa escura com Início, Contas, Contas a pagar, Investimentos, Planejador, Relatórios, Categorias. O módulo atual tem sublinhado âmbar; os que ainda não existem aparecem desativados. À direita, a troca de tema |
| Faixa de título | Abaixo da barra, com o título da página e os comandos (`Botao` nas variantes `barra` e `destaque`); a ação principal é a âmbar |
| Painel de tarefas (esquerda) | Escuro, com texto de ajuda, as tarefas da página e as visões (`BlocoPainel` + `Tarefa`). Some abaixo de 1000 de largura |
| Barra de opções | Clara, logo abaixo do título: busca, agrupamento e opções de exibição |
| Conteúdo | Superfície clara; blocos com **faixa de seção** colorida no topo (`Secao`), como as faixas do Money |
| Barra de situação (rodapé) | Totais e saldos da página, à direita |

Tudo isso é o componente `Pagina` de `comum/ui.tsx`; página nova não monta o arranjo à mão.

### Do Money 99 para o MeuMoney

| No Money 99 | No MeuMoney |
|---|---|
| Barra de texto (Conteúdo, Contas, Contas a pagar...) | A barra de módulos, igual na ordem |
| Home page com faixas de título | Início com seções em faixa: contas favoritas (nome como link e saldo final), saldos, onde está o dinheiro |
| Gerenciador de contas com ícones grandes | Ícones grandes por conta, agrupados em seções; a visão em lista é a alternativa; saldos no rodapé |
| Clique na conta abre a conta | Duplo clique ou Enter abre a página da conta, com Movimentação, Detalhes e Histórico no painel e o menu "Contas" para ir a outra |
| Registro com formulário embaixo (Gasto, Recebimento, Transferência) | Movimentação: grade com saldo linha a linha e, embaixo, o formulário em abas com Novo, Editar, Inserir e Cancelar |
| Botões no rodapé (Nova conta...) | Comandos na faixa de título e no painel de tarefas; ações do item no menu de contexto e no botão "…" |
| Assistente com várias janelas | Um pop-up com passos curtos (Voltar, Avançar, Concluir) |
| Ilustrações clip-art | Nenhuma ilustração; ícones Lucide |
| Dica do dia, Anúncios, Consultor financeiro, Leitura via Web | Não reproduzir |

### Regras

- **Cores vêm das variáveis de `src/Money.Web/src/estilo.css`**, nunca fixas no componente. Cada uma
  tem valor para o tema claro e para o escuro; o usuário escolhe entre o tema do Windows, o claro e
  o escuro (`comum/tema.ts`). Toda tela funciona nos dois.
- **Paleta**: barras em azul-petróleo escuro com texto claro (`barra`, `faixa`, `lateral`, `secao`);
  conteúdo em superfície clara e neutra (`fundo`, `painel`); **âmbar** (`destaque`), o complementar
  do azul-petróleo, só na ação principal e no módulo atual; `realce` para links e seleção no
  conteúdo; cores de tipo de conta dessaturadas (`--tipo-*`) nos selos. Vivo, sem cor gritante.
- **Saldo no formato contábil**: positivo em azul (`saldo-positivo`) e negrito; negativo em vermelho
  (`negativo`) e negrito, entre parênteses e sem sinal de menos (`(R$ 30,00)`): o parêntese marca o
  negativo sem depender da cor; zero na cor do texto. Todo saldo passa por `ValorSaldo`
  (`comum/ui.tsx`); na grade, sem negrito. O montante de um lançamento é sempre positivo e na cor do
  texto: a coluna (Gastar ou Receber) diz o sentido. O verde (`positivo`) fica para entradas e
  variações.
- **Densidade do Money**: texto base de 13 px, linhas de tabela baixas, muita informação por tela.
  Espaçamento em múltiplos de 4.
- **Valores monetários** no padrão pt-BR com o símbolo da moeda (`R$ 1.234,56`, `US$ 320,10`),
  alinhados à direita, com algarismos de largura fixa (`tabular-nums`). Datas em `dd/MM/yyyy`.
- **Módulo ou visão que ainda não existe** aparece desativado, com a dica "Em breve"; nunca com
  dado inventado.
- **Estado vazio** com ícone, mensagem e ação ("Nenhuma conta cadastrada" + "Criar conta").
- **Retorno ao usuário**: aviso passageiro (Sonner) para ação concluída ou recusada; pop-up de
  confirmação (`Confirmacao`) para exclusão de cadastro; erro de formulário dentro do próprio pop-up
  (`AvisoErro`). `MessageBox` do Windows só no erro fatal de inicialização.
- **Teclado**: toda ação alcançável sem mouse; item de lista focável, Enter abre, tecla de menu abre
  o menu de contexto; Ctrl+N para novo; botão só com ícone tem `aria-label`.
- A janela tem largura mínima de 720, sem rolagem horizontal: o painel de tarefas some abaixo de
  1000 e as barras quebram em linhas.
- **Tela nova se confere por captura antes de entregar**: abrir o aplicativo com banco temporário
  (`Database__ConnectionString`, `PastaDados` e `Porta=5199` no ambiente) e olhar a janela e as
  interações.

## Regras de trabalho para a IA

### Comunicação

- Responder sempre em **português pt-BR**, inclusive mensagens de commit, PR e artefatos do OpenSpec.
- Direto e factual: sem desculpas, elogios ou rodeios.

### Escopo

- Fazer **só o que foi pedido**. Melhoria fora do pedido se sugere, não se aplica.
- Exemplo dado pelo usuário ilustra o rumo; não é requisito nem define escopo.
- Limite declarado ("não mexa no código", "só analise") vale para a mensagem inteira, mesmo que um
  pedido adiante pareça autorizar ação. Na dúvida, perguntar.
- Escolha técnica é do usuário: a IA recomenda, aponta o custo e segue a decisão.
- Documento e código discordando: **o código vence**, e o documento se corrige no mesmo passo.
  Nunca citar API, método ou chave sem conferir no código.

### Banco de dados

- O banco em `%LOCALAPPDATA%\MeuMoney\` guarda dados financeiros reais. A IA **nunca** apaga,
  sobrescreve, roda `dotnet ef database update` ou script contra ele.
- Migration nova: gerar com `dotnet ef migrations add` e mostrar o arquivo para revisão.
  Desfazer migration ainda não aplicada: apagar os arquivos dela e restaurar o snapshot pelo git.

### Validação

- Antes de dar uma tarefa por concluída: `dotnet build` sem avisos (inclui a verificação de tipos e
  a compilação das telas), `dotnet test` passando e `npm run testar --prefix src/Money.Web`
  passando (depois do build, que gera o executável testado).
- Validação visual e de uso do aplicativo é do usuário: a IA entrega o build pronto, diz o que
  conferir e espera o retorno.

### Git e fluxo

- Funcionalidade nova começa por change do OpenSpec: `/opsx:propose` → `/opsx:apply` →
  `/opsx:archive`. Arquivar e sincronizar specs **no fim do próprio change**, não depois.
- Nada é commitado direto na `main`. Uma branch por change, criada a partir da `main`, com o nome
  do change.
- **Commit, push e PR só com pedido.** Pedido dado, commit e push andam juntos; não terminar com
  commit só local.
- Fechamento da tarefa: `/pull-request`, que já arquiva o change concluído (com as specs), faz o
  commit, o push e abre o PR, sem perguntar por cada etapa. Um PR por vez contra a `main`, sem empilhar branches.
  O merge é sempre do usuário.
- Depois de abrir o PR, conferir `gh pr view <n> --json state` antes de qualquer push: se já foi
  mergeado, o push recria a branch apagada.
- Antes de commitar, arquivar ou sincronizar specs, conferir `git branch --show-current`,
  `git status` e `git stash list`: o GitHub Desktop troca de branch e guarda alterações em stash
  sem aviso. **Nunca** aplicar ou descartar stash sem confirmação.
- Mensagem de commit: título em português, curto, sem prefixo de tipo e sem ponto final; corpo com
  o porquê e um tópico por projeto quando atravessar camadas. Detalhes em
  `.claude/commands/pull-request.md`.
- Commands, skills e hooks deste projeto ficam em `.claude/` do repositório, versionados.
