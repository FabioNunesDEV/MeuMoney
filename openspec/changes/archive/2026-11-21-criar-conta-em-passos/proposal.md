## Why

Hoje a nova conta abre num formulário único, com tudo ao mesmo tempo. O usuário quer seguir o
roteiro do Money 99, em que a criação vem em passos curtos, cada um com uma só pergunta. Este é o
primeiro dos quatro changes do roteiro da conta Dinheiro. Os seguintes são a movimentação, o
histórico e os detalhes editáveis.

## What Changes

- O pop-up "Nova conta" passa a ser um assistente em 4 passos, com Voltar, Avançar e Concluir:
  1. **Instituição**: "É mantida no(a)", escolhendo uma instituição existente ou digitando um nome
     novo, ou "Não é mantida em banco, corretora ou outra instituição".
  2. **Tipo**: lista dos 7 tipos atuais, com a descrição do tipo escolhido ao lado.
  3. **Nome**: nome da conta, com a sugestão "<instituição> <tipo>". Grupo e favorita, que são
     opcionais, também ficam neste passo.
  4. **Saldo e moeda**: saldo inicial e moeda. No Investimento, o saldo dá lugar à escolha da
     conta caixa.
- A edição de conta continua num formulário único, com todos os campos à vista e o tipo travado.
- A lista de tipos não muda: Corrente, Poupança, Dinheiro, Cartão de crédito, Investimento, Ativo e
  Passivo. Os demais tipos do Money ficam de fora até serem pedidos.
- As descrições dos tipos são revistas para orientar a escolha no passo 2, com texto próprio.
- O CLAUDE.md troca a regra "assistente com várias janelas → um pop-up com formulário curto" por
  "um pop-up com passos curtos".
- `Money99/` entra no `.gitignore`. É a pasta local com a instalação do Money 99, usada como
  referência.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `contas`: o requisito "Criar conta" passa a descrever o assistente em passos, com a ordem, a
  navegação, a opção explícita de não ter instituição e a validação de cada passo. Os cenários da
  escolha do tipo, do investimento sem saldo e do atalho Ctrl+N passam a citar o assistente.

As regras de instituição e de grupo (escolher, criar pelo nome ou deixar em branco) continuam as
mesmas e só mudam de lugar na tela. Por isso `instituicoes-financeiras` e `grupos-de-contas` ficam
sem delta.

## Impact

- `src/Money.Web/src/contas/ContaDialogo.tsx`: reorganizado em seções reaproveitadas pelo
  assistente (uma seção por passo) e pela edição (todas juntas).
- `src/Money.Services/Contas/TiposConta.cs`: só as descrições mudam.
- Sem mudança de API, de banco ou de regra de negócio: `SalvarContaDto` e `ContaServico` já recebem
  todos os dados de uma vez, no Concluir.
- `CLAUDE.md` (tabela "Do Money 99 para o MeuMoney") e `.gitignore`.
