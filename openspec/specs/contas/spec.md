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
- **WHEN** o usuário abre a lista de tipos no formulário de conta
- **THEN** cada um dos sete tipos aparece com sua descrição

### Requirement: Criar conta
O sistema SHALL criar uma conta a partir de tipo, nome, moeda e saldo inicial obrigatórios, e de
instituição financeira, grupo e favorita opcionais. A conta nasce aberta.

#### Scenario: Conta criada com dados válidos
- **WHEN** o usuário informa tipo Corrente, nome "Conta principal", moeda real e saldo inicial 100,00 e confirma
- **THEN** a conta aparece no gerenciador com saldo R$ 100,00 e o sistema confirma a criação

#### Scenario: Conta sem instituição
- **WHEN** o usuário cria uma conta sem informar instituição
- **THEN** a conta é criada e aparece como não mantida em instituição

#### Scenario: Nome sugerido
- **WHEN** o usuário escolhe instituição e tipo sem ter digitado nome
- **THEN** o formulário sugere o nome "<instituição> <tipo>", que o usuário pode alterar

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
Saldos MUST ser exibidos na moeda da conta, com o símbolo dela e o formato numérico pt-BR.

#### Scenario: Conta em dólar
- **WHEN** o usuário cria uma conta com moeda USD e saldo inicial 1234,5
- **THEN** o gerenciador exibe o saldo como US$ 1.234,50

#### Scenario: Casas decimais da moeda
- **WHEN** o usuário informa um saldo inicial com mais casas decimais do que a moeda da conta admite
- **THEN** o sistema recusa o valor

### Requirement: Saldo inicial e sinal por tipo
O saldo inicial SHALL ser o saldo exibido da conta enquanto não houver lançamentos. Para Cartão de
crédito e Passivo, o usuário MUST informar o saldo devedor como valor positivo, e o sistema MUST
exibi-lo e somá-lo ao total como negativo. Para Ativo, o saldo inicial MUST NOT ser negativo. Conta
de Investimento MUST NOT ter saldo inicial próprio: seu saldo exibido é zero até existirem posições.

#### Scenario: Cartão com dívida
- **WHEN** o usuário cria um Cartão de crédito com saldo devedor 500,00
- **THEN** o gerenciador exibe o saldo como negativo, −R$ 500,00, e o total em reais diminui 500,00

#### Scenario: Ativo negativo
- **WHEN** o usuário tenta criar um Ativo com saldo inicial negativo
- **THEN** o sistema recusa o valor

#### Scenario: Investimento sem saldo inicial
- **WHEN** o usuário escolhe o tipo Investimento no formulário
- **THEN** o campo de saldo inicial não é oferecido e a conta é criada com saldo zero

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
como caixa.

#### Scenario: Renomear conta
- **WHEN** o usuário altera o nome de uma conta para um nome válido e único
- **THEN** o gerenciador passa a exibir o novo nome

#### Scenario: Moeda de conta vinculada
- **WHEN** o usuário tenta alterar a moeda de uma conta caixa vinculada a um investimento
- **THEN** o sistema recusa e indica o vínculo que impede a alteração

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
Abrir uma conta (duplo clique ou Enter no gerenciador, ou clique numa favorita do Início) SHALL
levar à página da conta, com os detalhes dela (nome, tipo, instituição, moeda, saldo, grupo, conta
caixa do investimento, favorita e situação), os comandos de editar, favoritar e encerrar ou reabrir,
e o caminho de volta ao gerenciador. As visões Movimentação e Histórico MUST aparecer desativadas
até existirem lançamentos.

#### Scenario: Abrir a conta
- **WHEN** o usuário dá duplo clique numa conta do gerenciador
- **THEN** a página da conta mostra os detalhes e o saldo dela

#### Scenario: Voltar ao gerenciador
- **WHEN** o usuário escolhe "Gerenciador de contas" na página da conta
- **THEN** o gerenciador de contas é exibido

### Requirement: Ações pelo teclado e menu de contexto
Criar conta SHALL estar disponível pelo atalho Ctrl+N no gerenciador. Editar, favoritar e
encerrar/reabrir MUST estar disponíveis no menu de contexto da linha da conta, no botão
"Mais ações" da linha e pelo teclado.

#### Scenario: Atalho de nova conta
- **WHEN** o usuário pressiona Ctrl+N no gerenciador
- **THEN** o formulário de nova conta é aberto
