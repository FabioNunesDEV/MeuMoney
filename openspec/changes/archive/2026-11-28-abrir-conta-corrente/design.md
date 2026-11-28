## Context

O assistente (`NovaContaAssistente.tsx`) tem uma lista fixa de quatro passos e usa o estado de
`useFormularioConta`, o mesmo hook dos Detalhes e do pop-up de edição. O número da conta e o saldo
mínimo já existem na entidade `Conta` (colunas da tabela `Contas`, comuns a todos os tipos) e nos
campos complementares (`CamposComplementares`), mas só na edição. O que cada tipo usa é decidido por
extensões de `TipoConta` (`TemSaldoMinimo`, `PodeSerCaixa`, `RepresentaDivida`), que o
`TipoContaDto` repassa à tela.

A unicidade do nome é garantida por `Nomes.GarantirUnico` no `ContaServico.CriarAsync`; a validação
de vazio e de tamanho fica em `EntidadeNomeada`, num método privado.

O `Tipo` é gravado como texto (`HasConversion<string>`), então a conta Dinheiro é `Tipo = 'Dinheiro'`
no banco.

## Goals / Non-Goals

**Goals:**
- O que cada tipo pede na criação é decidido por capacidade do tipo, num lugar só, sem `if` de tipo
  na tela.
- O assistente, os Detalhes e o pop-up continuam usando os mesmos campos (sem copiar componente).

**Non-Goals:**
- Tabela ou entidade por tipo de conta: todas as contas continuam na mesma tabela e entidade.
- Aviso de saldo mínimo nos lançamentos e exibição do limite (change C).
- Passo de número para outros tipos (Poupança, Cartão...): entra quando o tipo for trabalhado,
  ligando a capacidade.

## Decisions

### 1. Capacidade nova `PedeNumeroNaCriacao` em `TipoContaExtensoes`
Ela devolve `true` só para a Corrente e vai à tela pelo `TipoContaDto`, como o `TemSaldoMinimo`.
O número continua editável nos Detalhes de qualquer tipo: a capacidade diz só se o **assistente**
pergunta.
- *Alternativa*: reaproveitar `TemSaldoMinimo` para mostrar o número. Recusada porque mistura duas
  perguntas diferentes, e a Poupança passaria a pedir número sem ter sido decidido.

### 2. `TemSaldoMinimo` passa a ser Corrente e Poupança
Essa mudança tira o campo da Dinheiro em todos os lugares de uma vez: no assistente, nos Detalhes, no
pop-up e na regra do domínio (`Conta.DefinirSaldoMinimo` já recusa os tipos sem saldo mínimo).

### 3. Passos com condição no assistente
Cada `Passo` ganha um `aplica?: (f) => boolean` opcional. Os passos visíveis são os que se aplicam ao
tipo atual, e o índice e o "Passo x de n" correm sobre essa lista. O passo do número usa
`f.infoTipo?.pedeNumeroNaCriacao`. O tipo só se escolhe no passo 2, então a lista só muda antes do
passo 3: o índice atual nunca aponta para um passo que sumiu.
- *Alternativa*: um passo que esconde o campo e mostra "nada a informar". Recusada porque é um passo
  inútil para o usuário.

### 4. Número e saldo mínimo do assistente vêm por campos compartilhados
`CamposComplementares` é dividido em peças exportadas: `CampoNumeroConta` e `CampoSaldoMinimo`
(este já condicionado a `temSaldoMinimo`). O assistente usa as duas peças, e os Detalhes e o pop-up
continuam usando `CamposComplementares`, que passa a montá-las.

No `salvar` de uma conta nova, o número só vai se o tipo pede o número na criação (o saldo mínimo já
só vai se o tipo o tem). Assim o valor digitado antes de trocar o tipo não é gravado. Na edição, o
número vai sempre, como hoje.

### 5. Conferência do nome: `GET /api/contas/nome-disponivel?nome=...`
`ContaServico.ConferirNomeAsync(nome, ct)` aplica as mesmas regras da criação: vazio e tamanho, pela
validação do `EntidadeNomeada` (o método passa de privado a `public static`), e unicidade, pelo
`Nomes.GarantirUnico`. Nome aceito → `200` sem corpo, como as demais rotas sem retorno; recusado →
`400 { mensagem }`, o padrão da API. No `api.ts`
fica `conferirNomeConta(nome)`, que lança `ErroRegra`. O passo 3 ganha uma `conferencia` assíncrona,
rodada no Avançar depois da `pendencia` local. A recusa vai para `f.setErro` e o assistente não avança.
- *Alternativa*: conferir na tela com a lista de contas. Recusada porque a regra de negócio fica em
  Services, e a tela não tem as contas encerradas.
- *Alternativa*: voltar ao passo 3 quando o Concluir recusar o nome. Recusada porque o usuário pediu
  o bloqueio no Avançar. A recusa no Concluir continua como rede de segurança (e cobre a corrida).

### 6. Migration só de dados: `LimparSaldoMinimoDinheiro`
Ela é gerada com `dotnet ef migrations add` (o modelo não muda, então o `Up` nasce vazio). No `Up`
entra `migrationBuilder.Sql("UPDATE Contas SET SaldoMinimo = NULL WHERE Tipo = 'Dinheiro'")`. O `Down`
fica vazio, com um comentário: o valor apagado não volta. O usuário revisa o arquivo antes do primeiro
uso.

## Risks / Trade-offs

- [O saldo mínimo de contas Dinheiro do banco real é apagado sem volta] → decisão do usuário. A
  migration só toca a coluna `SaldoMinimo` das linhas `Tipo = 'Dinheiro'` e é revisada antes. O
  backup do banco fica com o usuário.
- [Uma chamada à API a mais no passo 3] → é local (127.0.0.1) e só acontece no Avançar desse passo.
- [A contagem de passos muda depois do passo 2] → só muda antes de o usuário passar dele, e a barra
  de progresso mostra a contagem nova na hora.

## Migration Plan

1. Gerar a migration e mostrar ao usuário para revisão.
2. Ela roda ao iniciar o aplicativo (`IInicializadorBanco`), como as demais.
3. Para voltar atrás no código, basta reverter o commit. O saldo mínimo apagado das contas Dinheiro
   só volta por backup.
