## Why

Hoje a conta só guarda o saldo inicial: não há como registrar um gasto, um recebimento ou uma
transferência, que é o uso básico do Money 99. Este é o segundo dos quatro changes do roteiro da
conta Dinheiro, depois do assistente de criação (A). Os próximos são o histórico do saldo (C) e os
detalhes editáveis (D).

## What Changes

- **Movimentação da conta Dinheiro.** A página da conta Dinheiro passa a abrir na visão
  Movimentação, como no Money:
  - grade com Data, Favorecido, Categoria, C (situação), Gastar, Receber e o saldo linha a linha;
  - saldo final no rodapé;
  - formulário embaixo, com as abas Gasto (padrão), Recebimento e Transferência e os comandos
    Novo, Editar, Inserir e Cancelar, com o comportamento do roteiro.
- **Gasto e Recebimento.** Campos:
  - "Pagar a" (favorecido) e categoria, obrigatórios;
  - subcategoria, opcional;
  - memo, data e montante.

  Favorecido, categoria e subcategoria se escolhem da lista dos já usados ou se criam digitando um
  nome novo. Ao escolher um favorecido já usado, o formulário traz a categoria, a subcategoria e o
  montante do último lançamento dele.
- **Transferência entre contas Dinheiro da mesma moeda.**
  - Campos: De, Para, "Pagar a" (já preenchido com "Transferência"), memo, data e montante.
  - Gera um lançamento em cada conta, ligados entre si. Editar, anular ou excluir um deles vale
    para os dois.
- **Regras do lançamento:** montante maior que zero, nas casas decimais da moeda; data válida;
  conta encerrada não recebe nem altera lançamento.
- **Saldo negativo pede confirmação.** No Inserir, se o saldo final de uma conta afetada for ficar
  negativo, aparece um pop-up com o saldo resultante, Cancelar e Confirmar. Cancelar não registra
  nada; Confirmar registra normalmente.
- **Menu de contexto do lançamento:**
  - Editar;
  - Ir para a outra conta (na transferência);
  - Marcar como (Não reconciliado, Compensado, Reconciliado, Nulo);
  - Mover para conta;
  - Excluir, com confirmação; na transferência, exclui as duas pontas.
- **Nulo:** o lançamento continua na grade, marcado como anulado, e deixa de contar no saldo.
- **Saldo das contas:** saldo inicial mais os lançamentos não nulos. O saldo vale em toda parte:
  gerenciador, totais, Início e página da conta.
- **Menu "Contas" no painel da página da conta:** abre a lista de contas para ir direto a outra
  conta, com o gerenciador no topo e as encerradas num submenu.
- **Moeda travada:** conta com lançamentos não troca de moeda.
- **Saldo no formato contábil, em todas as telas:**
  - positivo em azul e negrito;
  - negativo em vermelho e negrito, entre parênteses, sem sinal de menos (`(R$ 500,00)`);
  - zero na cor normal.

  Vale para o saldo das contas, os totais, as dívidas do Início, o saldo final e a coluna Saldo da
  grade (nesta, sem negrito). As colunas Gastar e Receber ficam sempre positivas e na cor normal.
  Um componente único passa a mostrar todo saldo, e o CLAUDE.md troca a regra de cores.
- **Teste de tela** com o roteiro de aceite: duas contas Dinheiro, lançamentos nas duas e
  transferência entre elas, conferindo os saldos.

Fica de fora deste change:
- Movimentação dos outros tipos de conta: nas contas que não são Dinheiro, a visão aparece
  desativada, com "Em breve".
- Câmbio entre moedas.
- "Dividir", "Calc. saldo da conta", número do cheque.
- "Adicionar a Contas a pagar" e "Ir para favorecido", que dependem de módulos ainda inexistentes.
- Cadastro próprio de favorecidos e categorias (renomear, excluir, classificar): fica para o
  módulo Categorias.
- Histórico (change C) e Detalhes editável (change D).

## Capabilities

### New Capabilities

- `lancamentos`:
  - a Movimentação da conta Dinheiro: grade, formulário em abas e comandos;
  - gasto, recebimento e transferência;
  - favorecido, categoria e subcategoria criados pelo nome, com sugestão pelo último uso;
  - validação, confirmação de saldo negativo, situação (incluindo Nulo), mover e excluir;
  - o efeito dos lançamentos no saldo.

### Modified Capabilities

- `contas`:
  - "Saldo inicial e sinal por tipo": o saldo exibido passa a ser o saldo inicial mais os
    lançamentos;
  - "Editar conta": a moeda fica travada quando há lançamentos;
  - "Página da conta": a conta Dinheiro abre na Movimentação, Detalhes vira a segunda visão, o
    Histórico continua desativado e o painel ganha o menu "Contas";
  - "Moeda da conta": saldo exibido no formato contábil.
- `painel-inicial`: em "Dívidas e distribuição por tipo", a dívida passa a aparecer entre
  parênteses, em vermelho.

## Impact

- `Money.Domain`: pasta nova `Lancamentos/`, com:
  - `Lancamento` (tipo, data, valor, situação, memo, ligação da transferência);
  - `Favorecido`, `Categoria` e `Subcategoria`;
  - as regras do lançamento.
- `Money.Services`:
  - `LancamentoServico`, com os DTOs e o repositório de lançamentos (listar por conta e somar por
    conta);
  - `ContaServico` passa a calcular o saldo com os lançamentos e trava a moeda.
- `Money.Data`: mapeamentos e repositório. `Money.Data.Sqlite`: migration nova
  `AdicionarLancamentos`, só com tabelas novas e sem tocar nos dados existentes.
- `Money.Api`: rotas `/api/contas/{id}/lancamentos` e `/api/lancamentos` (opções do formulário,
  criar, alterar, situação, mover, excluir).
- `Money.Web`:
  - `lancamentos/`: Movimentação, formulário e menu;
  - `ContaPagina.tsx`: visões e menu "Contas";
  - `comum/`: componente `ValorSaldo`, no lugar dos 8 pontos que hoje repetem a regra de cor no
    gerenciador, no Início e na página da conta;
  - `estilo.css`: variável nova `--saldo-positivo`;
  - `api.ts`.
- `CLAUDE.md`: a regra "Verde e vermelho só para valor" passa a descrever o formato contábil.
- Testes: Domain, Services, Data, Api e um teste de tela novo.
