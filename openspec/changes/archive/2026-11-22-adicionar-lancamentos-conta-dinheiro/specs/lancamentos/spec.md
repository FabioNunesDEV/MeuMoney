## Purpose

Registra o dinheiro que entra e sai das contas, como gastos, recebimentos e transferências, na
Movimentação da conta, e faz o saldo das contas refletir esses lançamentos.

## ADDED Requirements

### Requirement: Movimentação da conta Dinheiro
A página da conta Dinheiro SHALL ter a visão Movimentação, com estas partes:
- **Grade:** os lançamentos da conta em ordem de data e, na mesma data, de registro, com as colunas
  Data (dd/MM/yyyy), Favorecido, Categoria (com a subcategoria, quando houver), C (situação), Gastar,
  Receber e Saldo (o saldo da conta depois daquela linha).
- **Rodapé:** o saldo final da conta.
- **Formulário:** fica abaixo da grade, com as abas Gasto, Recebimento e Transferência.

A aba Gasto MUST ser a escolhida ao entrar na Movimentação. Valores MUST aparecer na moeda da conta.
Gastar e Receber MUST aparecer sempre positivos e na cor normal do texto: a coluna diz o sentido.
A coluna Saldo e o saldo final seguem o formato contábil de saldo (requisito "Moeda da conta" de
`contas`): positivo em azul, negativo em vermelho entre parênteses.

#### Scenario: Entrar na Movimentação
- **WHEN** o usuário abre uma conta Dinheiro com saldo inicial 100,00 e um gasto de 30,00
- **THEN** a grade mostra o gasto com 30,00 em Gastar e saldo 70,00 em azul, o rodapé mostra saldo final R$ 70,00 em azul e negrito e a aba Gasto está escolhida

#### Scenario: Conta sem lançamentos
- **WHEN** o usuário abre uma conta Dinheiro sem lançamentos
- **THEN** a grade aparece vazia com a orientação de usar Novo para registrar o primeiro lançamento, e o rodapé mostra o saldo inicial

### Requirement: Comandos do formulário de lançamento
O formulário SHALL ter os comandos Novo, Editar, Inserir e Cancelar:
- **Novo:** mostra os campos vazios da aba escolhida, com a data de hoje. Desativa Novo e ativa
  Inserir e Cancelar.
- **Escolher uma linha da grade:** leva o formulário à aba do lançamento e mostra os dados dele só
  para leitura, com Editar ativo.
- **Editar:** libera os campos do lançamento escolhido para alteração. Desativa Editar e ativa
  Inserir e Cancelar.
- **Inserir:** valida os campos e grava o lançamento novo ou a alteração. Gravado, oculta os campos e
  atualiza a grade e o saldo. Com os campos de um lançamento novo todos vazios, Inserir MUST NOT
  gravar nada e MUST voltar ao estado inicial, com Novo ativo. Com campo inválido, a mensagem MUST
  aparecer no formulário, que continua aberto com o que foi digitado.
- **Cancelar:** abandona a inclusão ou a alteração sem mudar nada.

Durante uma inclusão ou alteração, trocar de aba MUST valer só para um lançamento novo; na alteração,
o tipo do lançamento MUST NOT mudar.

#### Scenario: Novo e Inserir
- **WHEN** o usuário clica em Novo na aba Gasto, preenche favorecido "Padaria", categoria "Alimentação", montante 12,50 e clica em Inserir
- **THEN** o gasto aparece na grade, o saldo diminui 12,50, os campos são ocultados e Novo volta a ficar ativo

#### Scenario: Inserir com campos vazios
- **WHEN** o usuário clica em Novo e em seguida em Inserir sem preencher nada
- **THEN** nada é gravado e o formulário volta ao estado inicial com Novo ativo

#### Scenario: Editar um lançamento
- **WHEN** o usuário escolhe um recebimento na grade, clica em Editar, muda o montante e clica em Inserir
- **THEN** o formulário vai para a aba Recebimento, a alteração é gravada e o saldo é recalculado

#### Scenario: Cancelar a alteração
- **WHEN** o usuário clica em Editar, muda o memo e clica em Cancelar
- **THEN** o lançamento continua como estava

### Requirement: Gasto e recebimento
Um gasto SHALL diminuir o saldo da conta e um recebimento SHALL aumentá-lo, no valor do montante.
Os campos são:

| Campo | Obrigatório |
|---|---|
| Pagar a (favorecido) | sim |
| Categoria | sim |
| Subcategoria | não |
| Memo | não |
| Data | sim |
| Montante | sim |

#### Scenario: Recebimento
- **WHEN** o usuário insere um recebimento de 200,00 do favorecido "Salário", categoria "Renda"
- **THEN** o lançamento aparece com 200,00 em Receber e o saldo aumenta 200,00

#### Scenario: Categoria ausente
- **WHEN** o usuário tenta inserir um gasto sem categoria
- **THEN** o sistema recusa com a mensagem de que a categoria é obrigatória

### Requirement: Validação do lançamento
O sistema MUST recusar um lançamento nestes casos:
- montante zero, negativo ou com mais casas decimais do que a moeda da conta admite;
- data inválida;
- favorecido vazio;
- categoria vazia no gasto ou no recebimento;
- conta encerrada ou que não seja do tipo Dinheiro.

A recusa MUST vir com a mensagem do motivo. Conta encerrada MUST NOT receber lançamento novo, nem ter
lançamento alterado, movido, marcado ou excluído.

#### Scenario: Montante negativo
- **WHEN** o usuário tenta inserir um gasto com montante −10,00
- **THEN** o sistema recusa com a mensagem de que o montante deve ser maior que zero

#### Scenario: Conta encerrada
- **WHEN** o usuário abre a Movimentação de uma conta Dinheiro encerrada
- **THEN** os lançamentos aparecem, mas Novo, Editar e as ações do menu de contexto ficam desativados

### Requirement: Favorecido, categoria e subcategoria pelo nome
Favorecido, categoria e subcategoria SHALL ser escolhidos de uma lista dos já cadastrados ou criados
ao digitar um nome novo. Os nomes seguem as regras de nome do sistema: são obrigatórios, sem espaços
nas pontas, com no máximo 100 caracteres, e a comparação não diferencia maiúsculas de minúsculas.
- Nome existente MUST ser reaproveitado, nunca duplicado.
- A subcategoria pertence à sua categoria: a lista de subcategorias MUST mostrar só as da categoria
  informada, e o mesmo nome MAY existir em categorias diferentes.
- Ao escolher um favorecido que já tem lançamento, o formulário SHALL trazer a categoria, a
  subcategoria e o montante do lançamento mais recente dele, que o usuário pode trocar.

#### Scenario: Favorecido novo
- **WHEN** o usuário digita "Feira do bairro" em "Pagar a", que ainda não existe, e insere o gasto
- **THEN** o favorecido é criado e passa a aparecer na lista dos próximos lançamentos

#### Scenario: Sugestão pelo último uso
- **WHEN** o último gasto do favorecido "Posto" foi na categoria "Carro", subcategoria "Combustível", de 150,00, e o usuário escolhe "Posto" num lançamento novo
- **THEN** categoria, subcategoria e montante são preenchidos com "Carro", "Combustível" e 150,00

#### Scenario: Subcategoria de outra categoria
- **WHEN** existem as subcategorias "Combustível" em "Carro" e "Gás" em "Casa", e o usuário escolhe a categoria "Casa"
- **THEN** a lista de subcategorias oferece "Gás" e não oferece "Combustível"

### Requirement: Transferência entre contas Dinheiro
A aba Transferência SHALL registrar a saída de uma conta (De) e a entrada em outra (Para), com
"Pagar a", memo, data e montante:
- De MUST vir preenchida com a conta da página e "Pagar a" com "Transferência"; o usuário pode
  trocar os dois.
- As duas contas MUST ser diferentes, abertas, do tipo Dinheiro e da mesma moeda; caso contrário, o
  sistema MUST recusar com a mensagem do motivo.
- A transferência SHALL gerar um lançamento em cada conta: na conta De, em Gastar; na conta Para, em
  Receber. A grade MUST indicar a outra conta.
- Alterar data, montante, favorecido ou memo de um lado MUST alterar o outro; excluir ou anular um
  lado MUST excluir ou anular os dois.

#### Scenario: Transferir entre carteiras
- **WHEN** existem as contas Dinheiro "Carteira" com saldo 100,00 e "Cofre" com saldo 0,00, e o usuário insere na "Carteira" uma transferência de 40,00 para "Cofre"
- **THEN** "Carteira" passa a ter saldo 60,00, "Cofre" passa a ter saldo 40,00 e cada uma mostra o lançamento indicando a outra conta

#### Scenario: Moedas diferentes
- **WHEN** o usuário tenta transferir de uma conta Dinheiro em BRL para uma conta Dinheiro em USD
- **THEN** o sistema recusa com a mensagem de que a transferência exige contas da mesma moeda

#### Scenario: Excluir transferência
- **WHEN** o usuário exclui, na "Cofre", o lançamento da transferência vinda da "Carteira"
- **THEN** o lançamento some das duas contas e os dois saldos voltam ao que eram

### Requirement: Confirmação de saldo negativo
Ao inserir um lançamento novo ou uma alteração, se o saldo final de uma conta afetada for ficar
negativo, o sistema SHALL pedir confirmação num pop-up antes de gravar. O pop-up mostra a conta e o
saldo resultante, com Cancelar e Confirmar. Cancelar MUST NOT gravar nada e MUST manter o formulário
como estava. Confirmar MUST gravar normalmente. Se o saldo já era negativo e continua negativo, a
confirmação também MUST ser pedida, desde que o lançamento diminua o saldo.

#### Scenario: Gasto maior que o saldo
- **WHEN** a "Carteira" tem saldo 20,00 e o usuário insere um gasto de 50,00
- **THEN** aparece o pop-up informando que o saldo da "Carteira" ficará em (R$ 30,00), com Cancelar e Confirmar

#### Scenario: Cancelar a confirmação
- **WHEN** o pop-up de saldo negativo aparece e o usuário clica em Cancelar
- **THEN** nada é gravado e o formulário continua aberto com os dados digitados

#### Scenario: Confirmar
- **WHEN** o pop-up de saldo negativo aparece e o usuário clica em Confirmar
- **THEN** o gasto é gravado e a conta passa a mostrar o saldo (R$ 30,00) em vermelho e negrito

#### Scenario: Recebimento não pede confirmação
- **WHEN** a conta tem saldo −30,00 e o usuário insere um recebimento de 10,00
- **THEN** o recebimento é gravado sem pop-up

### Requirement: Situação do lançamento
Cada lançamento SHALL ter uma situação: Não reconciliado (padrão), Compensado, Reconciliado ou Nulo.
O usuário SHALL marcá-la pelo menu de contexto em "Marcar como". A coluna C MUST mostrar "C" para
compensado, "R" para reconciliado e nada para não reconciliado. Um lançamento nulo MUST continuar na
grade, identificado como anulado e com o valor riscado, e MUST NOT contar no saldo. Voltar um nulo
para outra situação MUST fazê-lo contar de novo.

Na transferência, Compensado e Reconciliado valem só para o lado marcado; Nulo vale para os dois.

#### Scenario: Marcar como compensado
- **WHEN** o usuário marca um gasto como Compensado
- **THEN** a coluna C mostra "C" e o saldo não muda

#### Scenario: Anular um gasto
- **WHEN** o usuário marca como Nulo um gasto de 30,00
- **THEN** o gasto continua na grade identificado como anulado e o saldo aumenta 30,00

### Requirement: Mover e excluir lançamento
- **Mover para conta:** o menu de contexto SHALL oferecer essa ação, que leva o lançamento para
  outra conta Dinheiro aberta da mesma moeda, escolhida numa lista. Uma transferência MUST NOT ser
  movida para a conta do outro lado.
- **Excluir:** MUST pedir confirmação no pop-up de exclusão. Confirmada, o lançamento sai da conta e
  o saldo é recalculado.

Mover e excluir MUST ser avisados na notificação passageira.

#### Scenario: Mover um gasto
- **WHEN** o usuário move um gasto de 30,00 da "Carteira" para o "Cofre"
- **THEN** o gasto sai da "Carteira" e aparece no "Cofre", e os dois saldos são recalculados

#### Scenario: Excluir com confirmação
- **WHEN** o usuário escolhe Excluir num lançamento e confirma
- **THEN** o lançamento sai da grade, o saldo é recalculado e a notificação confirma a exclusão

### Requirement: Teclado e menu de contexto da grade
As linhas da grade MUST ser focáveis e navegáveis pelas setas:
- Enter na linha MUST levar o lançamento ao formulário para leitura;
- a tecla de menu ou o botão "Mais ações" MUST abrir o menu de contexto, com Editar, Ir para a
  outra conta (só na transferência), Marcar como, Mover para conta e Excluir;
- Ctrl+N MUST equivaler a Novo;
- no formulário, Enter MUST equivaler a Inserir e Esc a Cancelar.

#### Scenario: Novo pelo teclado
- **WHEN** o usuário pressiona Ctrl+N na Movimentação
- **THEN** o formulário abre um lançamento novo na aba escolhida, com o foco em "Pagar a"

#### Scenario: Ir para a outra conta
- **WHEN** o usuário escolhe "Ir para conta: Cofre" no menu de uma transferência da "Carteira"
- **THEN** a página da conta "Cofre" abre na Movimentação
