## 1. Domínio

- [x] 1.1 Em `TipoContaExtensoes`:
  - `TemSaldoMinimo` passa a ser Corrente e Poupança;
  - criar `PedeNumeroNaCriacao`, verdadeiro só na Corrente.

  Em `EntidadeNomeada`, tornar `ValidarNome` `public static` (decisão 5). Build sem avisos.
- [x] 1.2 Testes de domínio passando:
  - atualizar `TipoContaExtensoesTestes` com os tipos de cada capacidade;
  - `ContaTestes`: saldo mínimo recusado na Dinheiro; os testes que usavam saldo mínimo em Dinheiro
    passam a usar Corrente;
  - `ValidarNome` recusa vazio e nome com 101 caracteres.

## 2. Persistência

- [x] 2.1 Gerar a migration `LimparSaldoMinimoDinheiro` com `dotnet ef migrations add`. No `Up`, o
  `UPDATE Contas SET SaldoMinimo = NULL WHERE Tipo = 'Dinheiro'`; o `Down` fica vazio, com o
  comentário de que o valor não volta. Conferir que o snapshot não mudou e **mostrar o arquivo ao
  usuário para revisão** antes de seguir.
- [x] 2.2 Teste em `Money.Data.Tests`, passando: banco temporário migrado até a migration anterior,
  com uma Dinheiro e uma Corrente com saldo mínimo; depois da migration nova, a Dinheiro fica sem
  saldo mínimo e a Corrente mantém o valor. Ajustar `ContasPersistenciaTestes` se usar saldo mínimo
  em Dinheiro.

## 3. Serviço e API

- [x] 3.1 `TipoContaDto` expõe `PedeNumeroNaCriacao`. Criar `ContaServico.ConferirNomeAsync(nome, ct)`
  com a validação do nome e a unicidade (`Nomes.GarantirUnico`), conforme a decisão 5. Testes em
  `ContaServicoTestes`, passando:
  - nome livre aceito;
  - nome repetido recusado sem diferenciar maiúsculas, inclusive com conta encerrada;
  - nome vazio recusado.

  Ajustar os testes que usavam saldo mínimo em Dinheiro.
- [x] 3.2 Rota `GET /api/contas/nome-disponivel?nome=` em `ContasEndpoints`: 200 sem corpo se aceito, 400
  `{ mensagem }` se recusado. Testes em `ContasApiTestes`, passando:
  - os dois casos da rota;
  - `/api/contas/opcoes` traz `pedeNumeroNaCriacao` e `temSaldoMinimo` falso na Dinheiro;
  - saldo mínimo enviado para conta Dinheiro dá 400.

## 4. Telas

- [x] 4.1 `api.ts`: `pedeNumeroNaCriacao` no tipo de conta e `conferirNomeConta(nome)`, que lança
  `ErroRegra` na recusa. Em `CamposConta.tsx`, extrair `CampoNumeroConta` e `CampoSaldoMinimo`;
  `CamposComplementares` passa a montá-los e os Detalhes e o pop-up continuam iguais. `npm run build`
  sem erro.
- [x] 4.2 `NovaContaAssistente`, conforme as decisões 3 e 5:
  - passos com `aplica` e `conferencia`;
  - passo "Qual é o número desta conta?" entre o nome e o saldo, com `CampoNumeroConta`;
  - `CampoSaldoMinimo` no passo do saldo;
  - "Passo x de n" sobre os passos visíveis;
  - conferência do nome no Avançar do passo 3.

  `useFormularioConta.salvar` só manda o número de conta nova se o tipo pede o número na criação.
  `npm run build` sem erro.
- [x] 4.3 Testes de tela passando (`npm run testar`):
  - `contas.spec.ts` ganha a Corrente criada com número e saldo mínimo, conferidos nos Detalhes;
  - Dinheiro com quatro passos e sem saldo mínimo;
  - nome repetido barrado no passo 3;
  - em `detalhes.spec.ts`, o saldo mínimo passa a ser testado numa Corrente, e a Dinheiro não mostra
    o campo.

## 5. Fechamento

- [x] 5.1 Validação completa:
  - `dotnet build MeuMoney.slnx` sem avisos;
  - `dotnet test MeuMoney.slnx` passando;
  - `npm run testar --prefix src/Money.Web` passando.
- [x] 5.2 Abrir o aplicativo com banco temporário (`Porta=5199`) e conferir por captura, nos temas
  claro e escuro:
  - o assistente da Corrente nos cinco passos;
  - a mensagem de nome repetido no passo 3;
  - o assistente da Dinheiro em quatro passos.

  Entregar ao usuário o que conferir.
