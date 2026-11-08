# MeuMoney

Aplicativo desktop pessoal de gestão financeira, com o **Money 99** como referência funcional e o
**Windows 11** como referência visual. Uso individual, não comercial, mantido por uma pessoa.

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
| Interface | WPF + **WPF-UI 4.3** (Fluent, `FluentWindow`, fundo Mica, tema do Windows) |
| Padrão de UI | MVVM com **CommunityToolkit.Mvvm 8.4** (`[ObservableProperty]`, `[RelayCommand]`) |
| Infraestrutura | Generic Host (`Microsoft.Extensions.Hosting`) e injeção de dependência |
| Persistência | **EF Core 10**, sem amarrar a um provedor |
| Banco | **SQLite**, em `%LOCALAPPDATA%\MeuMoney\meumoney.db` |
| Testes | **xUnit 2.9** |
| Planejamento | **OpenSpec** (fluxo spec-driven, artefatos em pt-BR) |
| Solução | `MeuMoney.slnx`; pacotes com versão central em `Directory.Packages.props` |
| IDE | Visual Studio 2026 (o VS 2022 não carrega projetos .NET 10) |

### Comandos

```
dotnet build MeuMoney.slnx
dotnet test MeuMoney.slnx
dotnet run --project src/Money.Desktop
dotnet ef migrations add <Nome> --project src/Money.Data.Sqlite --startup-project src/Money.Data.Sqlite
openspec list
```

A saída do SDK vem em português nesta máquina: decidir por código de saída, não pelo texto.

### Pacotes aprovados

`Microsoft.EntityFrameworkCore` (+ `.Relational`, `.Sqlite`, `.Design`),
`Microsoft.Extensions.DependencyInjection` (+ `.Abstractions`), `Microsoft.Extensions.Hosting`,
`WPF-UI`, `CommunityToolkit.Mvvm`, `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`.

**Pacote fora desta lista: explicar o motivo, o que ele fará e o custo, e esperar autorização.**
Aprovado, entra nesta lista e no `Directory.Packages.props` (versão nunca no `.csproj`).
Ainda sem decisão, a resolver no módulo que precisar: asserções/mocks de teste, validação de
entrada, gráficos e relatórios, importação de extrato (OFX/CSV), log em arquivo.

## Arquitetura

Camadas num único executável. A dependência aponta sempre para dentro:
`Money.Domain ← Money.Services ← Money.Data ← Money.Desktop`.

| Projeto | Responsabilidade | Depende de |
|---|---|---|
| `Money.Domain` | Entidades e regras de negócio | nada |
| `Money.Services` | Serviços por módulo, DTOs e as interfaces de repositório | Domain |
| `Money.Data` | EF Core: `MoneyDbContext`, mapeamentos, repositórios. Não conhece provedor | Services, Domain |
| `Money.Data.Sqlite` | Escolha do SQLite, migrations, fábrica de design-time | Data |
| `Money.Desktop` | WPF, ViewModels, composition root, escolha do provedor (`appsettings.json`) | Services, Data.Sqlite |

- Cada projeto expõe uma extensão `AddMoney*` (`AddMoneyServices`, `AddMoneyData`, `AddMoneySqlite`)
  numa classe `InjecaoDependencia`.
- Trocar para SQL Server = criar `Money.Data.SqlServer` no mesmo padrão e acrescentar o caso em
  `App.AdicionarBanco`. Nada mais muda.
- **Nenhum tipo de interface** (WPF, diálogos, caminhos escolhidos pelo usuário) em Domain, Services
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
- Ficam em inglês: termos técnicos consagrados (`DbContext`, `ViewModel`, `Dto`, `Id`, `Async`,
  `Fake`), nomes impostos pelo framework (`App`, `OnStartup`, `Dispose`, `CreateDbContext`), o
  parâmetro `cancellationToken`, os nomes de projeto e as extensões `AddMoney*`.
- PascalCase para tipos, métodos, propriedades e constantes; camelCase para locais e parâmetros;
  `_camelCase` para campos privados (é o que o `[ObservableProperty]` espera).
- Interfaces com `I`; métodos assíncronos com sufixo `Async`.
- Sufixos por papel: `ContaServico`, `IContaRepositorio`/`ContaRepositorio`, `ContaDto`,
  `ContaConfiguracao` (mapeamento EF), `ContasViewModel`.

### Estilo

- Namespace por arquivo (`namespace X;`), `using` fora do namespace, `System` primeiro.
- Chaves sempre, mesmo em `if` de uma linha. Indentação de 4 espaços (2 em XAML, JSON e `.csproj`).
- Um tipo público por arquivo, com o nome do arquivo igual ao do tipo.
- `sealed` em classe que não foi feita para herança; `internal` no que não sai do projeto.
- Construtor primário para injeção de dependência.
- Build **sem avisos**. Aviso de nullable se corrige, não se suprime com `!`.

### Comentários

- Toda classe e interface: `/// <summary>` curto dizendo para que serve.
- Todo método: `<summary>`, `<param>` para cada argumento e `<returns>` quando retorna valor.
  Em implementação de interface ou override, `/// <inheritdoc />`.
- Dentro do método, comentar só o **porquê** que não é evidente, no mínimo de palavras.
- Classes de teste dispensam `<summary>`: o nome do teste é a documentação.

### Regras de domínio

- Dinheiro sempre em **`decimal`**, nunca `double` ou `float`.
- Data e hora sempre por **`TimeProvider`** injetado, nunca `DateTime.Now`, `DateTime.UtcNow` ou
  `DateTimeOffset.Now`. Data sem hora (vencimento, competência) em `DateOnly`.
- Id das entidades: `Guid` v7, gerado na criação por `Entidade`.
- Entidade protege seu estado: setters privados, alteração por método com a regra
  (`conta.Renomear(nome)`), validação na própria entidade.

### WPF e MVVM

- Code-behind só liga a janela ao ViewModel (`DataContext`, `InitializeComponent`). Lógica fica no
  ViewModel; regra de negócio fica em Services.
- Propriedades com `[ObservableProperty]`, ações com `[RelayCommand]` (async quando chamar serviço).
- Usar os controles do WPF-UI antes de criar controle próprio: o pacote já resolve tema, ícones,
  navegação e diálogos.
- Textos exibidos em português, com acento.

### Testes

- Código novo de Domain e Services tem teste unitário. Persistência tem teste em `Money.Data.Tests`
  com SQLite em arquivo temporário (ver `ProvedorSqliteTestes`).
- Nome do teste em português: `Metodo_Cenario_ResultadoEsperado`.
- Teste nunca toca o banco real em `%LOCALAPPDATA%\MeuMoney`.

## Interface visual

O Money 99 diz **o que** cada tela faz: campos, regras, fluxo, informação exibida. **Como** ela
aparece segue o **Fluent Design do Windows 11**, no estilo dos apps nativos (Configurações,
Microsoft Store, Fotos). O visual do Money 99 (faixas azuis, menus em texto, clip-art, botões
cinza no rodapé, assistentes de várias janelas) é defasado e **não se reproduz**.

### Do Money 99 para o MeuMoney

| No Money 99 | No MeuMoney |
|---|---|
| Menu Arquivo/Editar + barra de texto (Conteúdo, Contas, Contas a pagar...) | `NavigationView` lateral, recolhível, com ícone e texto por módulo; Configurações no rodapé |
| Botão "Voltar" da barra | Histórico de navegação do `NavigationView` |
| Home page com faixas de título azuis | Painel de cartões: patrimônio total, próximas contas a pagar, contas favoritas |
| Gerenciador de contas com ícones grandes | Cartões agrupados por tipo (banco, cartão, investimento, patrimônio) com nome, instituição e saldo; total no topo |
| Botões no rodapé (Nova conta, Excluir conta...) | Barra de comandos no topo da página; ação principal com `Appearance="Primary"`; ações do item também no menu de contexto |
| Assistente com várias janelas Voltar/Avançar | Um `ContentDialog` ou página com formulário curto; campos opcionais em `CardExpander`. Passos só quando o fluxo exigir, com indicador de progresso |
| Lista de tipos com texto explicativo ao lado | Opções em cartões ou `ComboBox` com a descrição embaixo de cada item |
| Registro denso com abas Cheque/Depósito/Transferência | `DataGrid` limpo, valores à direita, saldo corrente; lançamento num painel lateral ou diálogo, com o tipo escolhido por botões segmentados |
| Ilustrações clip-art | Nenhuma ilustração; ícones Fluent (`ui:SymbolIcon Symbol="..."`) |
| Dica do dia, Anúncios, Consultor financeiro, Leitura via Web | Não reproduzir |

### Regras

- **Tema claro e escuro** acompanhando o Windows (`SystemThemeWatcher`), fundo Mica e cor de
  destaque do sistema. Toda tela deve funcionar nos dois temas.
- **Cor nunca fixa no XAML.** Usar os recursos do tema por `DynamicResource`:
  `TextFillColorPrimaryBrush`, `TextFillColorSecondaryBrush`, `CardBackgroundFillColorDefaultBrush`,
  `ControlStrokeColorDefaultBrush`, `AccentTextFillColorPrimaryBrush`.
- **Valor positivo e negativo**: `SystemFillColorSuccessBrush` e `SystemFillColorCriticalBrush`,
  sempre acompanhados do sinal ou de ícone (cor sozinha não basta).
- **Tipografia** pelos estilos do WPF-UI: `ui:TextBlock FontTypography="..."` com `Display`,
  `TitleLarge`, `Title`, `Subtitle`, `BodyStrong`, `Body` e `Caption`. Não definir `FontSize` nem
  `FontFamily` à mão.
- **Espaçamento em múltiplos de 4.** Margem da página 24, entre cartões 8 a 12, dentro do cartão 16.
- Cantos, sombras e animações ficam como o WPF-UI entrega: não sobrescrever `CornerRadius` nem
  criar estilo próprio de botão.
- **Valores monetários** formatados em pt-BR (`R$ 1.234,56`, cultura `pt-BR`) e alinhados à direita.
  Datas em `dd/MM/yyyy`.
- **Estado vazio** com mensagem e ação ("Nenhuma conta cadastrada" + botão "Criar conta").
- **Retorno ao usuário**: `Snackbar` para confirmar uma ação concluída, `ContentDialog` para
  confirmar exclusão, `InfoBar` para aviso que fica na página. Nunca `MessageBox` do Windows, exceto
  no erro fatal de inicialização.
- **Teclado**: toda ação alcançável sem mouse, atalho para as frequentes (Ctrl+N para novo) e
  `AutomationProperties.Name` em botão que só tem ícone.
- A janela tem largura mínima de 720: abaixo de ~1000 o `NavigationView` recolhe e o conteúdo se
  reorganiza, sem barra de rolagem horizontal.

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

- Antes de dar uma tarefa por concluída: `dotnet build` sem avisos e `dotnet test` passando.
- Validação visual e de uso do aplicativo é do usuário: a IA entrega o build pronto, diz o que
  conferir e espera o retorno.

### Git e fluxo

- Funcionalidade nova começa por change do OpenSpec: `/opsx:propose` → `/opsx:apply` →
  `/opsx:archive`. Arquivar e sincronizar specs **no fim do próprio change**, não depois.
- Nada é commitado direto na `main`. Uma branch por change, criada a partir da `main`, com o nome
  do change.
- **Commit, push e PR só com pedido.** Pedido dado, commit e push andam juntos; não terminar com
  commit só local.
- Fechamento da tarefa: `/pull-request`. Um PR por vez contra a `main`, sem empilhar branches.
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
