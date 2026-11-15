## Purpose

É a tela de entrada do aplicativo: mostra de relance quanto o usuário tem em cada moeda e o saldo
das contas que ele marcou como favoritas, com caminho direto para o gerenciador de contas.

## ADDED Requirements

### Requirement: Saldo total por moeda
A página Início SHALL exibir a data de hoje por extenso e o saldo total de cada moeda, somando as
contas abertas, com os mesmos valores do gerenciador de contas.

#### Scenario: Totais em duas moedas
- **WHEN** existem contas abertas em BRL e em USD
- **THEN** a página mostra um total em reais e um total em dólares, separados

### Requirement: Dívidas e distribuição por tipo
Havendo contas com saldo negativo na moeda principal, a página SHALL exibir o total das dívidas.
A página SHALL mostrar como o saldo positivo da moeda principal se divide entre os tipos de conta,
em barra proporcional com a porcentagem de cada tipo. A moeda principal é a primeira dos totais
(o real, quando houver conta em reais).

#### Scenario: Distribuição
- **WHEN** existem em reais uma conta corrente com 100,00 e uma poupança com 300,00
- **THEN** a barra mostra Conta corrente com 25% e Poupança com 75%

#### Scenario: Dívidas
- **WHEN** existe um cartão de crédito em reais com saldo devedor 500,00
- **THEN** a página mostra dívidas de −R$ 500,00

### Requirement: Contas favoritas
A página Início SHALL listar as contas abertas marcadas como favoritas, com ícone do tipo, nome,
instituição e saldo na moeda da conta. Acionar uma conta favorita MUST abrir a página da conta. Havendo contas mas nenhuma favorita, a página MUST orientar o usuário a marcar favoritas
em Contas.

#### Scenario: Favorita aparece no início
- **WHEN** o usuário marca uma conta como favorita e abre a página Início
- **THEN** a conta aparece na lista de favoritas com o saldo

#### Scenario: Favorita encerrada
- **WHEN** uma conta favorita é encerrada
- **THEN** ela deixa de aparecer na página Início

#### Scenario: Sem favoritas
- **WHEN** existem contas, mas nenhuma é favorita
- **THEN** a página mostra a orientação de como marcar uma conta como favorita

### Requirement: Primeiro uso
Sem nenhuma conta cadastrada, a página Início SHALL mostrar uma mensagem de boas-vindas e a ação
que leva ao gerenciador de contas para criar a primeira conta.

#### Scenario: Aplicativo sem contas
- **WHEN** o aplicativo abre sem nenhuma conta cadastrada
- **THEN** a página Início mostra as boas-vindas e o botão que abre Contas
