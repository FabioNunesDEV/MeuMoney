# registro-de-falhas Specification

## Purpose
Garante que as falhas do aplicativo fiquem registradas num arquivo texto local, para que possam ser
investigadas depois, sem registrar a operação normal nem enviar nada para fora da máquina.

## Requirements

### Requirement: Registro de falhas em arquivo
O aplicativo SHALL registrar as falhas em arquivo texto dentro da pasta `logs` da pasta de dados do
aplicativo (por padrão `%LOCALAPPDATA%\MeuMoney\logs`), um arquivo por dia. Cada registro MUST ter
data e hora, o nível, a mensagem e os detalhes da exceção. O aplicativo MUST registrar só falhas
(nível de erro ou mais grave), inclusive as registradas pelo servidor local e pelo acesso ao banco,
e MUST NOT gravar o registro em console nem em outro destino.

#### Scenario: Erro numa chamada à API
- **WHEN** uma chamada das telas à API falha com uma exceção não tratada
- **THEN** a falha é gravada no arquivo do dia, com a exceção

#### Scenario: Uso normal não gera registro
- **WHEN** o usuário abre o aplicativo, navega e cria uma conta sem nenhuma falha
- **THEN** nenhum registro é gravado

#### Scenario: Regra de negócio recusada não é falha
- **WHEN** o sistema recusa uma ação por regra de negócio e devolve a mensagem ao usuário
- **THEN** nenhum registro é gravado

### Requirement: Falhas fora do fluxo normal
O aplicativo SHALL registrar a falha de inicialização antes de mostrar a caixa de mensagem de erro
fatal, e SHALL registrar as exceções não tratadas da janela e das tarefas em segundo plano. Uma
falha que encerra o aplicativo MUST ficar gravada no arquivo antes do encerramento.

#### Scenario: Falha na inicialização
- **WHEN** o aplicativo não consegue iniciar, por exemplo com um provedor de banco não suportado na configuração
- **THEN** a falha é gravada no arquivo do dia e a caixa de mensagem de erro fatal aparece

### Requirement: Retenção dos arquivos de registro
O aplicativo SHALL guardar os arquivos de registro dos últimos 30 dias e MUST apagar os mais
antigos.

#### Scenario: Arquivos antigos
- **WHEN** existem arquivos de registro de mais de 30 dias e o aplicativo grava um novo registro
- **THEN** os arquivos mais antigos que o limite são apagados
