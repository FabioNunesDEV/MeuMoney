## 1. Aviso vira erro

- [x] 1.1 Acrescentar `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` ao `Directory.Build.props`; `dotnet build MeuMoney.slnx` com código de saída 0

## 2. Pasta de dados configurável

- [x] 2.1 Acrescentar `"PastaDados": "%LOCALAPPDATA%\\MeuMoney"` ao `appsettings.json` do Desktop e resolver a chave no `App` (com `Environment.ExpandEnvironmentVariables`)
- [x] 2.2 Passar a pasta à `JanelaPrincipal` e criar o ambiente do WebView2 em `<PastaDados>\WebView2`. Conferir em dois casos:
  - sem `PastaDados` no ambiente, o aplicativo abre com o tema guardado de antes, ou seja, na mesma pasta de hoje;
  - com `PastaDados` apontando para uma pasta temporária, o WebView2 é criado lá.

## 3. Registro de falhas (Serilog)

- [x] 3.1 Referenciar `Serilog`, `Serilog.Extensions.Hosting` e `Serilog.Sinks.File` no `Money.Desktop.csproj` (sem versão; elas já estão no `Directory.Packages.props`); build sem erro
- [x] 3.2 No `OnStartup`, logo depois do `CreateBuilder`, criar o logger e ligá-lo ao host:
  - nível mínimo `Error`;
  - arquivo diário `<PastaDados>\logs\meumoney-.log`, com retenção de 30 arquivos;
  - `ClearProviders` e `AddSerilog`.

  Build sem erro.
- [x] 3.3 Criar o `ILogger` do `App` (por `SerilogLoggerFactory`) e registrar:
  - a falha de inicialização antes do `MessageBox`;
  - as exceções de `AppDomain.UnhandledException` (que também recebe a não tratada da janela, pois ela encerra o processo) e de `TaskScheduler.UnobservedTaskException`.

  O arquivo é gravado sem buffer, então a falha chega ao disco antes de o processo cair; o logger é esvaziado no `OnExit`. Conferir com `Database__Provider=Inexistente` e `PastaDados` temporária: o arquivo do dia recebe a falha e o `MessageBox` aparece.
- [x] 3.4 Passar o `ILogger` à `JanelaPrincipal` e registrar a falha de criação do WebView2 antes do `MessageBox`; build sem erro
- [x] 3.5 Conferir com `PastaDados` temporária:
  - abrir o aplicativo, criar uma conta e tentar repetir o nome, que é recusado pela regra;
  - a pasta `logs` continua sem arquivo, conforme os cenários "Uso normal não gera registro" e "Regra de negócio recusada não é falha".

## 4. Barra de título

- [x] 4.1 Criar no Desktop a classe interna `BarraTitulo`, com `[DllImport]` de `DwmSetWindowAttribute`:
  - `AtivarModoEscuro(janela)`, pelo atributo 20;
  - `Pintar(janela, fundo, texto)`, pelos atributos 35 e 36, com as cores em `#rrggbb` (formato diferente é ignorado);
  - erro devolvido pelo DWM é ignorado (Windows 10).

  Build sem erro.
- [x] 4.2 Na `JanelaPrincipal`, ligar o modo escuro no `SourceInitialized` e assinar `WebMessageReceived`. Com a mensagem `{ tipo: 'tema', corBarra, corTexto }`, chamar `Pintar`; qualquer outra mensagem é ignorada. Build sem erro.
- [x] 4.3 Em `comum/tema.ts`, criar `avisarJanela()`:
  - lê `--barra` e `--sobre-barra` do documento e envia a mensagem por `window.chrome.webview.postMessage`, só quando o objeto existe;
  - tipagem mínima de `window.chrome.webview` declarada localmente;
  - é chamada em `aplicarTemaGuardado` e em toda aplicação de tema do `useTema`;
  - comentário em `estilo.css`: `--barra` e `--sobre-barra` ficam em `#rrggbb`, porque a janela lê essas cores.

  `npm run build --prefix src/Money.Web` sem erro.

## 5. Testes de tela (Playwright)

- [x] 5.1 Instalar `@playwright/test` em `devDependencies` do `Money.Web`, sem baixar navegadores, e criar o script `"testar": "playwright test"`. O relatório é só no terminal (`list`) e as capturas de falha vão para `test-results/`, acrescentada ao `.gitignore`. Conferir que `npx playwright --version` responde.
- [x] 5.2 Criar `playwright.config.ts`, `testes/tsconfig.json`, `testes/node.d.ts` (tipos mínimos do Node, sem `@types/node`) e `testes/janela.ts`, com a fixture `janela`, de escopo de teste:
  - pasta própria do teste em `test-results/` e porta livre de depuração;
  - `Money.Desktop.exe` sobe com `Database__ConnectionString`, `PastaDados` e `WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS`;
  - espera `/json/version`, depois `connectOverCDP`, e usa a página já aberta;
  - teardown com captura da janela se o teste falhou, desconexão, `taskkill /T /F` e remoção da pasta, com novas tentativas por até 15 s;
  - mensagem clara se o executável não existir.

  O script `verificar` passa a checar também `testes/tsconfig.json`; `npm run verificar --prefix src/Money.Web` sem erro.
- [x] 5.3 Criar `testes/contas.spec.ts`, "criar conta em passos":
  - Contas → Nova conta → sem instituição → Dinheiro → nome "Carteira" → saldo 50,00 → Concluir;
  - confere a notificação e a "Carteira" com R$ 50,00 no gerenciador;
  - localizadores por papel e rótulo.

  `npm run testar --prefix src/Money.Web` passando, com a janela abrindo e fechando sozinha.
- [x] 5.4 Rodar `npm run testar` duas vezes seguidas e conferir:
  - nenhum `Money.Desktop.exe` sobra no Gerenciador de Tarefas;
  - nenhuma pasta temporária do teste sobra;
  - o banco e os dados do usuário em `%LOCALAPPDATA%\MeuMoney` têm a mesma data de modificação de antes.

## 6. Documentação e verificação

- [x] 6.1 Atualizar o `CLAUDE.md`:
  - `@playwright/test` nos pacotes npm aprovados;
  - "testes das telas" fora do "ainda sem decisão";
  - linha de Testes da tabela Stack;
  - comando `npm run testar --prefix src/Money.Web` em Comandos;
  - a seção Testes descreve os testes de tela pela janela real (pasta `testes/`, fixture `janela`, não usar o mouse enquanto rodam);
  - Validação inclui `npm run testar`;
  - Estilo diz que o build sem avisos é garantido pelo `TreatWarningsAsErrors`;
  - Log cita a pasta `<PastaDados>\logs` e a retenção de 30 dias.

  Conferir no código cada chave e comando citado.
- [x] 6.2 `dotnet build MeuMoney.slnx` sem avisos, `dotnet test MeuMoney.slnx` passando e `npm run testar --prefix src/Money.Web` passando
- [x] 6.3 Entregar ao usuário a conferência visual da barra de título:
  - tema claro, escuro e do Windows (trocando o Windows);
  - abertura sem clarão branco;
  - título e botões legíveis nos dois temas.
