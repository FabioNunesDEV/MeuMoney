## ADDED Requirements

### Requirement: Barra de título da janela
A barra de título da janela SHALL ter a mesma cor da barra de módulos do tema em uso, com o título
e os botões da janela (minimizar, maximizar e fechar) em cor clara. Ao trocar de tema, inclusive
quando o tema escolhido é o do Windows e o Windows troca, a barra de título MUST mudar junto com as
telas. Enquanto as telas ainda carregam, a barra de título MUST NOT aparecer branca. No Windows que
não permite colorir a barra de título, a barra MUST ficar escura, com o título legível.

#### Scenario: Tema escuro
- **WHEN** o usuário escolhe o tema escuro
- **THEN** a barra de título fica na cor da barra de módulos do tema escuro, com título e botões claros

#### Scenario: Tema claro
- **WHEN** o usuário escolhe o tema claro
- **THEN** a barra de título fica na cor da barra de módulos do tema claro, com título e botões claros

#### Scenario: Acompanhar o Windows
- **WHEN** o tema escolhido é o do Windows e o usuário muda o Windows de claro para escuro
- **THEN** a barra de título passa para a cor da barra de módulos do tema escuro junto com as telas

#### Scenario: Abertura do aplicativo
- **WHEN** o aplicativo abre e as telas ainda não terminaram de carregar
- **THEN** a barra de título aparece escura, sem clarão branco, e assume a cor da barra de módulos assim que as telas carregam
