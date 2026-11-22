import { ListOrdered, MoreHorizontal, Plus } from 'lucide-react';
import { type KeyboardEvent, useEffect, useState } from 'react';
import { api, type Conta, type Lancamento } from '../api';
import { classes, dataParaTexto, dinheiro } from '../comum/formato';
import { Botao, Confirmacao, estiloEntrada, MenuBotao, MenuContexto, Popup, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';
import { executarComAviso } from '../contas/acoes';
import { acoesLancamento } from './acoesLancamento';
import { FormularioLancamento } from './FormularioLancamento';
import { useFormularioLancamento } from './useFormularioLancamento';

const marcaSituacao: Partial<Record<Lancamento['situacao'], string>> = { Compensado: 'C', Reconciliado: 'R' };

function categoriaDaLinha(l: Lancamento): string {
  if (l.tipo === 'TransferenciaSaida') {
    return `Para: ${l.nomeOutraConta ?? ''}`;
  }

  if (l.tipo === 'TransferenciaEntrada') {
    return `De: ${l.nomeOutraConta ?? ''}`;
  }

  return [l.nomeCategoria, l.nomeSubcategoria].filter(Boolean).join(' : ');
}

// Setas sobem e descem entre as linhas da grade, como numa lista.
function navegar(evento: KeyboardEvent<HTMLTableRowElement>) {
  const alvo =
    evento.key === 'ArrowDown'
      ? evento.currentTarget.nextElementSibling
      : evento.key === 'ArrowUp'
        ? evento.currentTarget.previousElementSibling
        : null;
  if (alvo instanceof HTMLElement) {
    evento.preventDefault();
    alvo.focus();
  }
}

/**
 * Movimentação da conta, como no Money: a grade dos lançamentos com o saldo linha a linha e, embaixo,
 * o formulário em abas. `aoMudar` recarrega a conta (saldo do rodapé) depois de cada gravação.
 */
export function Movimentacao(props: { conta: Conta; aoMudar: () => Promise<void>; abrirConta: (id: string) => void }) {
  const { conta } = props;
  const [movimentacao, recarregarMovimentacao] = useDados(() => api.movimentacao(conta.id), [conta.id]);
  const [opcoes, recarregarOpcoes] = useDados(() => api.opcoesLancamento(conta.id), [conta.id]);
  const [mover, setMover] = useState<Lancamento | null>(null);
  const [excluir, setExcluir] = useState<Lancamento | null>(null);
  const [destino, setDestino] = useState('');

  const recarregar = async () => {
    await Promise.all([recarregarMovimentacao(), recarregarOpcoes(), props.aoMudar()]);
  };
  const f = useFormularioLancamento(conta, opcoes, recarregar);

  // Ctrl+N abre um lançamento novo, como o Novo do formulário.
  useEffect(() => {
    const aoTeclar = (evento: globalThis.KeyboardEvent) => {
      if (evento.ctrlKey && evento.key.toLowerCase() === 'n') {
        evento.preventDefault();
        f.novo();
      }
    };
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [f]);

  const contexto = {
    encerrada: conta.encerrada,
    editar: (l: Lancamento) => f.editar(l),
    irParaConta: props.abrirConta,
    pedirMover: (l: Lancamento) => {
      setDestino('');
      setMover(l);
    },
    pedirExcluir: setExcluir,
    recarregar,
  };
  const destinos = (opcoes?.contas ?? []).filter((c) => c.id !== conta.id && c.id !== mover?.outraContaId);
  const linhas = movimentacao?.lancamentos ?? [];

  return (
    <div className="flex h-full min-h-0 flex-col overflow-hidden rounded-lg border border-borda bg-painel">
      <div className="min-h-0 flex-1 overflow-auto">
        <table className="w-full border-collapse text-left">
          <thead className="sticky top-0 z-10 bg-secao text-[11.5px] text-sobre-barra">
            <tr>
              <th className="w-24 px-2 py-1 font-semibold">Data</th>
              <th className="px-2 py-1 font-semibold">Favorecido</th>
              <th className="px-2 py-1 font-semibold max-[820px]:hidden">Categoria</th>
              <th className="w-8 px-1 py-1 text-center font-semibold" title="Situação">
                C
              </th>
              <th className="w-28 px-2 py-1 text-right font-semibold">Gastar</th>
              <th className="w-28 px-2 py-1 text-right font-semibold">Receber</th>
              <th className="w-32 px-2 py-1 text-right font-semibold">Saldo</th>
              <th className="w-9">
                <span className="sr-only">Ações</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {linhas.map((l) => {
              const nulo = l.situacao === 'Nulo';
              const saida = l.tipo === 'Gasto' || l.tipo === 'TransferenciaSaida';
              const valor = <span className={classes('tabular-nums', nulo && 'text-fraco line-through')}>{dinheiro(l.valor, conta.moeda)}</span>;
              const acoes = acoesLancamento(l, contexto);
              return (
                <MenuContexto key={l.id} acoes={acoes}>
                  <tr
                    tabIndex={0}
                    aria-selected={f.escolhido?.id === l.id}
                    onClick={() => f.escolher(l)}
                    onDoubleClick={() => f.editar(l)}
                    onKeyDown={(evento) => {
                      if (evento.key === 'Enter' && evento.target === evento.currentTarget) {
                        f.escolher(l);
                      }

                      navegar(evento);
                    }}
                    className={classes(
                      'group cursor-default border-b border-borda outline-none focus-visible:bg-realce-fundo',
                      f.escolhido?.id === l.id ? 'bg-realce-fundo' : 'hover:bg-fundo',
                    )}
                  >
                    <td className="px-2 py-1 tabular-nums">{dataParaTexto(l.data)}</td>
                    <td className="max-w-0 truncate px-2 py-1">
                      {l.nomeFavorecido}
                      {nulo && <span className="ml-2 rounded border border-borda px-1 text-[10px] font-semibold text-suave uppercase">Anulado</span>}
                    </td>
                    <td className="max-w-0 truncate px-2 py-1 text-suave max-[820px]:hidden">{categoriaDaLinha(l)}</td>
                    <td className="px-1 py-1 text-center font-semibold text-suave">{marcaSituacao[l.situacao] ?? ''}</td>
                    <td className="px-2 py-1 text-right">{saida && valor}</td>
                    <td className="px-2 py-1 text-right">{!saida && valor}</td>
                    <td className="px-2 py-1 text-right">
                      <ValorSaldo valor={l.saldoApos} moeda={conta.moeda} forte={false} />
                    </td>
                    <td className="px-0.5">
                      <span className="opacity-0 group-focus-within:opacity-100 group-hover:opacity-100">
                        <MenuBotao
                          acoes={acoes}
                          gatilho={<Botao variante="discreto" icone={MoreHorizontal} aria-label={`Mais ações do lançamento de ${l.nomeFavorecido}`} />}
                        />
                      </span>
                    </td>
                  </tr>
                </MenuContexto>
              );
            })}
          </tbody>
        </table>

        {movimentacao && linhas.length === 0 && (
          <div className="mx-auto mt-10 mb-6 grid max-w-sm justify-items-center gap-2 text-center">
            <span className="grid size-12 place-items-center rounded-full bg-fundo text-fraco">
              <ListOrdered className="size-6" aria-hidden />
            </span>
            <p className="font-semibold">Nenhum lançamento nesta conta</p>
            <p className="text-suave">Use Novo para registrar o primeiro gasto, recebimento ou transferência.</p>
            {!conta.encerrada && (
              <Botao variante="primario" icone={Plus} className="mt-2" onClick={f.novo}>
                Novo lançamento
              </Botao>
            )}
          </div>
        )}
      </div>

      <FormularioLancamento f={f} />

      {mover && (
        <Popup
          titulo="Mover para conta"
          aoFechar={() => setMover(null)}
          rodape={
            <>
              <Botao onClick={() => setMover(null)}>Cancelar</Botao>
              <Botao
                variante="primario"
                disabled={!destino}
                onClick={() => {
                  const nome = destinos.find((c) => c.id === destino)?.nome ?? '';
                  const alvo = mover;
                  setMover(null);
                  f.esquecer();
                  void executarComAviso(() => api.moverLancamento(alvo.id, destino), `Lançamento movido para "${nome}".`, recarregar);
                }}
              >
                Mover
              </Botao>
            </>
          }
        >
          {destinos.length === 0 ? (
            <p className="text-suave">Não há outra conta Dinheiro aberta na mesma moeda para receber o lançamento.</p>
          ) : (
            <label className="grid gap-1">
              <span className="text-[11.5px] font-semibold text-suave">Conta de destino</span>
              <select className={estiloEntrada} value={destino} onChange={(e) => setDestino(e.target.value)}>
                <option value="">Escolha a conta</option>
                {destinos.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nome}
                  </option>
                ))}
              </select>
            </label>
          )}
        </Popup>
      )}

      {excluir && (
        <Confirmacao
          titulo="Excluir lançamento"
          texto={
            excluir.outraContaId
              ? `A transferência sai das duas contas ("${conta.nome}" e "${excluir.nomeOutraConta ?? ''}"). Esta ação não se desfaz.`
              : 'O lançamento sai da conta e o saldo é recalculado. Esta ação não se desfaz.'
          }
          confirmar="Excluir"
          aoConfirmar={() => {
            const alvo = excluir;
            f.esquecer();
            void executarComAviso(() => api.excluirLancamento(alvo.id), 'Lançamento excluído.', recarregar);
          }}
          aoCancelar={() => setExcluir(null)}
        />
      )}
    </div>
  );
}
