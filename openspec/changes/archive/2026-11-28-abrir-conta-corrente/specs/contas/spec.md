## MODIFIED Requirements

### Requirement: Criar conta
O sistema SHALL criar uma conta por um assistente em passos curtos, nesta ordem:
1. **Instituição**: a conta é mantida numa instituição, escolhida da lista ou digitada como nome
   novo, ou não é mantida em instituição;
2. **Tipo**: um dos tipos de conta, com a descrição do escolhido;
3. **Nome**: nome da conta, com grupo e favorita opcionais;
4. **Número da conta**: opcional, só nos tipos que pedem o número na criação (neste momento, só a
   Corrente); nos demais tipos o passo não existe;
5. **Saldo e moeda**: saldo inicial e moeda e, nos tipos que têm saldo mínimo, o saldo mínimo
   opcional; no Investimento, a conta caixa no lugar do saldo.

O assistente MUST mostrar em que passo está e quantos passos há para o tipo escolhido; a contagem
muda quando o tipo muda. O assistente MUST oferecer Voltar (exceto no primeiro passo), Avançar
(Concluir no último) e Cancelar. Voltar MUST preservar o que já foi informado. Avançar MUST NOT sair
do passo 1 com a opção de instituição marcada e o nome da instituição vazio, nem do passo 3 com o
nome da conta vazio. No passo 3, Avançar MUST conferir no sistema se o nome pode ser usado; se for
recusado (por exemplo, por já existir conta com esse nome), o assistente MUST permanecer no passo 3
com a mensagem do sistema. Número e saldo mínimo informados num tipo que não os tem MUST NOT ser
gravados, mesmo que tenham sido digitados antes de o usuário voltar e trocar o tipo.

A conta MUST ser gravada só ao concluir; se o sistema recusar os dados, a mensagem MUST aparecer no
assistente, que permanece aberto com os dados informados. Cancelar MUST fechar sem criar nada. A
conta nasce aberta.

#### Scenario: Conta criada com dados válidos
- **WHEN** o usuário percorre o assistente com tipo Corrente, nome "Conta principal", número "1234-5", moeda real, saldo inicial 100,00 e saldo mínimo −500,00 e conclui
- **THEN** a conta aparece no gerenciador com saldo R$ 100,00, os Detalhes mostram o número "1234-5" e o saldo mínimo (R$ 500,00), e o sistema confirma a criação

#### Scenario: Corrente sem número nem saldo mínimo
- **WHEN** o usuário cria uma conta Corrente deixando o número e o saldo mínimo em branco
- **THEN** a conta é criada sem número e sem saldo mínimo

#### Scenario: Tipo sem passo de número
- **WHEN** o usuário escolhe o tipo Dinheiro no passo 2
- **THEN** o assistente indica quatro passos, vai do nome direto para o saldo e não oferece saldo mínimo

#### Scenario: Corrente com passo de número
- **WHEN** o usuário escolhe o tipo Corrente no passo 2
- **THEN** o assistente indica cinco passos, com o passo do número entre o nome e o saldo, e o passo do saldo oferece o saldo mínimo

#### Scenario: Troca de tipo descarta o número
- **WHEN** o usuário escolhe Corrente, informa o número "1234-5", volta ao passo 2, escolhe Dinheiro e conclui
- **THEN** a conta Dinheiro é criada sem número

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

#### Scenario: Nome repetido impede avançar
- **WHEN** já existe a conta "Conta principal" e o usuário digita "conta principal" no passo 3 e clica Avançar
- **THEN** o assistente permanece no passo 3 com a mensagem de que já existe conta com esse nome e nada é criado

#### Scenario: Voltar preserva os dados
- **WHEN** o usuário chega ao último passo, volta ao passo 2 e avança de novo
- **THEN** a instituição, o tipo e o nome informados antes continuam preenchidos

#### Scenario: Recusa ao concluir
- **WHEN** o usuário conclui o assistente com dados que o sistema recusa, como um saldo com mais casas decimais do que a moeda admite
- **THEN** o assistente continua aberto com a mensagem do sistema e nenhuma conta é criada

#### Scenario: Cancelar o assistente
- **WHEN** o usuário cancela o assistente em qualquer passo
- **THEN** o pop-up fecha e nenhuma conta, instituição ou grupo é criado

### Requirement: Editar conta
O sistema SHALL permitir alterar nome, instituição, grupo, saldo inicial, favorita e, no
investimento, a conta caixa, com as mesmas validações da criação. Os campos complementares também
SHALL poder ser alterados, todos opcionais:
- **Número da conta:** texto livre de até 50 caracteres;
- **Abreviação:** texto livre de até 20 caracteres;
- **Comentário:** texto livre de até 1.000 caracteres;
- **Saldo mínimo:** valor na moeda da conta, positivo, zero ou negativo, com no máximo as casas
  decimais da moeda. MUST existir só nas contas Corrente e Poupança; a conta Dinheiro MUST NOT ter
  saldo mínimo. Neste momento ele só é guardado e exibido, sem aviso nem bloqueio.

Os textos MUST ser gravados sem os espaços das pontas; texto só com espaços é gravado como vazio.
Texto acima do limite MUST ser recusado com a mensagem que informa o limite. Dos campos
complementares, o assistente de nova conta MUST pedir só o número da conta e o saldo mínimo, nos
tipos que os têm; abreviação e comentário ficam só na edição.

O saldo mínimo que já estiver gravado numa conta Dinheiro MUST ser apagado ao atualizar o aplicativo.

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
- **WHEN** o usuário informa número "1234-5", abreviação "Princ", comentário "Uso diário" e saldo mínimo 50,00 numa conta Corrente e salva
- **THEN** ao abrir a conta de novo, os quatro valores aparecem como foram informados

#### Scenario: Saldo mínimo negativo
- **WHEN** o usuário informa saldo mínimo (R$ 500,00) numa conta Corrente
- **THEN** o sistema aceita e grava o valor negativo

#### Scenario: Saldo mínimo em tipo sem saldo mínimo
- **WHEN** um saldo mínimo é enviado para uma conta Cartão de crédito ou Dinheiro
- **THEN** o sistema recusa com a mensagem de que esse tipo de conta não tem saldo mínimo

#### Scenario: Dinheiro sem campo de saldo mínimo
- **WHEN** o usuário abre os Detalhes ou o pop-up de edição de uma conta Dinheiro
- **THEN** o campo de saldo mínimo não aparece, e salvar a conta funciona normalmente

#### Scenario: Saldo mínimo antigo da Dinheiro apagado
- **WHEN** o aplicativo atualizado abre um banco em que uma conta Dinheiro tem saldo mínimo 50,00 e uma Corrente tem saldo mínimo −500,00
- **THEN** a conta Dinheiro fica sem saldo mínimo e a Corrente mantém (R$ 500,00)

#### Scenario: Saldo mínimo com casas demais
- **WHEN** o usuário informa saldo mínimo 10,005 numa conta em real
- **THEN** o sistema recusa com a mensagem do número máximo de casas decimais da moeda

#### Scenario: Abreviação longa
- **WHEN** o usuário informa uma abreviação com 21 caracteres
- **THEN** o sistema recusa com a mensagem de que a abreviação tem no máximo 20 caracteres

#### Scenario: Pop-up do gerenciador com os campos complementares
- **WHEN** o usuário escolhe Editar numa conta no gerenciador
- **THEN** o pop-up de edição mostra também número, abreviação, comentário e, nos tipos que o têm, o saldo mínimo
