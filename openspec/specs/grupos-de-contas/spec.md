# grupos-de-contas Specification

## Purpose
Permite ao usuário criar grupos próprios (por exemplo, "Exterior" ou "Reserva de emergência") para
organizar as contas no gerenciador, independentemente do tipo e da instituição.

## Requirements

### Requirement: Criar grupo no formulário da conta
O sistema SHALL permitir escolher um grupo existente, digitar um nome novo ou deixar a conta sem
grupo; ao salvar a conta, um nome novo MUST criar o grupo. O nome MUST ter de 1 a 100 caracteres sem
contar espaços nas pontas e MUST ser único sem diferenciar maiúsculas de minúsculas.

#### Scenario: Grupo novo
- **WHEN** o usuário digita "Exterior" no campo de grupo, que ainda não existe, e salva a conta
- **THEN** o grupo "Exterior" é criado e a conta passa a pertencer a ele

#### Scenario: Conta sem grupo
- **WHEN** o usuário salva uma conta com o campo de grupo vazio
- **THEN** a conta fica sem grupo e, ao agrupar por grupo, aparece em "Sem grupo"

### Requirement: Renomear e excluir grupo
O sistema SHALL permitir listar, renomear e excluir grupos. Renomear MUST refletir em todas as
contas do grupo. Excluir um grupo MUST, após confirmação, deixar as contas dele sem grupo, sem
alterar mais nada nelas.

#### Scenario: Renomear grupo
- **WHEN** o usuário renomeia o grupo "Exterior" para "Internacional"
- **THEN** as contas do grupo aparecem agrupadas sob "Internacional"

#### Scenario: Excluir grupo em uso
- **WHEN** o usuário exclui um grupo que tem contas e confirma
- **THEN** o grupo deixa de existir e as contas dele ficam sem grupo
