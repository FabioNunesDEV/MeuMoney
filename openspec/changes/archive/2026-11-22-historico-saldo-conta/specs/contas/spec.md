## MODIFIED Requirements

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
- **Histórico:** na conta Dinheiro, o gráfico do saldo no tempo; nos demais tipos, MUST aparecer
  desativado, com a dica "Em breve".

#### Scenario: Abrir a conta
- **WHEN** o usuário dá duplo clique numa conta Dinheiro do gerenciador
- **THEN** a página da conta abre na Movimentação, com o saldo dela no rodapé

#### Scenario: Abrir conta de outro tipo
- **WHEN** o usuário dá duplo clique numa conta Corrente
- **THEN** a página abre em Detalhes, com a Movimentação e o Histórico desativados e a dica "Em breve"

#### Scenario: Ir para outra conta pelo menu Contas
- **WHEN** o usuário está na página da "Carteira" e escolhe "Cofre" no menu "Contas"
- **THEN** a página da conta "Cofre" é exibida

#### Scenario: Voltar ao gerenciador
- **WHEN** o usuário escolhe "Gerenciador de contas" na página da conta
- **THEN** o gerenciador de contas é exibido

#### Scenario: Histórico da conta Dinheiro
- **WHEN** o usuário escolhe Histórico na página de uma conta Dinheiro
- **THEN** a página mostra o gráfico do saldo da conta no tempo
