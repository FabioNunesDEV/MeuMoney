## MODIFIED Requirements

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
- **WHEN** o usuário escolhe o tipo Investimento e chega ao último passo do assistente
- **THEN** o campo de saldo inicial não é oferecido, a escolha da conta caixa aparece no lugar e a conta é criada com saldo zero

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
