## Context

- `App.xaml.cs` monta o servidor local com `WebApplication.CreateBuilder` e abre a `JanelaPrincipal`.
  Uma falha na inicialização vira `MessageBox` e encerra o aplicativo, sem registro nenhum.
- `JanelaPrincipal` cria o ambiente do WebView2 numa pasta fixa,
  `%LOCALAPPDATA%\MeuMoney\WebView2`.
- O tema é resolvido na tela (`comum/tema.ts`), que grava `data-tema` no documento. A janela WPF não
  sabe qual é o tema.
- A cor da barra de módulos é a variável `--barra` de `estilo.css`, com um valor por tema
  (`#0c2f39` no claro, `#081d23` no escuro).
- Os pacotes do Serilog estão no `Directory.Packages.props`, mas nenhum projeto os referencia.
- O StyleCop vale nos dez projetos pelo `Directory.Build.props`, e o build atual tem 0 avisos.
- Regra de negócio recusada não chega a ser exceção não tratada: o `FiltroRegraNegocio` da API a
  converte em 400.

O motivo da mudança está em `proposal.md`. Os requisitos estão em `specs/navegacao/spec.md` e
`specs/registro-de-falhas/spec.md`.

## Goals / Non-Goals

**Goals:**
- Barra de título na cor da barra de módulos, sem repetir a cor no C#.
- Registro de falhas a partir da primeira linha de `OnStartup` em que já se pode ler a configuração.
- Testes de tela pela janela real, sem tocar no banco, no log ou nos dados do WebView2 do usuário.

**Non-Goals:**
- Levar ao log os erros de JavaScript das telas. Exigiria uma rota na API e fica para quando houver
  necessidade.
- Rodar os testes de tela dentro do `dotnet test` ou num CI.
- Testar a cor da barra de título por automação. A conferência é visual.

## Decisions

### 1. A tela informa a cor; a janela pinta a barra pelo DWM

O fluxo de ponta a ponta:

1. Em `tema.ts`, toda aplicação de tema chama `avisarJanela()`. Isso vale para a guardada ao
   abrir, para a troca pelo usuário e para a troca do Windows.
2. `avisarJanela()` lê `--barra` e `--sobre-barra` com `getComputedStyle` e envia
   `{ tipo: 'tema', corBarra, corTexto }`, as duas em `#rrggbb`, por
   `window.chrome.webview.postMessage`.
3. Fora do WebView2 (`npm run dev` no navegador), `window.chrome?.webview` não existe e o aviso não
   é feito.
4. A `JanelaPrincipal` assina `WebMessageReceived`, confere o `tipo`, converte `#rrggbb` e chama a
   classe interna `BarraTitulo`.
5. `BarraTitulo` usa `DwmSetWindowAttribute` com três atributos:
   - `DWMWA_CAPTION_COLOR` (35): a cor de fundo;
   - `DWMWA_TEXT_COLOR` (36): o título na cor de `--sobre-barra`;
   - `DWMWA_USE_IMMERSIVE_DARK_MODE` (20): botões claros.

As cores têm um só dono, o `estilo.css`. Se a paleta mudar, a barra de título acompanha sem tocar no
C#.

*Alternativa:* fixar no C# as duas cores e receber da tela só "claro" ou "escuro". Foi descartada
porque duplica a paleta, e a regra do projeto é cor só nas variáveis.

**Ao abrir:** a `JanelaPrincipal` liga `DWMWA_USE_IMMERSIVE_DARK_MODE` no `SourceInitialized`, antes
de a janela aparecer. Assim a barra nasce escura, sem clarão branco, e assume a cor exata quando a
tela carrega e manda o primeiro aviso.

**No Windows 10:** `DWMWA_CAPTION_COLOR` e `DWMWA_TEXT_COLOR` não existem. A chamada devolve erro e
o erro é ignorado. Fica valendo o modo escuro do sistema, conforme a spec.

**Mensagem inválida:** formato de cor desconhecido ou `tipo` diferente é ignorado. A mensagem vem
das nossas próprias telas, e um erro aí não deve derrubar a janela.

**Chamada ao DWM:** usa `[DllImport]`, com o valor passado como `ref int`.

*Alternativa:* `[LibraryImport]`, o padrão mais recente do .NET. Foi descartada porque exige
`AllowUnsafeBlocks` no `.csproj` do Desktop por causa de uma única chamada. A sugestão de trocar
(SYSLIB1054) é só informativa e não vira erro com o `TreatWarningsAsErrors`.

### 2. Serilog configurado em código, no composition root

- Só o `Money.Desktop` referencia `Serilog`, `Serilog.Extensions.Hosting` e `Serilog.Sinks.File`.
  Domain, Services, Data e Api continuam usando só `ILogger<T>`, da abstração da Microsoft.
- **Configuração em código, não no `appsettings.json`.** Ler a configuração exigiria o pacote
  `Serilog.Settings.Configuration`, que não está aprovado. O que varia é só a pasta, e ela vem de
  `PastaDados`.
- A ordem em `OnStartup`:
  1. `WebApplication.CreateBuilder(...)`.
  2. Resolver `PastaDados` (padrão `%LOCALAPPDATA%\MeuMoney`, com `Environment.ExpandEnvironmentVariables`).
  3. Criar o logger:
     - `MinimumLevel.Error()`, sem exceção para `Microsoft.*`;
     - `WriteTo.File(<pasta>\logs\meumoney-.log, rollingInterval: Day, retainedFileCountLimit: 30)`.
  4. `builder.Logging.ClearProviders()` e `builder.Services.AddSerilog(logger)`. O `ClearProviders`
     tira console, debug e EventLog, conforme "nada de console ou outro destino".
  5. O restante do `OnStartup`, como hoje.
- **O `App` registra as próprias falhas** com um `ILogger` tirado de
  `new SerilogLoggerFactory(logger)`. Esse tipo vem do `Serilog.Extensions.Logging`, que chega como
  dependência do `Serilog.Extensions.Hosting`. Assim vale a norma "log só com `LogError` ou
  `LogCritical`" também antes de o host estar montado.
- **Exceções não tratadas**, assinadas logo depois de criado o logger:

  | Origem | Registro |
  |---|---|
  | `AppDomain.CurrentDomain.UnhandledException` | `LogCritical` |
  | `TaskScheduler.UnobservedTaskException` | `LogError` |

  A exceção não tratada da janela não tem tratador próprio. Sem ninguém que a trate, ela encerra o
  processo e chega ao `AppDomain.UnhandledException`. Assinar também o `DispatcherUnhandledException`
  gravaria a mesma falha duas vezes.
- **O arquivo é gravado sem buffer** (padrão do `Serilog.Sinks.File`). Cada falha chega ao disco na
  hora, mesmo que o processo caia logo depois. Por isso os tratadores não precisam esvaziar o
  logger.
- **Falha antes do logger:** se `CreateBuilder` falhar (por exemplo, `appsettings.json`
  malformado), só sai o `MessageBox`. É o único trecho sem registro, e é aceito.
- **`OnExit`** esvazia o logger depois de parar o servidor.
- **`JanelaPrincipal.AoCarregar`:** a falha ao criar o WebView2 também vai para o log. A janela
  recebe o `ILogger` pelo construtor.

*Alternativa:* `Log.Logger` estático com `Log.Fatal`. Foi descartada porque a norma do projeto pede
`LogError`/`LogCritical` pela abstração.

### 3. `PastaDados`: uma chave para tudo o que é do usuário e não é o banco

- O `appsettings.json` ganha `"PastaDados": "%LOCALAPPDATA%\\MeuMoney"`.
- Dela saem `<PastaDados>\WebView2` (dados do navegador embutido) e `<PastaDados>\logs`.
- O banco continua na `Database:ConnectionString`, que já é configurável e é o padrão do projeto
  para provedor.

*Alternativa:* duas chaves (`PastaWebView2` e `PastaLogs`). Foi descartada porque, no teste e no
uso, as duas andam juntas. Uma chave só é mais fácil de entender.

### 4. Aviso vira erro

- `Directory.Build.props` ganha `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
- Vale para os avisos do compilador, do nullable, do StyleCop e do NuGet, inclusive o aviso de
  pacote com vulnerabilidade conhecida (NU190x).
- Esse último é desejado: o projeto não aceita aviso, e uma vulnerabilidade pede atualização do
  pacote, não silêncio.

### 5. Playwright pela janela real, por CDP

- **Pacote:** `@playwright/test` em `devDependencies` do `Money.Web`.
- **Sem download de navegador:** os testes só usam `chromium.connectOverCDP`, que não precisa dos
  navegadores do Playwright, e o `npx playwright install` não entra no fluxo.
- **Arquivos:**
  - `src/Money.Web/playwright.config.ts`:
    - `testDir: 'testes'`;
    - `workers: 1` e `fullyParallel: false`, para uma janela por vez;
    - `retries: 0`;
    - relatório só no terminal (`list`).
  - `src/Money.Web/testes/janela.ts`: a fixture `janela`.
  - `src/Money.Web/testes/node.d.ts`: os tipos mínimos do Node que a fixture usa.
  - `src/Money.Web/testes/contas.spec.ts`: o primeiro teste.
- **Fixture `janela`, com escopo de teste**, para cada teste começar com banco vazio:
  1. Cria a pasta do teste em `test-results/` (`info.outputPath`), acrescentada ao `.gitignore`, e escolhe uma porta livre para a depuração (abrindo um servidor na
     porta 0 e fechando).
  2. Sobe `src/Money.Desktop/bin/Debug/net10.0-windows/Money.Desktop.exe` com o ambiente:
     - `Database__ConnectionString=Data Source=<tmp>\meumoney.db`;
     - `PastaDados=<tmp>`;
     - `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS=--remote-debugging-port=<porta>`.

     Se o executável não existir, falha com "Compile antes: dotnet build MeuMoney.slnx".
  3. Espera `http://127.0.0.1:<porta>/json/version` responder (limite de 30 s), conecta por CDP e
     pega a página aberta (`contexts()[0].pages()[0]`). Espera a página sair de `about:blank` e
     mostrar a barra de módulos.
  4. Ao final:
     - se o teste falhou, grava a captura da janela em `janela.png`, na pasta do teste;
     - desconecta;
     - encerra a árvore de processos com `taskkill /PID <pid> /T /F`, porque o WebView2 tem
       processos filhos;
     - apaga a pasta do aplicativo, com novas tentativas por até 15 s, enquanto o SQLite e o WebView2
       liberam os arquivos.
- **Por que o código de produção não muda para a depuração:** o runtime do WebView2 lê
  `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS` sozinho, desde que o código não passe argumentos próprios.
  A porta só existe quando o teste a pede.
- **Tipos:** um `tsconfig` próprio em `testes/` (`types: []`, incluindo também o
  `playwright.config.ts`). O script `verificar` passa a checar os dois tsconfigs.
  - O `build` continua checando só `src`, para não acoplar o build do aplicativo aos testes.
  - `@types/node` não vem como dependência do `@playwright/test` e não está aprovado. Os poucos
    tipos de Node que a fixture usa ficam em `testes/node.d.ts`, que sai se o pacote for aprovado.
- **Primeiro teste:** "criar conta em passos".
  1. Abre Contas e clica em "Nova conta".
  2. Percorre os quatro passos: sem instituição, Dinheiro, nome "Carteira", saldo 50,00.
  3. Conclui.
  4. Confere a notificação e a conta "Carteira" com R$ 50,00 no gerenciador.

  Localizadores por papel e texto acessível (`getByRole`, `getByLabel`), os mesmos que o teclado e
  o leitor de tela usam.
- **Script:** `"testar": "playwright test"`. A validação de uma tarefa passa a ser
  `dotnet build` → `dotnet test` → `npm run testar --prefix src/Money.Web`.

*Alternativa:* Edge apontado para o servidor local com `Porta=5199`. Foi descartada a pedido do
usuário: a janela real cobre o WebView2, os cookies e a ligação com a janela.

*Alternativa:* fixture com escopo de worker, com o aplicativo aberto uma vez. Foi descartada porque
os testes passariam a depender da ordem e do banco deixado pelo anterior. O custo de alguns
segundos por teste é aceitável com poucos testes.

## Risks / Trade-offs

- **[Janela aparece durante os testes e pode receber cliques do usuário]** → Os testes são curtos, e
  um de cada vez; o usuário não deve usar o mouse enquanto rodam. A regra fica escrita no CLAUDE.md.
- **[Processo do aplicativo sobra se o teste for interrompido (Ctrl+C)]** → O `taskkill /T` fica no
  teardown. Num corte abrupto, sobra um `Money.Desktop.exe` com dados só na pasta temporária, sem
  risco para os dados do usuário.
- **[Atualização do runtime do WebView2 ou do Playwright quebra o `connectOverCDP`]** → É o caminho
  documentado pela Microsoft para automatizar o WebView2. Em caso de quebra, o teste falha na
  conexão, com mensagem clara, não com falso positivo.
- **[`--barra` deixar de ser hex no `estilo.css`]** → A janela ignora a cor que não entende e a
  barra fica no modo escuro do sistema. A regra "`--barra` em `#rrggbb`" fica num comentário ao
  lado da variável.
- **[`TreatWarningsAsErrors` quebra o build por aviso de vulnerabilidade publicado depois]** → É o
  comportamento desejado. A correção é atualizar o pacote.
- **[Pasta `logs` cresce sem limite num dia de erro em laço]** → O nível `Error` e um arquivo por
  dia limitam o volume. Um limite de tamanho por arquivo fica como melhoria futura, se acontecer.

## Migration Plan

Nada a migrar no banco. Na primeira abertura depois do change:
- o WebView2 continua na mesma pasta, porque o padrão de `PastaDados` é o caminho atual;
- a pasta `logs` só é criada na primeira falha.

Para voltar atrás, basta reverter o PR.
