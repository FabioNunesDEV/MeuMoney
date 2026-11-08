---
description: Fecha a tarefa — confere o change do OpenSpec, compila, testa, commita, envia e abre o pull request.
argument-hint: "[--sem-pr para preparar título e corpo sem abrir o PR]"
allowed-tools: Read, Write, Edit, Grep, Glob, Bash
---

# /pull-request

O command de fechamento do MeuMoney. O trabalho segue como quiser — branch e commits quando
quiser — e este command entra **no fim**, uma vez, quando a tarefa terminou.

Commit avulso não precisa dele: peça "commita" e a IA lê o diff, escreve a mensagem na
convenção do passo 6 e para, sem push e sem PR.

**Base:** `git symbolic-ref --short refs/remotes/origin/HEAD` (descarte `origin/`); em último
caso, `main`.
**Unidade de trabalho:** um change do OpenSpec em `openspec/changes/<nome>/`. Mudança pequena
sem change (correção pontual, ajuste de build) também pode ser fechada; nesse caso o PR não
cita change.

## 1. Conferir o estado do git antes de tocar em nada

```bash
git branch --show-current && git stash list && git status --short
git log --oneline <base>..HEAD
```

- **Stash existente:** pare e avise. **Nunca dê `pop` por conta própria.** Working tree limpa
  **com** stash é o pior caso: tudo parece pronto e o PR sairia sem o trabalho.
- **Branch é a base (`main`):** não se abre PR da base para ela mesma.
  - Só alterações não commitadas e nenhum commit à frente de `origin/main`: proponha criar a
    branch (`git switch -c <nome>`, que leva as alterações junto) e siga após o "sim".
  - Já há commits locais em `main` à frente de `origin/main`: **pare e pergunte.** Mover
    commits entre branches é decisão do dono do repositório.
- **Nome de branch sugerido:** o nome do change do OpenSpec (`adicionar-modulo-contas`); sem
  change, um slug curto em minúsculas, sem acentos, com hífens.

## 2. Identificar o change do OpenSpec

```bash
openspec list --json
```

Ordem para descobrir o change da tarefa: (1) change com o mesmo nome da branch; (2) o único
change ativo; (3) o change arquivado no diff (`git diff --name-only <base>...HEAD -- openspec/changes/archive/`);
(4) havendo vários candidatos, **pergunte** — não escolha.

Com o change em mãos:

- Leia `proposal.md` (o porquê e o título do PR) e `tasks.md`.
- **Tarefas abertas (`- [ ]`) em `tasks.md`:** mostre quais são e pergunte se o PR sai assim
  mesmo. Não bloqueie, mas o corpo do PR lista o que ficou aberto.
- **Change concluído e ainda ativo:** pergunte se quer arquivar antes do PR (`/opsx:archive`),
  para que specs e arquivamento entrem no mesmo PR. A decisão é do dev; não arquive sozinho.

## 3. Verificação

**Build e testes, se o diff tocar código ou build:**

```bash
git diff --name-only <base>...HEAD; git status --short   # comitado + pendente
```

Havendo arquivo em `src/`, `tests/`, `*.csproj`, `*.props`, `*.slnx` ou `appsettings*.json`:

```bash
dotnet build MeuMoney.slnx -nologo -v q
dotnet test MeuMoney.slnx --no-build -nologo
```

Use o **código de saída**, não o texto: a saída do SDK vem em português nesta máquina.

| Resultado | O que fazer |
|---|---|
| Build e testes ok | Registre `build: ok · testes: <n> aprovados` |
| Build falhou | **Pare o fechamento** e mostre os erros. Não abra PR com build vermelho |
| Teste falhou | **Pare o fechamento** e mostre os testes que falharam |
| `dotnet` ausente | Registre `build: indisponível`, escreva isso no PR e siga |

Diff só de documentação, OpenSpec ou `.claude/`: `build: não se aplica`.

**Migrations do EF Core.** Se o diff tocar entidades em `src/Money.Domain/` ou mapeamentos e
o `MoneyDbContext` em `src/Money.Data/`, confira se há migration nova em
`src/Money.Data.Sqlite/Migrations/`. Com `dotnet-ef` 8 ou superior instalado:

```bash
dotnet ef migrations has-pending-model-changes --project src/Money.Data.Sqlite --startup-project src/Money.Data.Sqlite
```

Com versão anterior ou sem a ferramenta, use o critério do diff: modelo alterado e nenhuma
migration nova é **alerta**. Mostre ao dev e pergunte antes de seguir; nunca gere migration
por conta própria.

**Regras de arquitetura do `openspec/config.yaml`.** Confira só nas linhas **adicionadas** do
diff e, achando algo, mostre o trecho e pergunte:

- `DateTime.Now`, `DateTime.UtcNow` ou `DateTimeOffset.Now` em qualquer projeto: o padrão é
  `TimeProvider`.
- `double` ou `float` em valor monetário: o padrão é `decimal`.
- Tipo de interface (`System.Windows`, `Wpf.Ui`, diálogos, caminhos de arquivo do usuário) em
  `Money.Domain`, `Money.Services` ou `Money.Data*`.
- Referência de projeto contra a direção `Domain <- Services <- Data <- Desktop`.
- Serviço em `Money.Services` devolvendo entidade em vez de DTO, ou método assíncrono sem
  `CancellationToken`.

**Teste manual — 2 perguntas.** Nunca pergunte "está testado?":

```
a) Você abriu o aplicativo e usou a funcionalidade como usuário?   (sim/não)
b) Ficou algo NÃO testado que valha registrar no PR?               (o quê)
```

Sendo (a) "não": **não bloqueie.** Escreva "não testado manualmente" no corpo do PR.

## 4. O que fica fora

Nunca entram no commit:

- `imagens_tela/` (prints de referência do Money 99, ignorados de propósito);
- arquivos de banco SQLite (`*.db`, `*.db-shm`, `*.db-wal`);
- `bin/`, `obj/`, `.vs/` e `appsettings.*.local.json` ou equivalente com dado pessoal.

Achando algum deles fora do `.gitignore`, avise e proponha incluir no `.gitignore`.

## 5. Commit

Liste, separe, `add` explícito. **Nunca `git add -A` às cegas.**

```bash
git status --short                  # leia e separe o que é da tarefa
git add <os arquivos da tarefa>
git commit -F <arquivo-da-mensagem>
```

Achando arquivo que não parece da tarefa, **mostre e pergunte**. Nada pendente? Não crie
commit vazio; siga.

## 6. Convenção de mensagem

Siga o histórico do repositório (`git log -5`):

- **Título** em português, curto, no infinitivo ou como frase nominal, sem prefixo de tipo e
  sem ponto final: `Adicionar cadastro de contas`, `Corrigir saldo inicial negativo`.
- Linha em branco e **corpo** explicando o porquê, com tópicos por projeto quando a mudança
  atravessar camadas (`- Money.Domain: ...`, `- Money.Data.Sqlite: ...`).
- Citando o change: `Change OpenSpec: <nome>`.
- Termine com a linha de atribuição do Claude Code indicada pela sessão.

Grave a mensagem num arquivo temporário e use `git commit -F`, para preservar a formatação.

## 7. Push

```bash
git push -u origin HEAD
```

Push recusado (non-fast-forward): **pare e relate.** Nunca `--force` sem pedido explícito.

## 8. Título do pull request

Em português, no mesmo estilo do título de commit. Origem, nesta ordem:

1. o título do `proposal.md` do change;
2. o título do commit principal da branch;
3. resumo do diff, mostrado ao dev para confirmação.

```
Adicionar cadastro de contas
```

## 9. Abrir o PR

```bash
git diff --stat <base>...HEAD
gh pr list --head "<branch>"     # já existe PR aberto?
gh pr create --base <base> --head "<branch>" --title "<título>" --body-file "<arquivo>"
```

Grave o corpo num arquivo temporário e use `--body-file`, para preservar a formatação.

```markdown
## Resumo
<2 a 4 linhas: o que muda para quem usa o aplicativo e por quê>

## Change OpenSpec
`<nome>` — <ativo | arquivado neste PR> · tarefas: <x/y concluídas>
<tarefas abertas, se houver>
<ou "Sem change: <motivo>">

## Verificação
Build: <ok | falhou | indisponível | não se aplica> · Testes: <n aprovados | não se aplica>
Migrations: <nova migration `<nome>` | sem mudança de modelo | ⚠️ modelo alterado sem migration>

## Teste manual
<o que foi usado no aplicativo, o que ficou de fora; ou "não testado manualmente">

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

- **PR já aberto para a branch? Não crie outro.** Informe a URL e pergunte se quer atualizar
  título e corpo (`gh pr edit`).
- **`gh` ausente ou não autenticado: não invente sucesso.** Relate o erro exato e entregue o
  comando pronto. `gh auth login` é interativo; sugira rodar com `! gh auth login`.
- **"No commits between..."**: a branch provavelmente já foi integrada. **Não recrie a
  branch**; relate e pergunte.
- Com `--sem-pr`, entregue título e corpo prontos e **pare aqui**.

**Falha que você não entende é hora de parar e perguntar**, não de tentar o próximo comando.

## 10. Resumo

Três linhas, no máximo: PR nº e URL · branch → base com o volume do diff · change do OpenSpec
e situação da verificação (build, testes, migrations).
