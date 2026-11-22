# contas Specification

## Purpose
Permite ao usuário cadastrar e organizar as contas onde guarda dinheiro, dívidas, investimentos e
bens, em qualquer moeda, e ver o saldo de cada uma e o total por moeda num gerenciador.

## Requirements

### Requirement: Tipos de conta com comportamento fixo
O sistema SHALL oferecer exatamente estes tipos de conta, cada um com uma descrição curta exibida na
escolha: Corrente, Poupança, Dinheiro, Cartão de crédito, Investimento, Ativo e Passivo. O usuário
MUST NOT poder criar, renomear ou excluir tipos.

#### Scenario: Escolha do tipo mostra a descrição
- **WHEN** o usuário chega ao passo de tipo do assistente de nova conta e escolhe um tipo
- **THEN** os sete tipos aparecem em lista e a descrição do tipo escolhido fica visível ao lado

### Requirement: Criar conta
O sistema SHALL criar uma conta por um assistente em quatro passos, nesta ordem:
1. **Instituição**: a conta é mantida numa instituição, escolhida da lista ou digitada como nome
   novo, ou não é mantida em instituição;
2. **Tipo**: um dos tipos de conta, com a descrição do escolhido;
3. **Nome**: nome da conta, com grupo e favorita opcionais;
4. **Saldo e moeda**: saldo inicial e moeda; no Investimento, a conta caixa no lugar do saldo.

O assistente MUST oferecer Voltar (exceto no primeiro passo), Avançar (Concluir no último) e
Cancelar. Voltar MUST preservar o que já foi informado. Avançar MUST NOT sair do passo 1 com a
opção de instituição marcada e o nome da instituição vazio, nem do passo 3 com o nome da conta vazio.
A conta MUST ser gravada só ao concluir; se o sistema recusar os dados, a mensagem MUST aparecer no
assistente, que permanece aberto com os dados informados. Cancelar MUST fechar sem criar nada. A
conta nasce aberta.

#### Scenario: Conta criada com dados válidos
- **WHEN** o usuário percorre o assistente com tipo Corrente, nome "Conta principal", moeda real e saldo inicial 100,00 e conclui
- **THEN** a conta aparece no gerenciador com saldo R$ 100,00 e o sistema confirma a criação

#### Scenario: Conta sem instituição
- **WHEN** o usuário escolhe "não é mantida em instituição", tipo Dinheiro, nome "Carteira", moeda real e saldo 50,00 e conclui
- **THEN** a conta "Carteira" é criada com saldo R$ 50,00 e aparece como não mantida em instituição

#### Scenario: Conta com instituição nova
- **WHEN** o usuário marca que a conta é mantida em instituição, digita "Banco A", que ainda não existe, e conclui o assistente
- **THEN** a instituição "Banco A" é criada e associada à conta

#### Scenario: Instituição em branco
- **WHEN** o usuário marca que a conta é mantida em instituição, deixa o nome vazio e tenta avançar
- **THEN** o assistente permanece no passo 1 e pede o nome da instituição ou a opção sem instituição

#### Scenario: Nome sugerido
- **WHEN** o usuário informa a instituição "Banco A" e o tipo Corrente e chega ao passo do nome sem ter digitado nome
- **THEN** o assistente sugere o nome "Banco A Conta corrente", que o usuário pode alterar

#### Scenario: Nome vazio impede avançar
- **WHEN** o usuário apaga o nome da conta no passo 3 e tenta avançar
- **THEN** o assistente permanece no passo 3 e indica que o nome é obrigatório

#### Scenario: Voltar preserva os dados
- **WHEN** o usuário chega ao passo 4, volta ao passo 2 e avança de novo
- **THEN** a instituição, o tipo e o nome informados antes continuam preenchidos

#### Scenario: Recusa ao concluir
- **WHEN** o usuário conclui o assistente com o nome de uma conta que já existe
- **THEN** o assistente continua aberto com a mensagem de nome repetido e nenhuma conta é criada

#### Scenario: Cancelar o assistente
- **WHEN** o usuário cancela o assistente em qualquer passo
- **THEN** o pop-up fecha e nenhuma conta, instituição ou grupo é criado

### Requirement: Nome da conta válido e único
O nome da conta MUST ter de 1 a 100 caracteres sem contar espaços nas pontas e MUST ser único entre
todas as contas, abertas ou encerradas, sem diferenciar maiúsculas de minúsculas.

#### Scenario: Nome vazio
- **WHEN** o usuário tenta salvar uma conta com nome vazio ou só com espaços
- **THEN** o sistema recusa e indica que o nome é obrigatório

#### Scenario: Nome repetido
- **WHEN** o usuário tenta salvar uma conta com o nome "conta principal" e já existe "Conta principal"
- **THEN** o sistema recusa e indica que já existe conta com esse nome

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

### Requirement: Vínculo entre conta de investimento e conta caixa
Uma conta de Investimento SHALL poder ter uma conta caixa vinculada, ou nenhuma. A conta caixa MUST
ser do tipo Corrente, Poupança ou Dinheiro, estar aberta e ter a mesma moeda do investimento. Uma
mesma conta caixa MAY servir a mais de um investimento. Na criação do investimento, o usuário SHALL
poder criar a conta caixa junto (tipo Dinheiro, mesma moeda e instituição, nome sugerido
"<nome do investimento> (Caixa)" e saldo inicial próprio) ou escolher uma conta existente compatível.

#### Scenario: Investimento criado com caixa nova
- **WHEN** o usuário cria o investimento "Corretora" em USD escolhendo criar a conta caixa com saldo 50,00
- **THEN** são criadas a conta de Investimento "Corretora" e a conta Dinheiro "Corretora (Caixa)" em USD com saldo US$ 50,00, vinculadas

#### Scenario: Caixa de outra moeda
- **WHEN** o usuário tenta vincular a um investimento em USD uma conta caixa em BRL
- **THEN** o sistema recusa o vínculo

#### Scenario: Caixa de tipo incompatível
- **WHEN** o usuário tenta vincular como caixa uma conta do tipo Cartão de crédito
- **THEN** o sistema recusa o vínculo

#### Scenario: Lista de caixas oferece só compatíveis
- **WHEN** o usuário escolhe usar uma conta caixa existente
- **THEN** a lista mostra apenas contas abertas Corrente, Poupança ou Dinheiro na moeda do investimento

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

### Requirement: Conta favorita
O usuário SHALL poder marcar e desmarcar uma conta como favorita, e o gerenciador MUST indicar
visualmente as favoritas.

#### Scenario: Marcar favorita
- **WHEN** o usuário marca uma conta como favorita
- **THEN** o cartão da conta passa a exibir o indicador de favorita

### Requirement: Encerrar e reabrir conta
O sistema SHALL permitir encerrar uma conta aberta e reabrir uma encerrada. Conta encerrada MUST
sair do gerenciador e do total, a menos que o usuário peça para mostrar as encerradas, e MUST NOT
ser oferecida como conta caixa. Uma conta caixa vinculada a investimento aberto MUST NOT ser
encerrada.

#### Scenario: Encerrar conta
- **WHEN** o usuário encerra uma conta
- **THEN** ela some do gerenciador e o total da sua moeda deixa de incluí-la

#### Scenario: Mostrar encerradas
- **WHEN** o usuário ativa a opção de mostrar contas encerradas
- **THEN** as encerradas aparecem identificadas como encerradas e podem ser reabertas

#### Scenario: Encerrar caixa em uso
- **WHEN** o usuário tenta encerrar uma conta caixa vinculada a um investimento aberto
- **THEN** o sistema recusa e indica o investimento vinculado

### Requirement: Conta não é excluída
O sistema MUST NOT oferecer exclusão de conta, nem definitiva nem lógica. A única forma de tirar uma
conta de uso SHALL ser encerrá-la, o que preserva a conta e o seu histórico.

#### Scenario: Ações disponíveis para uma conta
- **WHEN** o usuário abre o menu de ações de uma conta aberta
- **THEN** o menu oferece editar, favoritar e encerrar, e não oferece excluir

#### Scenario: Conta caixa vinculada
- **WHEN** o usuário quer tirar de uso uma conta caixa vinculada a um investimento aberto
- **THEN** o sistema exige encerrar antes o investimento, e a conta caixa continua existindo

### Requirement: Gerenciador de contas
O sistema SHALL exibir as contas abertas com ícone do tipo, nome e saldo, em ícones grandes (padrão)
ou em lista com a instituição, à escolha do usuário, agrupadas por tipo (padrão), por instituição
ou por grupo. No rodapé MUST aparecer um total por moeda, somando o saldo exibido das contas abertas
daquela moeda. O usuário
SHALL poder filtrar a lista digitando parte do nome da conta ou da instituição. Sem nenhuma
conta, MUST aparecer a mensagem "Nenhuma conta cadastrada" com a ação de criar conta.

#### Scenario: Total por moeda
- **WHEN** existem contas abertas em BRL com saldos 100,00 e −30,00 e uma em USD com saldo 10,00
- **THEN** o rodapé exibe R$ 70,00 e US$ 10,00 como totais separados

#### Scenario: Agrupar por instituição
- **WHEN** o usuário escolhe agrupar por instituição
- **THEN** as contas ficam agrupadas pelo nome da instituição, com as contas sem instituição num grupo próprio

#### Scenario: Busca
- **WHEN** o usuário digita "azul" na busca
- **THEN** a lista mostra só as contas com "azul" no nome ou na instituição, sem diferenciar maiúsculas

#### Scenario: Estado vazio
- **WHEN** não existe nenhuma conta
- **THEN** o gerenciador mostra "Nenhuma conta cadastrada" e o botão "Criar conta"

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

### Requirement: Ações pelo teclado e menu de contexto
Criar conta SHALL estar disponível pelo atalho Ctrl+N no gerenciador. Editar, favoritar e
encerrar/reabrir MUST estar disponíveis no menu de contexto da linha da conta, no botão
"Mais ações" da linha e pelo teclado. No assistente de nova conta, Enter MUST avançar (ou concluir
no último passo) e Esc MUST cancelar.

#### Scenario: Atalho de nova conta
- **WHEN** o usuário pressiona Ctrl+N no gerenciador
- **THEN** o assistente de nova conta é aberto no passo da instituição

#### Scenario: Avançar com Enter
- **WHEN** o usuário pressiona Enter no passo do nome com um nome preenchido
- **THEN** o assistente vai para o passo de saldo e moeda
