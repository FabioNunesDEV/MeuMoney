## Why

Antes de começar os lançamentos (change B do roteiro da conta Dinheiro), a base do aplicativo tem
quatro lacunas:

- a barra de título do Windows fica clara mesmo com o tema escuro;
- as falhas não são registradas em lugar nenhum: o Serilog foi aprovado, mas nunca foi ligado;
- o build sem avisos é só um combinado: um aviso novo do StyleCop não impede a compilação;
- as telas não têm teste automatizado.

O change B vai ter fluxos longos de tela, como o teste de aceite com duas contas Dinheiro,
lançamentos e transferência. Esses fluxos precisam de teste automático desde o começo.

## What Changes

- A barra de título da janela passa a ter a cor da barra de módulos, nos dois temas, com texto e
  botões da janela claros. A cor vem da própria tela, que avisa a janela quando o tema muda.
- O Serilog passa a gravar as falhas num arquivo texto diário em `%LOCALAPPDATA%\MeuMoney\logs\`.
  Grava só a partir do nível de erro e apaga os arquivos antigos. Entram no log:
  - a falha de inicialização;
  - as exceções não tratadas da janela e das tarefas em segundo plano;
  - os erros que o próprio ASP.NET e o EF Core registram.
- Aviso de compilação passa a ser erro em todos os projetos (`TreatWarningsAsErrors`).
- Os testes de tela chegam com o `@playwright/test`, aprovado pelo usuário:
  - os testes abrem o aplicativo de verdade, com banco temporário;
  - os testes se ligam ao WebView2 da janela pela porta de depuração;
  - o primeiro teste cobre o assistente de criar conta em passos;
  - o comando é `npm run testar --prefix src/Money.Web`.
- A pasta de dados do aplicativo (dados do WebView2 e log) passa a ser configurável pela chave
  `PastaDados`. O padrão continua `%LOCALAPPDATA%\MeuMoney`. O teste aponta essa chave para uma
  pasta temporária e não toca nos dados do usuário.
- CLAUDE.md:
  - `@playwright/test` entra na lista de pacotes npm aprovados;
  - "testes das telas" sai do "ainda sem decisão";
  - a seção de testes e a validação passam a citar `npm run testar`;
  - fica registrado que o build sem avisos é garantido pelo `TreatWarningsAsErrors`.

## Capabilities

### New Capabilities

- `registro-de-falhas`: onde e como o aplicativo registra as falhas, o que entra no registro (só
  falhas) e por quanto tempo os arquivos ficam guardados.

### Modified Capabilities

- `navegacao`: novo requisito para a barra de título da janela acompanhar a cor da barra de
  módulos no tema em uso.

O `TreatWarningsAsErrors` e a infraestrutura de teste de tela não mudam comportamento do
aplicativo. Ficam no design e nas tarefas, sem spec.

## Impact

- `src/Money.Desktop`:
  - `App.xaml.cs` liga o Serilog e os tratadores de exceção não tratada;
  - `JanelaPrincipal.xaml.cs` recebe a pasta de dados e trata a mensagem de tema da tela;
  - uma classe nova pinta a barra de título pela API do DWM;
  - o `.csproj` referencia o Serilog;
  - o `appsettings.json` ganha a chave `PastaDados`.
- `src/Money.Web`:
  - `comum/tema.ts` avisa a janela da cor da barra a cada troca de tema;
  - os testes ficam em `testes/`, com a configuração do Playwright;
  - o `package.json` ganha `@playwright/test` e o script `testar`.
- `Directory.Build.props`: `TreatWarningsAsErrors`.
- `.gitignore`: `test-results/`, a pasta dos testes de tela.
- `CLAUDE.md`, conforme acima.
- Sem mudança de API, de banco ou de regra de negócio.
