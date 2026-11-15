import { Check, Pencil, Trash2, X } from 'lucide-react';
import { useState } from 'react';
import { api, type Cadastro, ErroRegra, type TipoCadastro } from '../api';
import { AvisoErro, Botao, Confirmacao, estiloEntrada, Popup } from '../comum/ui';
import { useDados } from '../comum/useDados';

const textos: Record<TipoCadastro, { titulo: string; vazio: string }> = {
  instituicoes: { titulo: 'Instituições financeiras', vazio: 'Nenhuma instituição ainda. Elas são criadas no formulário da conta.' },
  grupos: { titulo: 'Grupos de contas', vazio: 'Nenhum grupo ainda. Eles são criados no formulário da conta.' },
};

/** Pop-up de manutenção de instituições ou de grupos: listar, renomear e excluir. */
export function CadastrosDialogo(props: { tipo: TipoCadastro; aoFechar: () => void; aoAlterar: () => void }) {
  const [itens, recarregar] = useDados(() => api.cadastros(props.tipo), [props.tipo]);
  const [editando, setEditando] = useState<Cadastro | null>(null);
  const [novoNome, setNovoNome] = useState('');
  const [excluindo, setExcluindo] = useState<Cadastro | null>(null);
  const [erro, setErro] = useState<string | null>(null);

  async function executar(operacao: () => Promise<void>) {
    setErro(null);
    try {
      await operacao();
      setEditando(null);
      props.aoAlterar();
      await recarregar();
    } catch (falha) {
      if (!(falha instanceof ErroRegra)) {
        throw falha;
      }

      setErro(falha.message);
    }
  }

  return (
    <>
      <Popup titulo={textos[props.tipo].titulo} aoFechar={props.aoFechar} rodape={<Botao onClick={props.aoFechar}>Fechar</Botao>}>
        <AvisoErro mensagem={erro} />
        {itens?.length === 0 && <p className="text-suave">{textos[props.tipo].vazio}</p>}
        <ul className="grid max-h-80 gap-0.5 overflow-auto">
          {itens?.map((item) => (
            <li key={item.id} className="flex h-10 items-center gap-2 rounded-md px-2 hover:bg-fundo">
              {editando?.id === item.id ? (
                <form
                  className="flex flex-1 items-center gap-1"
                  onSubmit={(evento) => {
                    evento.preventDefault();
                    void executar(() => api.renomearCadastro(props.tipo, item.id, novoNome));
                  }}
                >
                  <input className={estiloEntrada} aria-label="Novo nome" autoFocus value={novoNome} onChange={(e) => setNovoNome(e.target.value)} />
                  <Botao type="submit" variante="discreto" icone={Check} aria-label="Confirmar nome" />
                  <Botao variante="discreto" icone={X} aria-label="Cancelar" onClick={() => setEditando(null)} />
                </form>
              ) : (
                <>
                  <span className="flex-1 truncate font-medium">{item.nome}</span>
                  <span className="text-[11.5px] text-fraco">
                    {item.quantidadeContas} {item.quantidadeContas === 1 ? 'conta' : 'contas'}
                  </span>
                  <Botao
                    variante="discreto"
                    icone={Pencil}
                    aria-label={`Renomear ${item.nome}`}
                    onClick={() => {
                      setEditando(item);
                      setNovoNome(item.nome);
                      setErro(null);
                    }}
                  />
                  <Botao variante="discreto" icone={Trash2} aria-label={`Excluir ${item.nome}`} onClick={() => setExcluindo(item)} />
                </>
              )}
            </li>
          ))}
        </ul>
      </Popup>
      {excluindo && (
        <Confirmacao
          titulo="Excluir cadastro"
          texto={`"${excluindo.nome}" será apagado de vez.`}
          confirmar="Excluir"
          aoCancelar={() => setExcluindo(null)}
          aoConfirmar={() => {
            const alvo = excluindo;
            setExcluindo(null);
            void executar(() => api.excluirCadastro(props.tipo, alvo.id));
          }}
        />
      )}
    </>
  );
}
