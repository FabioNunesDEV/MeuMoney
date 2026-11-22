## MODIFIED Requirements

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
- **THEN** a página mostra dívidas de (R$ 500,00), em vermelho e negrito
