## Purpose

Mostra a evolução do saldo de uma conta ao longo do tempo, num gráfico em degrau como o "Movimentação
do saldo" do Money 99, para o usuário ver de relance quando o dinheiro entrou, saiu e onde o saldo
chegou.

## ADDED Requirements

### Requirement: Gráfico do saldo no tempo
A visão Histórico da conta Dinheiro SHALL mostrar o gráfico "Movimentação do saldo", com o período
indicado sob o título ("dd/MM/yyyy até dd/MM/yyyy"). O gráfico é uma linha em degrau:
- o saldo fica constante entre um dia e outro e muda no dia de cada lançamento;
- o valor de cada dia é o saldo da conta ao fim daquele dia: saldo inicial mais os lançamentos não
  nulos até ele, o mesmo da coluna Saldo da Movimentação;
- o eixo vertical mostra o saldo na moeda da conta, e o horizontal, as datas no formato dd/MM.

Passar o mouse ou o foco sobre o gráfico SHALL mostrar a data e o saldo do ponto. Quando o saldo
ficar negativo em algum momento do período, uma linha de referência MUST marcar o zero.

#### Scenario: Degraus nos dias dos lançamentos
- **WHEN** a conta tem saldo inicial 100,00, um gasto de 30,00 no dia 10 e um recebimento de 50,00 no dia 15, e o período vai do dia 1 ao dia 20
- **THEN** o gráfico fica em 100,00 do dia 1 ao 9, em 70,00 do dia 10 ao 14 e em 120,00 do dia 15 ao 20

#### Scenario: Lançamento nulo não muda o gráfico
- **WHEN** um gasto de 30,00 é marcado como Nulo
- **THEN** o gráfico não tem o degrau desse gasto

#### Scenario: Saldo negativo
- **WHEN** o saldo da conta fica em (R$ 20,00) em algum dia do período
- **THEN** o gráfico desce abaixo da linha do zero, que aparece marcada

### Requirement: Período do histórico
O usuário SHALL escolher o período numa lista junto ao título do gráfico: último mês, últimos 3 meses, últimos 6 meses,
últimos 12 meses e "Desde o primeiro lançamento". O padrão MUST ser os últimos 3 meses.
- O período termina hoje; havendo lançamento com data depois de hoje, termina na data dele.
- O gráfico MUST começar no primeiro dia do período com o saldo que a conta tinha naquele dia,
  incluindo os lançamentos anteriores ao período.
- Em "Desde o primeiro lançamento", o período começa na data do primeiro lançamento da conta.

#### Scenario: Período padrão
- **WHEN** o usuário abre o Histórico de uma conta Dinheiro em 22/11/2026
- **THEN** o período é 22/08/2026 até 22/11/2026

#### Scenario: Lançamentos antes do período
- **WHEN** a conta tem saldo inicial 100,00 e um recebimento de 50,00 há seis meses, e o período escolhido é o último mês
- **THEN** o gráfico começa em 150,00 no primeiro dia do período

#### Scenario: Lançamento com data futura
- **WHEN** existe um gasto com data 10 dias depois de hoje
- **THEN** o período vai até a data desse gasto e o degrau dele aparece no gráfico

### Requirement: Resumo do período
Acima do gráfico, a visão SHALL mostrar o saldo no início do período, o maior saldo, o menor saldo e
o saldo final do período, no formato contábil de saldo.

#### Scenario: Resumo
- **WHEN** no período o saldo começa em 100,00, chega a 120,00, desce a 70,00 e termina em 80,00
- **THEN** o resumo mostra início R$ 100,00, maior R$ 120,00, menor R$ 70,00 e final R$ 80,00

### Requirement: Histórico sem lançamentos
Conta sem nenhum lançamento SHALL mostrar no Histórico o estado vazio, com a mensagem de que o gráfico
aparece depois do primeiro lançamento e a ação de ir para a Movimentação.

#### Scenario: Conta nova
- **WHEN** o usuário abre o Histórico de uma conta Dinheiro sem lançamentos
- **THEN** aparece a mensagem com a ação "Ir para a Movimentação", e nenhum gráfico
