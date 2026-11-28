## Why

A conta Corrente é o próximo tipo do roteiro, depois da Dinheiro. A abertura dela no Money 99 pede
o número da conta e é o lugar natural para informar o limite (saldo mínimo). Hoje o assistente não
pergunta nenhum dos dois, e o nome repetido só é recusado no Concluir, longe do passo em que foi
digitado. Também ficou decidido que conta Dinheiro não tem saldo mínimo: ou o dinheiro existe ou
não existe.

Este é o primeiro de três changes da Corrente: A (abertura, este), B (movimentação da Corrente) e
C (aviso e exibição do saldo mínimo).

## What Changes

- Assistente de nova conta com o passo **Número da conta** (opcional), entre o nome e o saldo, só
  nos tipos que pedem número na criação; por ora, só a Corrente. Nos demais tipos o assistente
  continua com quatro passos.
- **Saldo mínimo** (opcional) no passo do saldo, nos tipos que têm saldo mínimo.
- O **Avançar do passo do nome** confere na API se já existe conta com o nome; se existir, o
  assistente fica no passo com a mensagem. A recusa no Concluir continua valendo.
- Rota nova na API para conferir o nome da conta.
- **BREAKING**: a conta **Dinheiro deixa de ter saldo mínimo**. O campo some dos Detalhes e do
  pop-up de edição dela, e o sistema recusa saldo mínimo enviado para ela. Uma migration limpa o
  saldo mínimo já gravado nas contas Dinheiro. Ficam com saldo mínimo a Corrente e a Poupança.
- Fica fora, como decidido: o passo "mais contas na mesma instituição" e a configuração on-line do
  Money 99. Detalhes e Histórico da Corrente já são os mesmos da Dinheiro e não mudam aqui.

## Capabilities

### New Capabilities

(nenhuma)

### Modified Capabilities

- `contas`: o assistente de criação ganha o passo do número e o saldo mínimo, e confere o nome no
  passo 3; o saldo mínimo passa a existir só na Corrente e na Poupança.

## Impact

- **Money.Domain**: `TipoContaExtensoes` (Dinheiro sai de `TemSaldoMinimo`; capacidade nova de
  pedir o número na criação).
- **Money.Services**: `TipoContaDto` expõe a nova capacidade; `ContaServico` ganha a conferência
  do nome.
- **Money.Api**: rota nova em `ContasEndpoints`.
- **Money.Data.Sqlite**: migration que limpa o saldo mínimo das contas Dinheiro (só dados; o
  esquema não muda). Precisa de revisão do usuário antes de aplicar.
- **Money.Web**: `NovaContaAssistente`, `CamposConta`, `useFormularioConta`, `api.ts`.
- **Testes**: Domain, Services, Api, Data e telas (`contas.spec.ts`, `detalhes.spec.ts`); os que
  usam saldo mínimo em conta Dinheiro passam a usar Corrente.
