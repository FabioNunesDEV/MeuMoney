## Context

O cadastro da conta é editado hoje por um pop-up, o `ContaDialogo`, aberto pelo comando Editar no
gerenciador e na página da conta. O pop-up monta as seções de `CamposConta.tsx` sobre o estado do
hook `useFormularioConta`, que também serve ao assistente de criação. A gravação passa por
`PUT /api/contas/{id}` com o `SalvarContaDto`, e o `ContaServico.AtualizarAsync` aplica as regras.
A visão Detalhes da `ContaPagina` é uma lista só de leitura (`<dl>`).

A entidade `Conta` não tem número, abreviação, comentário nem saldo mínimo. Acrescentá-los muda a
tabela `Contas`, que já guarda dados reais.

## Goals / Non-Goals

**Goals:**
- Um único formulário de edição, o mesmo estado e as mesmas seções, servindo ao pop-up e aos
  Detalhes. Só muda o que fica em volta: o pop-up com Cancelar e Salvar, os Detalhes com Desfazer e
  Salvar.
- Campos novos validados na entidade, como o resto do cadastro.

**Non-Goals:**
- Reproduzir o arranjo exato do Money 99 (rótulo à esquerda, um campo por linha, "Modificar..."
  para o nome). Os Detalhes usam as seções e o estilo de formulário que o MeuMoney já tem.
- Uso do saldo mínimo (aviso, destaque, relatório) e da abreviação fora do cadastro.
- Pedir os campos novos no assistente de criação.

## Decisions

### 1. Campos novos na entidade `Conta`

- `NumeroConta`, `Abreviacao` e `Comentario` (`string?`) e `SaldoMinimo` (`decimal?`), com setters
  privados.
- Dois métodos:
  - `DefinirComplementos(numero, abreviacao, comentario)`: tira os espaços das pontas, grava vazio
    como nulo e recusa texto acima do limite (50, 20 e 1.000 caracteres), com a mensagem do limite.
  - `DefinirSaldoMinimo(decimal? valor)`: recusa valor não nulo em tipo sem saldo mínimo e valida
    as casas decimais na moeda atual, com a mesma mensagem do saldo inicial.
- O tipo que tem saldo mínimo é dito por uma extensão nova, `TipoConta.TemSaldoMinimo()`, em
  `TipoContaExtensoes`.
  - Alternativa descartada: reaproveitar `PodeSerCaixa()`. Hoje os dois conjuntos coincidem
    (Corrente, Poupança e Dinheiro), mas são regras diferentes. Unir as duas faria uma mudar junto
    com a outra.
- No `AtualizarAsync`, o saldo mínimo é aplicado **depois** de `DefinirMoedaESaldo`. Assim, ao
  trocar a moeda, ele é validado nas casas da moeda nova.
- No `CriarAsync`, o serviço aplica os campos complementares e o saldo mínimo que vierem, pelo mesmo
  `AplicarCamposAsync`. O assistente simplesmente não os envia. Assim a criação pela API continua
  completa e não há dois caminhos.

### 2. DTOs e API

- `SalvarContaDto` e `ContaDto` ganham `NumeroConta`, `Abreviacao`, `Comentario` e `SaldoMinimo`
  (nulos quando não informados).
- As rotas não mudam.
- O `api.ts` espelha os campos.

### 3. Persistência

- `ContaConfiguracao` mapeia:
  - os três textos com `HasMaxLength` igual ao limite da entidade, que expõe os limites como
    constantes públicas, como o `TamanhoMaximoNome`;
  - o `SaldoMinimo` com `HasPrecision(18, 2)`, como o `SaldoInicial`.
- Migration `AdicionarDetalhesConta`: quatro colunas anuláveis em `Contas`, sem valor padrão. As
  contas existentes ficam com os quatro campos vazios. A migration vai para a revisão do usuário
  antes de seguir.

### 4. Formulário compartilhado nas telas

- O `useFormularioConta` ganha o estado dos quatro campos (o saldo mínimo como texto, lido com
  `lerValor`, como o saldo inicial) e passa a expor `alterado`. Para isso, compara os dados que
  seriam enviados com os da conta original. Na criação, `alterado` não é usado.
- O `CamposConta.tsx` ganha a seção `CamposComplementares`:
  - número e abreviação lado a lado;
  - o saldo mínimo, só nos tipos que o têm, alinhado à direita com algarismos de largura fixa;
  - o comentário numa área de texto.
- O `ContaDialogo` passa a mostrar essa seção depois do saldo.
- A tela pergunta à API (`api.opcoes`, nos dados do tipo) se o tipo tem saldo mínimo, em vez de
  repetir a lista de tipos no TypeScript. O `TipoContaDto` ganha `TemSaldoMinimo`.
  - Alternativa descartada: fixar a lista no `tiposConta.tsx`. A regra ficaria em dois lugares.
- Os Detalhes viram um componente próprio, `DetalhesConta.tsx` em `contas/`, que monta as mesmas
  seções do pop-up:
  - no topo, o cartão de hoje: selo do tipo, nome do tipo, situação e saldo atual, só leitura;
  - depois, as seções em blocos `Secao`: Conta (nome, grupo e favorita), Instituição, Saldo e moeda
    (ou conta caixa), Complementos. O nome vem primeiro, como no Money 99;
  - no fim, Desfazer e Salvar (Salvar na variante principal), desativados enquanto `alterado` for
    falso;
  - o `AvisoErro` logo acima dos botões, e a tela rola até ele quando aparece. Quem clica em Salvar
    está no fim de um formulário longo, e um aviso no topo ficaria fora da vista.
  - Alternativa descartada: um formulário novo no arranjo do Money 99. Duplicaria o pop-up, e o
    CLAUDE.md pede não duplicar.
- Desfazer e o recarregamento da conta remontam o formulário: a `ContaPagina` passa ao componente
  uma `key` feita da conta carregada mais um contador de Desfazer. Remontar é a forma mais simples
  de voltar todo o estado do hook aos valores gravados, sem uma função de "reset" campo a campo.
  - Efeito colateral aceito: favoritar ou encerrar pela faixa de título com alteração não salva
    descarta a alteração. A spec já diz que alteração não salva se descarta ao sair.
- O Editar da `ContaPagina` deixa de abrir o `ContaDialogo`. Ele troca a visão para Detalhes e pede
  o foco no Nome:
  - o foco é um pedido (`focarNome`) que a página liga no Editar. Os Detalhes focam o primeiro
    campo, que é o Nome, e avisam a página (`aoFocar`), que desliga o pedido. Assim salvar,
    desfazer ou voltar aos Detalhes pelo painel não roubam o foco. Funciona também quando os
    Detalhes já estão abertos;
  - a `ContaPagina` deixa de importar o `ContaDialogo`, que fica só no gerenciador (`Contas.tsx`).

### 5. Testes

- **Domain:** `DefinirComplementos` (espaços, vazio, limites) e `DefinirSaldoMinimo` (tipo, casas,
  valor negativo).
- **Services:** criar e alterar com os campos; saldo mínimo validado na moeda nova ao trocar a moeda.
- **Data:** ida e volta dos quatro campos no SQLite.
- **Api:** `PUT` e `GET` de ponta a ponta com os campos.
- **Tela** (`testes/detalhes.spec.ts`), com a conta criada pela API:
  - Editar leva aos Detalhes com o foco no Nome;
  - salvar os campos novos e conferir depois de trocar de visão e voltar;
  - recusa de nome vazio dentro do formulário;
  - Desfazer.

## Risks / Trade-offs

- [Migration numa tabela com dados reais] → Só colunas anuláveis, sem transformar dados.
  `IInicializadorBanco` aplica ao abrir o aplicativo. O usuário revisa o arquivo antes.
- [Formulário longo no pop-up do gerenciador] → O `Popup` já rola o conteúdo. A seção nova fica
  por último, sem empurrar os campos principais.
- [Alteração perdida ao trocar de visão sem salvar] → Comportamento declarado na spec. Salvar e
  Desfazer ativos deixam visível que há alteração pendente. Se incomodar no uso, a confirmação ao
  sair fica para um ajuste próprio.

## Migration Plan

1. Gerar `AdicionarDetalhesConta` com `dotnet ef migrations add` e mostrar o arquivo ao usuário.
2. Testes de persistência e de API com SQLite temporário.
3. Ao abrir o aplicativo, o `IInicializadorBanco` aplica a migration no banco real.

Desfazer antes de aplicar: apagar os arquivos da migration e restaurar o snapshot pelo git.
