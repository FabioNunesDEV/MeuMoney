## Context

`ContaDialogo.tsx` cria e edita contas num só formulário. Ele guarda o estado de todos os campos,
monta o `SalvarConta` e chama `api.criarConta` ou `api.atualizarConta`. É aberto por `Contas.tsx`
(nova conta e edição) e por `ContaPagina.tsx` (edição). O back-end já recebe a conta inteira de uma
vez (`SalvarContaDto`), com instituição e grupo por nome, e cria os dois quando o nome é novo. O
motivo da mudança está em proposal.md; os requisitos, em specs/contas/spec.md.

## Goals / Non-Goals

**Goals:**
- Assistente de 4 passos na criação, sem duplicar campos nem a lógica de gravação da edição.
- Navegação toda pelo teclado: Tab, setas na lista de tipos, Enter para avançar, Esc para cancelar.

**Non-Goals:**
- Mudar API, DTO, regra de domínio ou banco.
- Mudar a edição de conta. Ela continua num formulário único até o change `detalhes-conta-editavel`.
- Tipos novos do Money (Aposentadoria, Empréstimo etc.).
- Validar no navegador regra que é do servidor, como nome único ou casas decimais da moeda.

## Decisions

### 1. Estado num hook, campos em seções, duas cascas

- `useFormularioConta(original)`, em `contas/useFormularioConta.ts`, concentra o estado dos campos,
  a sugestão de nome e o `salvar()` que hoje vivem no `ContaDialogo`.
- `contas/CamposConta.tsx` tem uma seção por assunto: `CamposInstituicao`, `CamposTipo`,
  `CamposNome` (nome, grupo, favorita) e `CamposSaldo` (moeda e saldo, ou conta caixa no
  Investimento).
- `NovaContaAssistente.tsx` mostra uma seção por passo. `ContaDialogo.tsx` passa a servir só à
  edição e mostra todas as seções juntas.

*Alternativa:* um único componente com `if (criando)` em cada bloco. Foi descartada porque mistura
dois fluxos de navegação no mesmo JSX. Duplicar os campos em dois arquivos fere "não duplicar
código".

### 2. Validação de passo só de preenchimento

O Avançar só confere o que a tela consegue ver:
- passo 1: com "É mantida no(a)" marcado, o nome da instituição não pode estar vazio;
- passo 3: o nome da conta não pode estar vazio.

Moeda, saldo, nome repetido e conta caixa são validados pelo servidor no Concluir, como hoje. A
mensagem aparece no `AvisoErro` do último passo e o usuário volta com Voltar. A API devolve só
`{ mensagem }`, sem indicar o campo, então o assistente não tenta pular sozinho para o passo do
erro. O saldo em formato inválido (`lerValor`) é conferido ao avançar do passo 4, como hoje no salvar.

### 3. Instituição com duas opções explícitas

- Dois rádios, como no Money: "É mantida no(a)", com o campo de escolha ou digitação (`datalist`
  das instituições), e "Não é mantida em banco, corretora ou outra instituição".
- Marcar o segundo desativa o campo e manda `nomeInstituicao: null`. O texto digitado fica guardado
  e volta se o usuário remarcar o primeiro.
- A edição usa a mesma seção. Hoje lá existe só um campo opcional; a mudança vem junto porque a
  seção é compartilhada e fica mais clara.

### 4. Tipo como lista de rádios com painel de descrição

- Os 7 tipos viram rádios nativos estilizados (ícone `SeloTipo` + nome), em coluna à esquerda. A
  descrição do escolhido fica num painel à direita.
- O rádio nativo dá as setas para cima e para baixo e o anúncio do leitor de tela sem código extra.
- Na edição a seção aparece desativada, porque o tipo não muda depois da criação.
- O tipo inicial continua Corrente.

### 5. Sugestão de nome ao chegar no passo 3

Continua a regra atual: enquanto o usuário não digitar um nome próprio, o nome acompanha
"<instituição> <tipo>". A sugestão é recalculada ao trocar a instituição ou o tipo. Por isso, voltar
e mudar o tipo atualiza a sugestão, mas não um nome digitado. Sem instituição, não há sugestão.

### 6. Grupo e favorita no passo do nome

O Money deixa grupo e favorita para a tela de Detalhes. Aqui eles continuam na criação, como
opcionais, para o assistente não perder o que o formulário atual já faz. O passo 3 é o lugar que
menos pesa.

### 7. Moldura do assistente

- Cada passo tem título em forma de pergunta ("Onde esta conta é mantida?", "Que tipo de conta você
  quer criar?", "Que nome deseja dar a esta conta?", "Qual é o saldo desta conta?") e o indicador
  "Passo N de 4".
- A área de conteúdo tem altura mínima fixa, para o pop-up não pular entre passos.
- Rodapé com Cancelar, Voltar (desativado no passo 1) e Avançar ou Concluir na variante `primario`.
- Ao trocar de passo, o foco vai para o primeiro campo do passo novo. O formulário tem um botão
  `submit` oculto, como hoje, então o Enter em qualquer campo aciona Avançar ou Concluir.
- O `Popup` de `comum/ui.tsx` é reaproveitado sem mudança: o rodapé já é livre.

### 8. Descrições dos tipos

As descrições em `TiposConta.cs` são reescritas para orientar a escolha no passo 2, com texto
próprio (sem copiar o Money). Exemplo para Dinheiro: "Dinheiro em espécie, como a carteira, para
acompanhar os gastos do dia a dia. Também serve de caixa de uma corretora." O teste
`ListarTipos_DevolveSeteTiposComDescricao` continua valendo como está.

## Risks / Trade-offs

- [Mais cliques para quem já sabe o que quer] → Enter avança e os passos são curtos. A edição segue
  num formulário só.
- [Erro do servidor aparece no passo 4, longe do campo] → A mensagem da API já cita o campo ("Já
  existe conta com esse nome") e Voltar mantém tudo preenchido.
- [Edição e criação divergirem com o tempo] → As duas usam as mesmas seções e o mesmo hook. Só a
  casca muda.

## Migration Plan

Só telas, sem migration nem mudança de API. Desfazer é reverter o commit.
