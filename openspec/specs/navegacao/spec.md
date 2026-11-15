# navegacao Specification

## Purpose
Define a casca do aplicativo: a navegação lateral entre os módulos e as formas padronizadas de
confirmar ações, pedir confirmação e avisar o usuário.

## Requirements

### Requirement: Barra de módulos e arranjo das páginas
A janela SHALL ter no topo uma barra com os módulos do aplicativo, na ordem Início, Contas, Contas a
pagar, Investimentos, Planejador, Relatórios e Categorias, com o módulo atual destacado, e MUST
abrir na página Início. Módulo ainda não entregue MUST aparecer desativado. Cada página SHALL ter
uma faixa de título com os comandos da página, um painel à esquerda com as tarefas e as visões da
página, o conteúdo e, quando houver totais, uma barra de situação no rodapé. Abaixo de 1000 de
largura o painel de tarefas MUST sumir, sem barra de rolagem horizontal até a largura mínima da
janela (720).

#### Scenario: Abertura do aplicativo
- **WHEN** o aplicativo inicia
- **THEN** a barra de módulos mostra Início destacado, Contas disponível e os demais módulos desativados

#### Scenario: Trocar de módulo
- **WHEN** o usuário escolhe Contas na barra de módulos
- **THEN** o gerenciador de contas é aberto e Contas passa a ser o módulo destacado

#### Scenario: Janela estreita
- **WHEN** o usuário reduz a janela para menos de 1000 de largura
- **THEN** o painel de tarefas some e o conteúdo se ajusta sem rolagem horizontal

### Requirement: Retorno ao usuário padronizado
O aplicativo SHALL confirmar ações concluídas com uma notificação passageira na janela, pedir
confirmação de exclusão num pop-up dentro da janela e mostrar erros de validação junto ao
formulário. Uma ação recusada fora de formulário MUST ser avisada na notificação passageira. O aplicativo MUST NOT usar a caixa de mensagem do Windows, exceto no erro fatal de
inicialização.

#### Scenario: Ação concluída
- **WHEN** o usuário salva uma conta com sucesso
- **THEN** aparece uma notificação passageira confirmando a ação

#### Scenario: Erro de validação
- **WHEN** o usuário tenta salvar um formulário com dado inválido
- **THEN** o formulário permanece aberto e mostra a mensagem do erro

### Requirement: Tema claro, escuro ou do Windows
O usuário SHALL poder escolher, na barra de módulos, entre o tema do Windows, o claro e o escuro. A
escolha MUST valer para todas as telas e continuar valendo nas próximas aberturas do aplicativo.
No tema do Windows, as telas MUST trocar junto quando o Windows trocar.

#### Scenario: Escolher o tema claro
- **WHEN** o Windows está no tema escuro e o usuário escolhe o tema claro
- **THEN** todas as telas passam para o tema claro e abrem no tema claro na próxima vez

#### Scenario: Acompanhar o Windows
- **WHEN** o tema escolhido é o do Windows e o usuário muda o Windows de claro para escuro
- **THEN** as telas passam para o tema escuro
