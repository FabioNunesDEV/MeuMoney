## Context

- O `LancamentoServico` já monta a Movimentação com o saldo linha a linha: o saldo inicial da conta
  somado ao `EfeitoNoSaldo` de cada lançamento (nulo vale zero), em ordem de data e registro.
- O `ILancamentoRepositorio.ListarDaContaAsync` traz os lançamentos da conta nessa ordem.
- A página da conta (`ContaPagina.tsx`) troca entre Movimentação e Detalhes; o Histórico é uma
  `Tarefa` "Em breve". As cores vêm das variáveis do `estilo.css`, com valores por tema.
- O C# ainda não usa `TimeProvider`: nenhuma regra até aqui dependeu de "hoje".
- O `recharts` 3.11 aceita React 19 e tem o `react-is` como dependência de par, que o npm instala
  junto.

Motivo e escopo estão em `proposal.md`. Os requisitos estão em `specs/historico-saldo/spec.md` e
`specs/contas/spec.md`.

## Goals / Non-Goals

**Goals:**
- O gráfico usa o mesmo cálculo de saldo da Movimentação, feito no serviço. A tela só desenha.
- O `recharts` não pesa na abertura do aplicativo: ele só é carregado quando o usuário abre um
  Histórico.

**Non-Goals:**
- Histórico de outros tipos de conta: eles ainda não têm lançamentos.
- Período livre, com datas escolhidas pelo usuário: ficam as cinco opções fixas.
- Gráfico de várias contas juntas, ou por categoria: será assunto do módulo Relatórios.

## Decisions

### 1. O serviço devolve os pontos do degrau

`LancamentoServico.ObterHistoricoAsync(contaId, de, ate)` devolve
`HistoricoSaldoDto(ContaId, De, Ate, Pontos, SaldoInicioPeriodo, Maior, Menor, Final, TemLancamentos)`.

| Elemento | Como é calculado |
|---|---|
| Saldo no início | Saldo inicial da conta mais os lançamentos com data anterior a `de` |
| `Pontos` | Primeiro `(de, saldo no início)`. Depois, para cada data entre `de` e `ate` com lançamento, `(data, saldo ao fim do dia)`. Por fim, `(ate, saldo final)`, se o último ponto for de outra data |
| Maior, menor | O maior e o menor saldo entre os pontos |
| Final | O saldo do último ponto |

**Parâmetros:**
- **`de` vazio** ("Desde o primeiro lançamento"): vale a data do primeiro lançamento da conta.
- **`ate`:** é a data que a tela informa (hoje). O serviço o estende até o último lançamento, se
  houver lançamento com data futura.
- **Conta sem lançamento:** volta com `TemLancamentos = false` e sem pontos.

A regra "só conta Dinheiro" não é repetida aqui: as outras contas não têm lançamento e cairiam no
estado vazio. A tela nem oferece o Histórico a elas.

*Por que no serviço:* o saldo linha a linha já está lá, e a regra do projeto é que a tela não soma
dinheiro.

*Alternativa:* a tela montar os pontos a partir da Movimentação que ela já carrega. Foi descartada
porque duplica o cálculo do saldo em TypeScript.

### 2. "Hoje" vem da tela

A tela calcula o período com a data da máquina (`hojeIso`) e manda `de` e `ate` na rota
`GET /api/contas/{id}/historico?de=yyyy-MM-dd&ate=yyyy-MM-dd`. O `de` é opcional.

*Por que não `TimeProvider` no serviço:* a lista de períodos é da tela ("últimos 3 meses" a partir
de hoje). O serviço só recebe um intervalo, sem saber o que é hoje, e os testes ficam sem relógio. A
regra do `TimeProvider` continua valendo quando o C# precisar de "agora".

### 3. Gráfico com `recharts`, carregado sob demanda

**Arquivo e carregamento:**
- `src/Money.Web/src/lancamentos/Historico.tsx` usa `ResponsiveContainer`, `LineChart`,
  `CartesianGrid`, `XAxis`, `YAxis`, `Tooltip`, `ReferenceLine` e `Line type="stepAfter"`.
- A `ContaPagina` importa o `Historico` com `React.lazy` e mostra "Carregando…" no `Suspense`. O
  Vite separa o `recharts` num arquivo próprio, que só é baixado na primeira abertura de um
  Histórico.
- Sem isso, o arquivo único das telas passaria de 500 KB, e o Vite avisaria no build.

**Eixos:**
- **Horizontal:** a data como número (dias), em escala proporcional, para que um intervalo de 20
  dias sem lançamento ocupe o espaço de 20 dias. Os rótulos são dd/MM.
- **Vertical:** o saldo, com rótulos no formato contábil curto da moeda, sem centavos
  (`R$ 1.500`).

**Destaques:**
- **Dica:** `Tooltip` próprio, com a data dd/MM/yyyy e o `ValorSaldo`.
- **Zero:** `ReferenceLine y={0}` tracejada em `--negativo`, só quando `Menor < 0`.

**Cores:** vêm do `estilo.css`. A linha usa `--realce`; grade e eixos usam `--borda` e `--suave`.
- O `recharts` grava as cores como atributos do SVG, onde `var()` não é garantido. Por isso o
  componente lê o valor resolvido de cada variável com `getComputedStyle`.
- A leitura se repete a cada troca de tema: um `MutationObserver` acompanha o `data-tema` do
  documento.
- A cor continua só no `estilo.css`.

O âmbar fica de fora, porque é reservado à ação principal.

**Seletor de período:** um `<select>` junto ao título "Movimentação do saldo", com as cinco opções.
A escolha fica no estado da página e volta ao padrão (3 meses) ao trocar de conta.

### 4. Página da conta

- `Visao` passa a ter `'historico'`.
- A `Tarefa` Histórico fica ativa na conta Dinheiro e "Em breve" nos demais tipos, como a
  Movimentação.
- A troca de visão da barra de opções (janela estreita) ganha o botão Histórico.
- O rodapé mostra "Saldo:" com o saldo atual, como em Detalhes.

## Risks / Trade-offs

- **[Conta com muitos anos de lançamentos em "Desde o primeiro lançamento"]** → O gráfico recebe
  um ponto por dia com lançamento, poucos milhares no pior caso, o que o `recharts` desenha sem
  esforço. O serviço já lê a conta inteira para a Movimentação.
- **[Peso do `recharts` (o arquivo do Histórico tem cerca de 123 KB compactados)]** → Carregamento sob demanda: quem não abre o
  Histórico não paga.
- **[Fuso da máquina diferente do esperado]** → Tela e servidor rodam na mesma máquina, e "hoje" é
  só o fim padrão do período.
