## Context

- **Saldo.** A conta guarda só o `SaldoInicial`. O `ContaServico` monta o `ContaDto.Saldo` e os
  totais do gerenciador direto dele, e o Início usa o gerenciador.
- **Criação pelo nome.** Instituição e grupo são criados pelo nome, pelo `ResolverPorNomeAsync`
  privado do `ContaServico`, com unicidade verificada em memória por `Nomes.GarantirUnico`, sem
  índice único no banco.
- **Repositórios.** Há um `IRepositorio<T>` genérico (`ListarAsync`, `ObterPorIdAsync`,
  `Adicionar`, `Remover`) e a unidade de trabalho é o próprio `MoneyDbContext`.
- **Telas.** A página da conta (`ContaPagina.tsx`) mostra só os detalhes; Movimentação e Histórico
  aparecem desativados. Os menus (`MenuContexto`, `MenuBotao`) recebem uma lista plana de
  `AcaoMenu`, sem submenu nem item marcado.
- **Painel de tarefas.** Some abaixo de 1000 de largura. Hoje isso já esconde a troca de visão da
  página da conta.
- **Decimal no SQLite.** O provedor SQLite do EF grava `decimal` como texto e não traduz `Sum` de
  `decimal` para SQL.

Motivo e escopo estão em `proposal.md`. Os requisitos estão em `specs/lancamentos/spec.md` e
`specs/contas/spec.md`.

## Goals / Non-Goals

**Goals:**
- Um modelo de lançamento que já sirva aos outros tipos de conta quando forem liberados, sem
  reescrever a tabela. A restrição a Dinheiro fica numa regra só.
- Uma única fórmula de saldo, usada pelo gerenciador, pelos totais, pelo Início e pela Movimentação.
- A regra de quando pedir confirmação de saldo negativo fica no Services. A tela só mostra o pop-up.

**Non-Goals:**
- Classificar categoria como despesa ou receita: o tipo do lançamento já diz se é gasto ou
  recebimento.
- Reconciliação com extrato (Calc. saldo da conta) e divisão em várias categorias (Dividir).
- Paginação da grade. O volume de um usuário cabe na memória; ver Riscos.

## Decisions

### 1. Um lançamento por conta; a transferência é um par ligado por `TransferenciaId`

`Lancamento : Entidade`, em `Money.Domain/Lancamentos/`, tem:

| Propriedade | Conteúdo |
|---|---|
| `ContaId` | A conta do lançamento |
| `Tipo` | `TipoLancamento`: `Gasto`, `Recebimento`, `TransferenciaSaida` ou `TransferenciaEntrada` |
| `Data` | `DateOnly` |
| `Valor` | `decimal`, maior que zero, nas casas da moeda |
| `FavorecidoId` | O favorecido |
| `CategoriaId?`, `SubcategoriaId?` | Vazios na transferência |
| `Memo?` | Até 255 caracteres |
| `Situacao` | `SituacaoLancamento`: `NaoReconciliado`, `Compensado`, `Reconciliado` ou `Nulo` |
| `TransferenciaId?` | O mesmo Guid nas duas pontas de uma transferência |

O efeito no saldo é uma propriedade da entidade, `EfeitoNoSaldo`:
- `Gasto` e `TransferenciaSaida` subtraem o valor;
- `Recebimento` e `TransferenciaEntrada` somam;
- `Nulo` vale zero.

*Por que um registro por conta:* a grade de cada conta é uma consulta simples
(`WHERE ContaId = ?`). O saldo é a soma dos efeitos, e mover ou marcar uma ponta não exige casos
especiais. É também o modelo do Money: cada lado tem a própria situação (C/R).

*Por que `TransferenciaId` em vez de uma chave estrangeira para a outra ponta:* duas linhas que
apontam uma para a outra formam um ciclo. Na exclusão, o EF não consegue ordenar o `DELETE`, ou
seria preciso anular a chave antes. Um identificador de grupo, com índice e sem chave estrangeira,
liga as pontas sem ciclo.

*Alternativa:* entidade `Transferencia` própria, com conta de origem e de destino. Foi descartada
porque a grade da conta teria de unir duas fontes, e o saldo, somar dois tipos de registro.

### 2. Regras no domínio, orquestração no serviço

- `Conta.GarantirAceitaLancamento()` recusa conta encerrada ou que não seja Dinheiro. É o único
  ponto da restrição a Dinheiro; liberar outro tipo depois é mudar essa regra.
- Fábricas e métodos do `Lancamento`, cada um com sua regra:
  - `Lancamento.CriarGasto(conta, ...)` e `Lancamento.CriarRecebimento(conta, ...)`;
  - `Lancamento.CriarTransferencia(de, para, ...)`, que devolve as duas pontas e valida contas
    diferentes, mesma moeda e as duas aceitando lançamento;
  - `Alterar(conta, dados)`, em que `DadosLancamento` agrupa data, valor, favorecido, categoria,
    subcategoria e memo já resolvidos para Id (o mesmo objeto alimenta as fábricas);
  - `MarcarComo(situacao)`;
  - `MoverPara(atual, destino)`.
- `Favorecido`, `Categoria : EntidadeNomeada` e `Subcategoria : EntidadeNomeada` (esta com
  `CategoriaId`), na mesma pasta.
- `LancamentoServico`, em `Money.Services/Lancamentos/`, aplica as regras que envolvem mais de um
  registro:
  - nas transferências, alterar, anular e excluir valem para as duas pontas; tirar uma ponta de
    Nulo devolve a outra a Não reconciliado, para a transferência voltar a contar nas duas contas;
  - a unicidade de nomes;
  - a sugestão pelo último uso;
  - a verificação de saldo negativo.
- `ResolverPorNomeAsync` sai do `ContaServico` e vai para `Money.Services/Comum/Nomes.cs`, como
  método estático. Passa a servir a instituição, o grupo, o favorecido e a categoria, sem duplicar
  código. A subcategoria usa o mesmo método, filtrando as subcategorias da categoria.

### 3. Repositório de lançamentos com duas consultas próprias

`ILancamentoRepositorio : IRepositorio<Lancamento>`, declarado em Services e implementado em Data,
acrescenta três consultas:
- `ListarDaContaAsync(contaId)`: em ordem de `Data` e `Id`. O `Id` é Guid v7, então segue a ordem
  de registro.
- `SomarEfeitosPorContaAsync()`: devolve `IReadOnlyDictionary<Guid, decimal>`. Carrega os
  lançamentos sem rastreamento e soma `EfeitoNoSaldo` em memória, porque o SQLite não soma
  `decimal`.
- `ListarDaTransferenciaAsync(transferenciaId)`.

O `ContaServico` passa a receber o `ILancamentoRepositorio`:
- o saldo exibido é `SaldoInicial + soma` (o `SaldoInicial` já vem com o sinal do tipo);
- o mesmo valor alimenta `ContaDto.Saldo`, os totais do gerenciador e, por eles, o Início;
- `AtualizarAsync` recusa troca de moeda quando a conta tem lançamentos.

*Alternativa:* guardar o saldo na conta e atualizá-lo a cada lançamento. Foi descartada porque
duplica a informação, e qualquer caminho que esqueça de atualizar deixa o saldo errado para sempre.

### 4. Saldo negativo: gravar pede licença

`SalvarLancamentoDto` traz `ConfirmarSaldoNegativo`. Ao incluir ou alterar, o `LancamentoServico`:
1. Calcula, para cada conta afetada (a conta, ou as duas da transferência), o saldo atual e a
   variação: o efeito novo menos o efeito antigo. Na alteração, conta também a ponta que sai de uma
   conta e entra em outra.
2. Separa as contas com variação negativa e saldo final menor que zero.
3. Havendo alguma e sem confirmação, não grava e devolve
   `ResultadoLancamentoDto(Gravado: false, SaldosNegativos: [{ conta, saldoFinal, moeda }])`.
4. Com confirmação ou sem conta negativa, grava e devolve `Gravado: true`.

A tela mostra o pop-up de confirmação com a lista e, no Confirmar, reenvia com
`ConfirmarSaldoNegativo = true`.

*Por que 200 com `Gravado: false` e não um erro:* não é regra violada. Um 400 levaria a tela a
mostrar a mensagem como erro de formulário, e um código novo (409) criaria um segundo formato de
erro na API.

*Alternativa:* rota de prévia separada, chamada antes de gravar. Foi descartada porque faz duas
viagens em todo lançamento, e a regra de quando perguntar ficaria dividida entre as duas chamadas.

*Escopo:* só o Inserir pergunta. Mover, excluir e marcar como Nulo não perguntam. Foi o pedido
("ao clicar em Inserir").

### 5. API

| Rota | Serviço |
|---|---|
| `GET /api/contas/{id}/lancamentos` | `MovimentacaoDto`: a conta, as linhas com o saldo após cada uma e o saldo final |
| `GET /api/lancamentos/opcoes?contaId=` | Favorecidos com a sugestão do último uso; categorias com as subcategorias; contas Dinheiro abertas da mesma moeda (para Para e para Mover) |
| `POST /api/lancamentos` | Cria; devolve `ResultadoLancamentoDto` |
| `PUT /api/lancamentos/{id}` | Altera; devolve `ResultadoLancamentoDto` |
| `PUT /api/lancamentos/{id}/situacao` | Marca a situação |
| `POST /api/lancamentos/{id}/mover` | Move para outra conta |
| `DELETE /api/lancamentos/{id}` | Exclui; na transferência, as duas pontas |

- Enum trafega como texto, conforme o padrão do projeto.
- Na transferência, `ContaId` é a conta De e `ContaDestinoId` é a conta Para.
- O saldo linha a linha é calculado no serviço, junto com a ordem: a tela não soma dinheiro.

*Alternativa:* rotas próprias `/api/favorecidos` e `/api/categorias`. Foi descartada por agora:
sem cadastro próprio, tudo o que o formulário precisa vem numa chamada só (`opcoes`). As rotas
nascem com o módulo Categorias.

### 6. Banco

- Migration `AdicionarLancamentos`, que só cria tabelas novas:
  - `Favorecidos`, `Categorias`, `Subcategorias` e `Lancamentos`;
  - chaves estrangeiras com `Restrict` para a conta, o favorecido, a categoria e a subcategoria;
  - índices em `(ContaId, Data)`, `TransferenciaId` e `FavorecidoId`.
- Tipo e situação são gravados como texto, como em `Conta.Tipo`. Valor com `HasPrecision(18, 2)`,
  como `SaldoInicial`.
- Nenhuma tabela existente é alterada. O banco real recebe a migration na próxima abertura do
  aplicativo, pelo `IInicializadorBanco`, como as anteriores.

### 7. Telas

**Pastas e arquivos:**
- `src/Money.Web/src/lancamentos/`:
  - `Movimentacao.tsx`: a grade e o rodapé;
  - `FormularioLancamento.tsx`: as abas, os comandos e os campos;
  - `useFormularioLancamento.ts`: o estado do formulário (modo leitura, inclusão ou alteração),
    seguindo o padrão do `useFormularioConta`;
  - `acoesLancamento.ts`: o menu de contexto.
- Favorecido, categoria e subcategoria usam `<input list>` com `datalist`, como a instituição no
  assistente: escolhe da lista ou digita um nome novo.

**`ContaPagina.tsx`:**
- A visão (Movimentação ou Detalhes) fica num estado da página. Dinheiro abre em Movimentação e os
  demais tipos em Detalhes.
- A visão Detalhes passa a mostrar "Saldo inicial" com o `saldoInformado`, porque hoje ela mostra o
  saldo, que agora inclui os lançamentos, e ganha a linha "Saldo".
- O menu "Contas" é uma `Tarefa` no painel que abre um `MenuBotao`. Lista o gerenciador e as contas
  abertas; as encerradas vão num submenu.

**`comum/ui.tsx`:**
- `Tarefa` aceita as propriedades de botão, para servir de gatilho do menu "Contas".
- `Pagina` ganha `opcoesSoEstreita`: a barra de opções só aparece com o painel escondido.
- A `AcaoMenu` passa a ter ícone e ação opcionais (o item de submenu não tem os dois) e ganha três
  campos opcionais:
- `submenu`, para "Marcar como" e para as contas encerradas;
- `marcado`, para o ponto na situação atual;
- `desativado`, para as ações de conta encerrada.

Os dois menus continuam montados pela mesma função `itens`.

**Abaixo de 1000 de largura,** com o painel escondido, a barra de opções da página da conta mostra
a troca de visão e o menu "Contas". Assim toda ação continua alcançável na janela estreita.

**Saldo no formato contábil:**
- O componente `ValorSaldo`, em `comum/ui.tsx`, recebe valor, moeda e `forte` (negrito, ligado por
  padrão) e aplica a regra numa linha só:
  - valor positivo: `text-saldo-positivo`;
  - negativo: `text-negativo`, com o número entre parênteses;
  - zero: sem cor.
- `dinheiroContabil()`, em `formato.ts`, monta o texto a partir do `dinheiro()`: o valor absoluto,
  entre parênteses quando negativo. Também serve ao pop-up de saldo negativo.
- A grade usa `forte={false}` na coluna Saldo.

*Dor que o componente resolve:* a regra de cor está repetida em 8 pontos (`dinheiro(...)` com
`saldo < 0 && 'text-negativo'`), e o formato novo mudaria os 8 de novo.

**Variável `--saldo-positivo`:** é um azul próprio, com um tom para o tema claro e outro para o
escuro. Não reaproveita o `--realce` (azul-petróleo dos links e da seleção), para que o saldo ao
lado de um nome de conta clicável não pareça link.

O `--positivo` (verde, hoje sem uso nas telas) continua reservado para entradas e variações.

**Data padrão:** a data de hoje vem do navegador, no fuso da máquina. O `TimeProvider` vale para o
C#; a tela só sugere o valor, e o serviço valida a data recebida.

### 8. Teste de tela

`testes/lancamentos.spec.ts` segue o roteiro de aceite:
1. Cria duas contas Dinheiro pela API do aplicativo aberto, para não repetir o assistente, que já
   tem teste próprio: "Carteira" com 100,00 e "Cofre" com 0,00.
2. Pela tela, faz um gasto e um recebimento na "Carteira" e um recebimento no "Cofre".
3. Faz uma transferência da "Carteira" para o "Cofre".
4. Confere a grade e o saldo final das duas contas e o total no gerenciador.

Um segundo teste cobre o pop-up de saldo negativo: primeiro Cancelar, sem gravar nada; depois
Confirmar, com gravação.

A fixture `janela` passa a expor também o endereço da API (`enderecoDoAplicativo`, lido da URL da
página), para essa criação direta.

Nos testes de API, a montagem do servidor em porta livre com SQLite temporário sai de
`ContasApiTestes` para a base `TesteDeApi`, usada também por `LancamentosApiTestes`. Sem isso, as
mesmas 50 linhas seriam copiadas.

## Risks / Trade-offs

- **[Soma em memória fica lenta com muitos anos de lançamentos]** → Com algumas dezenas de milhares
  de linhas, a leitura sem rastreamento leva milissegundos no SQLite local. Se pesar, troca-se a
  implementação do repositório (por exemplo, valor em centavos inteiros, que o SQLite soma), sem
  mudar o contrato.
- **[Na mesma data, a ordem segue o `Id` (Guid v7), que só é cronológico ao milissegundo]** → O
  .NET preenche com sorteio o que vem depois do carimbo de tempo, e dois lançamentos criados no mesmo
  milissegundo podem trocar de ordem. Pela tela isso não acontece: cada Inserir é uma ação do
  usuário. O teste de persistência confere o contrato real (data, depois `Id`).
- **[Usuário digita favorecido com grafia diferente ("Padaria" × "padaria ")]** → A comparação
  ignora maiúsculas e espaços nas pontas e reaproveita o existente. Acento diferente cria outro
  favorecido; o cadastro de favorecidos virá com o módulo Categorias.
- **[Pergunta de saldo negativo depende do saldo final, não do saldo na data]** → Um lançamento
  retroativo pode deixar o saldo negativo num dia passado sem perguntar. É o mesmo critério do
  "Saldo final" do Money e foi o combinado.
- **[`AcaoMenu` com submenu muda um componente usado pelas contas]** → Os campos novos são
  opcionais, e os menus atuais não mudam de comportamento.
- **[Azul do saldo confundido com link]** → O tom é diferente do `--realce`, e o saldo nunca é
  clicável. A conferência visual da tarefa final olha as favoritas do Início, onde o nome-link fica
  ao lado do saldo.
- **[Data vinda do navegador em fuso diferente]** → A máquina é a mesma do servidor local; não há
  diferença de fuso entre tela e API.

## Migration Plan

1. A migration `AdicionarLancamentos` é gerada com `dotnet ef migrations add` e mostrada para
   revisão antes de qualquer execução.
2. Ela só cria tabelas. Ao abrir o aplicativo com o banco real, o `IInicializadorBanco` a aplica,
   sem alterar contas existentes.
3. Para voltar atrás antes do merge: apagar os arquivos da migration e restaurar o snapshot pelo
   git. Depois de aplicada no banco real, voltar atrás exige uma migration de remoção. Não é
   esperado.
