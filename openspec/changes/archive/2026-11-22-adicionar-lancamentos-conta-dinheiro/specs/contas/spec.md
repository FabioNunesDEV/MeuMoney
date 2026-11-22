## MODIFIED Requirements

### Requirement: Moeda da conta
Cada conta SHALL ter uma moeda, escolhida de um catálogo identificado pelo código ISO 4217, com real
(BRL) como padrão. O catálogo MUST incluir pelo menos real (BRL), dólar americano (USD) e euro (EUR).
Saldos MUST ser exibidos na moeda da conta, com o símbolo dela e o formato numérico pt-BR, no
formato contábil:
- **positivo:** em azul e negrito (`R$ 1.234,56`);
- **negativo:** em vermelho e negrito, entre parênteses e sem sinal de menos (`(R$ 1.234,56)`);
- **zero:** na cor normal do texto.

Isso vale para todo saldo: o da conta, os totais por moeda, as dívidas, o saldo final da
Movimentação e a coluna Saldo da grade (nesta, com a cor mas sem negrito).

#### Scenario: Conta em dólar
- **WHEN** o usuário cria uma conta com moeda USD e saldo inicial 1234,5
- **THEN** o gerenciador exibe o saldo como US$ 1.234,50

#### Scenario: Casas decimais da moeda
- **WHEN** o usuário informa um saldo inicial com mais casas decimais do que a moeda da conta admite
- **THEN** o sistema recusa o valor

#### Scenario: Saldo positivo e negativo
- **WHEN** a "Carteira" tem saldo 1.250,00 e uma conta corrente tem saldo −320,00
- **THEN** a "Carteira" aparece com R$ 1.250,00 em azul e negrito, e a conta corrente com (R$ 320,00) em vermelho e negrito

#### Scenario: Saldo zero
- **WHEN** uma conta tem saldo 0,00
- **THEN** o saldo aparece como R$ 0,00 na cor normal do texto

### Requirement: Saldo inicial e sinal por tipo
O saldo exibido da conta SHALL ser o saldo inicial somado aos lançamentos não nulos da conta, com
recebimentos e entradas de transferência somando e gastos e saídas de transferência subtraindo. Sem
lançamentos, o saldo exibido é o próprio saldo inicial. O saldo exibido MUST valer no gerenciador,
nos totais por moeda, na página Início e na página da conta.

Para Cartão de crédito e Passivo, o usuário MUST informar o saldo devedor como valor positivo, e o
sistema MUST exibi-lo e somá-lo ao total como negativo. Para Ativo, o saldo inicial MUST NOT ser
negativo. Conta de Investimento MUST NOT ter saldo inicial próprio: seu saldo exibido é zero até
existirem posições.

#### Scenario: Cartão com dívida
- **WHEN** o usuário cria um Cartão de crédito com saldo devedor 500,00
- **THEN** o gerenciador exibe o saldo como negativo, (R$ 500,00) em vermelho e negrito, e o total em reais diminui 500,00

#### Scenario: Ativo negativo
- **WHEN** o usuário tenta criar um Ativo com saldo inicial negativo
- **THEN** o sistema recusa o valor

#### Scenario: Investimento sem saldo inicial
- **WHEN** o usuário escolhe o tipo Investimento e chega ao último passo do assistente
- **THEN** o campo de saldo inicial não é oferecido, a escolha da conta caixa aparece no lugar e a conta é criada com saldo zero

#### Scenario: Saldo com lançamentos
- **WHEN** uma conta Dinheiro com saldo inicial 100,00 tem um recebimento de 50,00, um gasto de 30,00 e um gasto nulo de 20,00
- **THEN** o gerenciador, o total da moeda, o Início e a página da conta usam o saldo 120,00

### Requirement: Editar conta
O sistema SHALL permitir alterar nome, instituição, grupo, saldo inicial, favorita e, no
investimento, a conta caixa, com as mesmas validações da criação. O tipo MUST NOT ser alterado após
a criação. A moeda MUST NOT ser alterada enquanto a conta estiver vinculada como investimento ou
como caixa, nem enquanto a conta tiver lançamentos.

#### Scenario: Renomear conta
- **WHEN** o usuário altera o nome de uma conta para um nome válido e único
- **THEN** o gerenciador passa a exibir o novo nome

#### Scenario: Moeda de conta vinculada
- **WHEN** o usuário tenta alterar a moeda de uma conta caixa vinculada a um investimento
- **THEN** o sistema recusa e indica o vínculo que impede a alteração

#### Scenario: Moeda de conta com lançamentos
- **WHEN** o usuário tenta alterar a moeda de uma conta que tem lançamentos
- **THEN** o sistema recusa com a mensagem de que a moeda não muda depois do primeiro lançamento

#### Scenario: Saldo inicial de conta com lançamentos
- **WHEN** o usuário altera o saldo inicial de uma conta Dinheiro que tem lançamentos
- **THEN** a alteração é aceita e o saldo de todas as linhas da Movimentação é recalculado

### Requirement: Página da conta
Abrir uma conta (duplo clique ou Enter no gerenciador, clique numa favorita do Início ou escolha no
menu "Contas") SHALL levar à página da conta, que tem:
- os comandos de editar, favoritar e encerrar ou reabrir;
- no painel, as visões e o menu "Contas", que lista as contas abertas para ir direto a outra, com o
  gerenciador no topo e as encerradas num submenu;
- o caminho de volta ao gerenciador.

As visões são Movimentação, Detalhes e Histórico:
- **Movimentação:** a conta Dinheiro MUST abrir nesta visão; nos demais tipos, ela MUST aparecer
  desativada, com a dica "Em breve", e a página abre em Detalhes.
- **Detalhes:** nome, tipo, instituição, moeda, saldo inicial, saldo, grupo, conta caixa do
  investimento, favorita e situação.
- **Histórico:** MUST aparecer desativado, com a dica "Em breve".

#### Scenario: Abrir a conta
- **WHEN** o usuário dá duplo clique numa conta Dinheiro do gerenciador
- **THEN** a página da conta abre na Movimentação, com o saldo dela no rodapé

#### Scenario: Abrir conta de outro tipo
- **WHEN** o usuário dá duplo clique numa conta Corrente
- **THEN** a página abre em Detalhes, com a Movimentação desativada e a dica "Em breve"

#### Scenario: Ir para outra conta pelo menu Contas
- **WHEN** o usuário está na página da "Carteira" e escolhe "Cofre" no menu "Contas"
- **THEN** a página da conta "Cofre" é exibida

#### Scenario: Voltar ao gerenciador
- **WHEN** o usuário escolhe "Gerenciador de contas" na página da conta
- **THEN** o gerenciador de contas é exibido
