## MODIFIED Requirements

### Requirement: Editar conta
O sistema SHALL permitir alterar nome, instituição, grupo, saldo inicial, favorita e, no
investimento, a conta caixa, com as mesmas validações da criação. Os campos complementares também
SHALL poder ser alterados, todos opcionais:
- **Número da conta:** texto livre de até 50 caracteres;
- **Abreviação:** texto livre de até 20 caracteres;
- **Comentário:** texto livre de até 1.000 caracteres;
- **Saldo mínimo:** valor na moeda da conta, positivo, zero ou negativo, com no máximo as casas
  decimais da moeda. MUST existir só nas contas Corrente, Poupança e Dinheiro. Neste momento ele
  só é guardado e exibido, sem aviso nem bloqueio.

Os textos MUST ser gravados sem os espaços das pontas; texto só com espaços é gravado como vazio.
Texto acima do limite MUST ser recusado com a mensagem que informa o limite. O assistente de nova
conta MUST NOT pedir os campos complementares.

O tipo MUST NOT ser alterado após a criação. A moeda MUST NOT ser alterada enquanto a conta estiver
vinculada como investimento ou como caixa, nem enquanto a conta tiver lançamentos.

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

#### Scenario: Campos complementares gravados
- **WHEN** o usuário informa número "1234-5", abreviação "Cart", comentário "Uso diário" e saldo mínimo 50,00 numa conta Dinheiro e salva
- **THEN** ao abrir a conta de novo, os quatro valores aparecem como foram informados

#### Scenario: Saldo mínimo negativo
- **WHEN** o usuário informa saldo mínimo (R$ 500,00) numa conta Corrente
- **THEN** o sistema aceita e grava o valor negativo

#### Scenario: Saldo mínimo em tipo sem saldo mínimo
- **WHEN** um saldo mínimo é enviado para uma conta Cartão de crédito
- **THEN** o sistema recusa com a mensagem de que esse tipo de conta não tem saldo mínimo

#### Scenario: Saldo mínimo com casas demais
- **WHEN** o usuário informa saldo mínimo 10,005 numa conta em real
- **THEN** o sistema recusa com a mensagem do número máximo de casas decimais da moeda

#### Scenario: Abreviação longa
- **WHEN** o usuário informa uma abreviação com 21 caracteres
- **THEN** o sistema recusa com a mensagem de que a abreviação tem no máximo 20 caracteres

#### Scenario: Pop-up do gerenciador com os campos complementares
- **WHEN** o usuário escolhe Editar numa conta no gerenciador
- **THEN** o pop-up de edição mostra também número, abreviação, comentário e, nos tipos que o têm, o saldo mínimo

### Requirement: Página da conta
Abrir uma conta (duplo clique ou Enter no gerenciador, clique numa favorita do Início ou escolha no
menu "Contas") SHALL levar à página da conta, que tem:
- os comandos de editar, favoritar e encerrar ou reabrir; Editar MUST levar à visão Detalhes com o
  foco no primeiro campo editável;
- no painel, as visões e o menu "Contas", que lista as contas abertas para ir direto a outra, com o
  gerenciador no topo e as encerradas num submenu;
- o caminho de volta ao gerenciador.

As visões são Movimentação, Detalhes e Histórico:
- **Movimentação:** a conta Dinheiro MUST abrir nesta visão; nos demais tipos, ela MUST aparecer
  desativada, com a dica "Em breve", e a página abre em Detalhes.
- **Detalhes:** o formulário do cadastro da conta, em todos os tipos. Os campos são editáveis:
  nome, instituição, grupo, número da conta, moeda, saldo inicial (exceto no investimento), saldo
  mínimo (só Corrente, Poupança e Dinheiro), conta caixa (só no investimento), abreviação,
  comentário e favorita. O tipo, o saldo atual e a situação (aberta ou encerrada) ficam só para
  leitura.
  - Salvar e Desfazer MUST ficar desativados enquanto nada for alterado.
  - Salvar grava as alterações com as regras de "Editar conta" e confirma com um aviso
    passageiro. Recusa MUST aparecer dentro do formulário, que mantém os dados digitados.
  - Desfazer volta os campos aos valores gravados.
  - Alteração não salva é descartada ao trocar de visão ou de conta.
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

#### Scenario: Editar leva aos Detalhes
- **WHEN** o usuário está na Movimentação de uma conta Dinheiro e escolhe Editar na faixa de título
- **THEN** a visão Detalhes é exibida, com o foco no campo Nome

#### Scenario: Salvar nos Detalhes
- **WHEN** o usuário altera o nome da conta nos Detalhes para um nome válido e único e escolhe Salvar
- **THEN** o título da página passa a exibir o novo nome, o sistema confirma a alteração e Salvar volta a ficar desativado

#### Scenario: Recusa nos Detalhes
- **WHEN** o usuário apaga o nome da conta nos Detalhes e escolhe Salvar
- **THEN** a mensagem de nome obrigatório aparece no formulário, que continua com os dados digitados

#### Scenario: Desfazer nos Detalhes
- **WHEN** o usuário altera o comentário nos Detalhes e escolhe Desfazer
- **THEN** o comentário volta ao valor gravado e Salvar e Desfazer ficam desativados
