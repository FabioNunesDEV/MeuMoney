# instituicoes-financeiras Specification

## Purpose
Mantém a lista de instituições financeiras (bancos, corretoras e afins) que o usuário associa às
contas, para evitar grafias diferentes do mesmo nome e permitir agrupar as contas por instituição.

## Requirements

### Requirement: Criar instituição no formulário da conta
O sistema SHALL permitir escolher uma instituição existente ou digitar um nome novo no formulário
da conta; ao salvar a conta, um nome novo MUST criar a instituição. O nome MUST ter de 1 a 100
caracteres sem contar espaços nas pontas.

#### Scenario: Instituição nova
- **WHEN** o usuário digita "Banco A" no campo de instituição, que ainda não existe, e salva a conta
- **THEN** a instituição "Banco A" é criada e associada à conta

#### Scenario: Instituição existente com outra caixa
- **WHEN** o usuário digita "banco a" e já existe "Banco A"
- **THEN** a conta é associada à instituição existente, sem criar outra

### Requirement: Nome de instituição único
O nome da instituição MUST ser único sem diferenciar maiúsculas de minúsculas.

#### Scenario: Renomear para nome existente
- **WHEN** o usuário tenta renomear uma instituição para o nome de outra já existente
- **THEN** o sistema recusa e indica que o nome já existe

### Requirement: Renomear e excluir instituição
O sistema SHALL permitir listar, renomear e excluir instituições. Renomear MUST refletir em todas as
contas associadas. Instituição associada a alguma conta, aberta ou encerrada, MUST NOT ser excluída.

#### Scenario: Renomear instituição
- **WHEN** o usuário renomeia "Banco A" para "Banco A S.A."
- **THEN** todas as contas associadas passam a exibir "Banco A S.A."

#### Scenario: Excluir instituição em uso
- **WHEN** o usuário tenta excluir uma instituição associada a uma conta
- **THEN** o sistema recusa e indica quantas contas a usam

#### Scenario: Excluir instituição sem uso
- **WHEN** o usuário exclui uma instituição sem contas associadas e confirma
- **THEN** a instituição deixa de existir
