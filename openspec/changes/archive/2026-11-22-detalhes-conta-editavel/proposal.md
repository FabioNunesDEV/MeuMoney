## Why

Na página da conta, a visão Detalhes só mostra o cadastro. Para mudar qualquer dado é preciso abrir o
pop-up de edição, e quatro campos do "Detalhes da conta" do Money 99 não existem no MeuMoney. No
Money 99, os Detalhes são o próprio formulário do cadastro. Este é o último dos quatro changes do
roteiro da conta Dinheiro (A: criação em passos, B: lançamentos, C: histórico, D: detalhes).

## What Changes

- **Detalhes editável na página da conta**, em todos os tipos de conta.
  - Os Detalhes viram um formulário com os campos do cadastro, sempre editáveis, e os botões
    Salvar e Desfazer, ativos só quando há alteração.
  - Ficam só para leitura o tipo, o saldo atual e a situação (aberta ou encerrada). Encerrar e
    reabrir continuam nos comandos da faixa de título.
  - O comando Editar da página da conta passa a levar aos Detalhes, com o foco no primeiro campo,
    em vez de abrir o pop-up.
  - Recusa da API aparece dentro do formulário, como hoje no pop-up. Gravação concluída mostra o
    aviso passageiro.
- **Campos novos no cadastro da conta**, todos opcionais:
  - **Número da conta**: texto livre, como agência e conta ou o final do cartão;
  - **Abreviação**: nome curto da conta;
  - **Comentário**: texto longo livre;
  - **Saldo mínimo**: valor na moeda da conta, só nas contas Corrente, Poupança e Dinheiro. Neste
    change ele só é guardado e mostrado. Aviso de saldo abaixo do mínimo fica para um change próprio.
- **Pop-up de edição no gerenciador**: continua existindo, com os mesmos campos dos Detalhes,
  inclusive os novos.
- **Assistente de nova conta**: não muda. Os campos novos ficam para a edição, como no Money 99,
  cujo assistente também não os pede.
- Fica de fora o botão "Configuração on-line" do Money 99.

## Capabilities

### New Capabilities

Nenhuma.

### Modified Capabilities

- `contas`:
  - "Editar conta" passa a incluir os campos novos e as regras do saldo mínimo;
  - "Página da conta" passa a ter os Detalhes editáveis, e o Editar da página leva a eles.

## Impact

- `Money.Domain`: `Conta` ganha número, abreviação, comentário e saldo mínimo, com validação de
  tamanho, de tipo e de casas decimais da moeda.
- `Money.Data` e `Money.Data.Sqlite`: mapeamento das colunas novas e uma migration (quatro colunas
  opcionais na tabela `Contas`, sem perda de dados). A migration vai para a revisão do usuário.
- `Money.Services`: `SalvarContaDto` e `ContaDto` com os campos novos, e o `ContaServico` aplicando-os.
- `Money.Api`: sem rota nova. As rotas de conta passam a trafegar os campos novos.
- `Money.Web`:
  - `api.ts`;
  - `useFormularioConta` e `CamposConta`, com os campos novos;
  - `ContaDialogo`;
  - `ContaPagina`, com os Detalhes em formulário e o Editar levando a eles.
- Testes: domínio, serviço, persistência, API e tela.
