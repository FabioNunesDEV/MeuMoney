import { Folder, Landmark, LayoutGrid, List, MoreHorizontal, Plus, Search, Star, Wallet } from 'lucide-react';
import { useEffect, useState } from 'react';
import { toast } from 'sonner';
import { api, type Conta, type TipoCadastro } from '../api';
import { classes } from '../comum/formato';
import { aparenciaTipo, SeloTipo } from '../comum/tiposConta';
import { BlocoPainel, Botao, estiloEntradaBase, MenuBotao, MenuContexto, Pagina, Secao, Tarefa, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';
import { acoesDaConta } from './acoes';
import { CadastrosDialogo } from './CadastrosDialogo';
import { ContaDialogo } from './ContaDialogo';
import { NovaContaAssistente } from './NovaContaAssistente';

type Agrupamento = 'tipo' | 'instituicao' | 'grupo';
type Exibicao = 'icones' | 'lista';

const agrupamentos: { chave: Agrupamento; rotulo: string }[] = [
  { chave: 'tipo', rotulo: 'Por tipo' },
  { chave: 'instituicao', rotulo: 'Por instituição' },
  { chave: 'grupo', rotulo: 'Por grupo' },
];

/** Separa as contas em grupos conforme o critério; contas sem instituição ou sem grupo ficam por último. */
function agrupar(contas: Conta[], criterio: Agrupamento): { titulo: string; contas: Conta[] }[] {
  const chave = (conta: Conta) =>
    criterio === 'tipo' ? conta.nomeTipo : criterio === 'instituicao' ? conta.nomeInstituicao : conta.nomeGrupo;
  const semChave = criterio === 'instituicao' ? 'Sem instituição' : 'Sem grupo';
  const grupos = new Map<string, Conta[]>();
  for (const conta of contas) {
    const titulo = chave(conta) ?? semChave;
    grupos.set(titulo, [...(grupos.get(titulo) ?? []), conta]);
  }

  const lista = [...grupos].map(([titulo, doGrupo]) => ({ titulo, contas: doGrupo }));
  const ordemTipos = Object.keys(aparenciaTipo);
  return criterio === 'tipo'
    ? lista.sort((a, b) => ordemTipos.indexOf(a.contas[0].tipo) - ordemTipos.indexOf(b.contas[0].tipo))
    : lista.sort((a, b) => Number(a.titulo === semChave) - Number(b.titulo === semChave) || a.titulo.localeCompare(b.titulo, 'pt-BR'));
}

/** Gerenciador de contas: contas em ícones grandes ou em lista, agrupadas, com os saldos no rodapé. */
export function Contas({ abrirConta }: { abrirConta: (id: string) => void }) {
  const [mostrarEncerradas, setMostrarEncerradas] = useState(false);
  const [gerenciador, recarregar] = useDados(() => api.gerenciador(mostrarEncerradas), [mostrarEncerradas]);
  const [agrupamento, setAgrupamento] = useState<Agrupamento>('tipo');
  const [exibicao, setExibicao] = useState<Exibicao>('icones');
  const [busca, setBusca] = useState('');
  const [formulario, setFormulario] = useState<{ conta: Conta | null } | null>(null);
  const [cadastros, setCadastros] = useState<TipoCadastro | null>(null);

  // Ctrl+N abre a nova conta, como no restante do Windows.
  useEffect(() => {
    const aoTeclar = (evento: KeyboardEvent) => {
      if (evento.ctrlKey && evento.key.toLowerCase() === 'n') {
        evento.preventDefault();
        setFormulario({ conta: null });
      }
    };
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, []);

  const novaConta = () => setFormulario({ conta: null });
  const termo = busca.trim().toLocaleLowerCase('pt-BR');
  const visiveis =
    gerenciador?.contas.filter((c) => !termo || `${c.nome} ${c.nomeInstituicao ?? ''}`.toLocaleLowerCase('pt-BR').includes(termo)) ?? [];
  const haContas = gerenciador?.existemContas ?? false;

  return (
    <Pagina
      titulo="Gerenciador de contas"
      ferramentas={
        <>
          <Botao variante="barra" icone={Landmark} onClick={() => setCadastros('instituicoes')}>
            Instituições
          </Botao>
          <Botao variante="barra" icone={Folder} onClick={() => setCadastros('grupos')}>
            Grupos
          </Botao>
          <Botao variante="destaque" icone={Plus} title="Nova conta (Ctrl+N)" onClick={novaConta}>
            Nova conta
          </Botao>
        </>
      }
      painel={
        <>
          <p className="mb-5 px-2 text-[11.5px] leading-relaxed text-sobre-barra-suave">
            Aqui você vê um resumo de todas as suas contas, cria uma conta nova e altera ou encerra uma conta existente.
          </p>
          <BlocoPainel titulo="Tarefas">
            <Tarefa icone={Plus} aoClicar={novaConta}>
              Nova conta
            </Tarefa>
            <Tarefa icone={Landmark} aoClicar={() => setCadastros('instituicoes')}>
              Instituições financeiras
            </Tarefa>
            <Tarefa icone={Folder} aoClicar={() => setCadastros('grupos')}>
              Grupos de contas
            </Tarefa>
          </BlocoPainel>
          <BlocoPainel titulo="Visualizar">
            <Tarefa icone={LayoutGrid} ativo={exibicao === 'icones'} aoClicar={() => setExibicao('icones')}>
              Ícones grandes
            </Tarefa>
            <Tarefa icone={List} ativo={exibicao === 'lista'} aoClicar={() => setExibicao('lista')}>
              Lista
            </Tarefa>
          </BlocoPainel>
        </>
      }
      opcoes={
        haContas && (
          <>
            <div className="relative">
              <Search className="pointer-events-none absolute top-2 left-2.5 size-4 text-fraco" aria-hidden />
              <input
                className={classes(estiloEntradaBase, 'w-44 bg-fundo pl-8')}
                type="search"
                aria-label="Buscar conta"
                placeholder="Buscar conta"
                value={busca}
                onChange={(e) => setBusca(e.target.value)}
              />
            </div>
            <select
              className={classes(estiloEntradaBase, 'bg-fundo')}
              aria-label="Agrupar por"
              title="Agrupar por"
              value={agrupamento}
              onChange={(e) => setAgrupamento(e.target.value as Agrupamento)}
            >
              {agrupamentos.map((a) => (
                <option key={a.chave} value={a.chave}>
                  {a.rotulo}
                </option>
              ))}
            </select>
            <label className="flex cursor-pointer items-center gap-1.5 text-suave">
              <input type="checkbox" className="size-4 accent-realce" checked={mostrarEncerradas} onChange={(e) => setMostrarEncerradas(e.target.checked)} />
              Mostrar encerradas
            </label>
            <div className="ml-auto flex" role="group" aria-label="Visualizar">
              <Botao
                variante={exibicao === 'icones' ? 'padrao' : 'discreto'}
                icone={LayoutGrid}
                aria-label="Ícones grandes"
                aria-pressed={exibicao === 'icones'}
                title="Ícones grandes"
                onClick={() => setExibicao('icones')}
              />
              <Botao
                variante={exibicao === 'lista' ? 'padrao' : 'discreto'}
                icone={List}
                aria-label="Lista"
                aria-pressed={exibicao === 'lista'}
                title="Lista"
                onClick={() => setExibicao('lista')}
              />
            </div>
          </>
        )
      }
      rodape={
        haContas && (
          <>
            <span>
              {gerenciador?.contas.length} {gerenciador?.contas.length === 1 ? 'conta' : 'contas'}
            </span>
            <span className="font-semibold">Saldos:</span>
            {gerenciador?.totais.map((total) => (
              <ValorSaldo key={total.moeda.codigo} valor={total.total} moeda={total.moeda} />
            ))}
          </>
        )
      }
    >
      {gerenciador && !haContas && (
        <div className="mx-auto mt-20 grid max-w-sm justify-items-center gap-2 text-center">
          <span className="grid size-14 place-items-center rounded-2xl bg-realce-fundo text-realce">
            <Wallet className="size-7" aria-hidden />
          </span>
          <h2 className="mt-2 text-base font-semibold">Nenhuma conta cadastrada</h2>
          <p className="text-suave">Comece pela conta que você mais usa. Depois acrescente cartões, investimentos e bens.</p>
          <Botao variante="primario" icone={Plus} className="mt-3" onClick={novaConta}>
            Criar conta
          </Botao>
        </div>
      )}

      {haContas && visiveis.length === 0 && (
        <p className="mt-10 text-center text-suave">
          {termo ? 'Nenhuma conta corresponde à busca.' : 'Todas as contas estão encerradas. Marque "Mostrar encerradas" para vê-las.'}
        </p>
      )}

      {agrupar(visiveis, agrupamento).map((grupo) => (
        <Secao key={grupo.titulo} titulo={grupo.titulo} detalhe={`${grupo.contas.length} ${grupo.contas.length === 1 ? 'conta' : 'contas'}`}>
          <ul className={exibicao === 'icones' ? 'grid grid-cols-[repeat(auto-fill,minmax(150px,1fr))] gap-1 p-2' : undefined}>
            {grupo.contas.map((conta) => {
              const acoes = acoesDaConta(conta, () => setFormulario({ conta }), recarregar);
              const abrir = () => abrirConta(conta.id);
              const saldo = <ValorSaldo valor={conta.saldo} moeda={conta.moeda} />;
              const maisAcoes = (
                <MenuBotao acoes={acoes} gatilho={<Botao variante="discreto" icone={MoreHorizontal} aria-label={`Mais ações de ${conta.nome}`} />} />
              );
              return (
                <MenuContexto key={conta.id} acoes={acoes}>
                  {exibicao === 'icones' ? (
                    <li
                      tabIndex={0}
                      title="Duplo clique abre a conta"
                      onDoubleClick={abrir}
                      onKeyDown={(e) => e.key === 'Enter' && e.target === e.currentTarget && abrir()}
                      className={classes(
                        'group relative grid cursor-default justify-items-center gap-1 rounded-lg border border-transparent px-2 pt-3 pb-2 text-center hover:border-borda hover:bg-fundo',
                        conta.encerrada && 'opacity-55',
                      )}
                    >
                      <SeloTipo tipo={conta.tipo} tamanho="enorme" />
                      {conta.favorita && <Star className="absolute top-2 left-2 size-4 fill-amber-400 text-amber-400" aria-label="Favorita" />}
                      <span className="absolute top-1 right-1 opacity-0 group-focus-within:opacity-100 group-hover:opacity-100">{maisAcoes}</span>
                      <span className="line-clamp-2 leading-tight font-medium">{conta.nome}</span>
                      {saldo}
                      {conta.encerrada && <span className="text-[11px] text-suave">Encerrada</span>}
                    </li>
                  ) : (
                    <li
                      tabIndex={0}
                      onDoubleClick={abrir}
                      onKeyDown={(e) => e.key === 'Enter' && e.target === e.currentTarget && abrir()}
                      className={classes(
                        'flex cursor-default items-center gap-3 border-b border-borda px-3 py-1.5 last:border-b-0 hover:bg-fundo',
                        conta.encerrada && 'opacity-55',
                      )}
                    >
                      <SeloTipo tipo={conta.tipo} />
                      <div className="min-w-0 flex-1">
                        <p className="truncate font-medium">{conta.nome}</p>
                        <p className="truncate text-[11.5px] text-suave">
                          {conta.nomeInstituicao ?? 'Sem instituição'} · {conta.nomeTipo}
                          {conta.encerrada && ' · Encerrada'}
                        </p>
                      </div>
                      {conta.favorita && <Star className="size-4 fill-amber-400 text-amber-400" aria-label="Favorita" />}
                      {conta.moeda.codigo !== 'BRL' && (
                        <span className="rounded border border-borda px-1.5 text-[10px] font-semibold text-suave">{conta.moeda.codigo}</span>
                      )}
                      <span className="w-36 text-right">{saldo}</span>
                      {maisAcoes}
                    </li>
                  )}
                </MenuContexto>
              );
            })}
          </ul>
        </Secao>
      ))}

      {formulario?.conta && (
        <ContaDialogo
          conta={formulario.conta}
          aoFechar={() => setFormulario(null)}
          aoSalvar={(salva) => {
            toast.success(`"${salva.nome}" foi alterada.`);
            setFormulario(null);
            void recarregar();
          }}
        />
      )}
      {formulario && !formulario.conta && (
        <NovaContaAssistente
          aoFechar={() => setFormulario(null)}
          aoSalvar={(salva) => {
            toast.success(`"${salva.nome}" foi criada.`);
            setFormulario(null);
            void recarregar();
          }}
        />
      )}
      {cadastros && <CadastrosDialogo tipo={cadastros} aoFechar={() => setCadastros(null)} aoAlterar={() => void recarregar()} />}
    </Pagina>
  );
}
