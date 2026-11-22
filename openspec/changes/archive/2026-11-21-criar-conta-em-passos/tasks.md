## 1. Descrições e repositório

- [x] 1.1 Reescrever as descrições dos 7 tipos em `Money.Services/Contas/TiposConta.cs` com texto próprio que oriente a escolha; `ListarTipos_DevolveSeteTiposComDescricao` passando
- [x] 1.2 Acrescentar `Money99/` ao `.gitignore`; `git check-ignore Money99/qualquer` confirma

## 2. Estado e seções do formulário

- [x] 2.1 Extrair de `ContaDialogo.tsx` o hook `contas/useFormularioConta.ts` (estado dos campos, sugestão de nome, montagem do `SalvarConta` e `salvar()` com `ErroRegra`); `npm run build --prefix src/Money.Web` sem erro
- [x] 2.2 Criar `contas/CamposConta.tsx` com `CamposInstituicao` (rádios "É mantida no(a)" / "Não é mantida…" + campo com `datalist`), `CamposTipo` (rádios nativos com `SeloTipo` e painel de descrição), `CamposNome` (nome, grupo, favorita) e `CamposSaldo` (moeda e saldo, ou conta caixa no Investimento); build sem erro
- [x] 2.3 Reescrever `ContaDialogo.tsx` como pop-up só de edição, com todas as seções juntas e o tipo desativado; build sem erro

## 3. Assistente de nova conta

- [x] 3.1 Criar `contas/NovaContaAssistente.tsx`: 4 passos com título-pergunta, "Passo N de 4", altura mínima fixa, Cancelar/Voltar/Avançar→Concluir, foco no primeiro campo a cada passo e Enter pelo `submit` oculto; build sem erro
- [x] 3.2 Validar ao avançar: instituição vazia com "É mantida" marcada (passo 1), nome vazio (passo 3) e formato do saldo (passo 4); a recusa do servidor ao concluir aparece em `AvisoErro` sem fechar; build sem erro
- [x] 3.3 Trocar em `Contas.tsx` a abertura da nova conta (botão, tarefa do painel, estado vazio e Ctrl+N) para `NovaContaAssistente`, mantendo `ContaDialogo` na edição; build sem erro

## 4. Documentação e verificação

- [x] 4.1 Corrigir no `CLAUDE.md` a linha "Assistente com várias janelas → Um pop-up com formulário curto" para "um pop-up com passos curtos"
- [x] 4.2 `dotnet build MeuMoney.slnx` sem avisos e `dotnet test MeuMoney.slnx` passando
- [x] 4.3 Conferir por captura com banco temporário (`Database__ConnectionString`, `Porta=5199`), nos temas claro e escuro: criar Dinheiro sem instituição, Corrente com instituição nova (nome sugerido), Investimento com caixa nova, Voltar preservando dados, nome repetido recusado no passo 4, Enter e Esc; editar uma conta pelo formulário único
