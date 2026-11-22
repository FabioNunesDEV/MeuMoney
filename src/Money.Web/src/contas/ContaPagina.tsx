import { ArrowLeft, ClipboardList, History, ListOrdered, Wallet } from 'lucide-react';
import { lazy, type ReactNode, Suspense, useState } from 'react';
import { toast } from 'sonner';
import { api, type Conta } from '../api';
import { dinheiro } from '../comum/formato';
import { SeloTipo } from '../comum/tiposConta';
import { type AcaoMenu, BlocoPainel, Botao, MenuBotao, Pagina, Secao, Tarefa, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';
import { Movimentacao } from '../lancamentos/Movimentacao';
import { acoesDaConta } from './acoes';
import { ContaDialogo } from './ContaDialogo';

type Visao = 'movimentacao' | 'detalhes' | 'historico';

// O Histórico usa o recharts: carregado só quando aberto, para não pesar na abertura do aplicativo.
const Historico = lazy(() => import('../lancamentos/Historico').then((m) => ({ default: m.Historico })));

function Dado({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[150px_1fr] gap-3 border-b border-borda px-3 py-1.5 last:border-b-0">
      <dt className="text-suave">{rotulo}</dt>
      <dd className="font-medium">{children}</dd>
    </div>
  );
}

// Menu "Contas", como no Money: o gerenciador no topo, as contas abertas e as encerradas num submenu.
function acoesMenuContas(contas: Conta[], atual: string, voltar: () => void, abrirConta: (id: string) => void): AcaoMenu[] {
  const item = (c: Conta): AcaoMenu => ({ rotulo: c.nome, marcado: c.id === atual, aoEscolher: () => abrirConta(c.id) });
  const abertas = contas.filter((c) => !c.encerrada).map(item);
  const encerradas = contas.filter((c) => c.encerrada).map(item);
  const acoes: AcaoMenu[] = [{ rotulo: 'Gerenciador de contas', icone: Wallet, aoEscolher: voltar }];
  if (abertas.length > 0) {
    abertas[0] = { ...abertas[0], separar: true };
  }

  acoes.push(...abertas);
  if (encerradas.length > 0) {
    acoes.push({ rotulo: 'Contas encerradas', separar: true, submenu: encerradas });
  }

  return acoes;
}

/** Página de uma conta, como no Money: Movimentação (só na conta Dinheiro, por ora), Detalhes e Histórico. */
export function ContaPagina({ id, voltar, abrirConta }: { id: string; voltar: () => void; abrirConta: (id: string) => void }) {
  const [conta, recarregar] = useDados(() => api.conta(id), [id]);
  const [todas] = useDados(() => api.gerenciador(true), []);
  const [editando, setEditando] = useState(false);
  const [escolhida, setVisao] = useState<Visao | null>(null);

  if (!conta) {
    return <Pagina titulo="Conta" painel={null} children={null} />;
  }

  const temMovimentacao = conta.tipo === 'Dinheiro';
  const visao: Visao = escolhida ?? (temMovimentacao ? 'movimentacao' : 'detalhes');
  const acoes = acoesDaConta(conta, () => setEditando(true), recarregar);
  const menuContas = acoesMenuContas(todas?.contas ?? [], conta.id, voltar, abrirConta);

  return (
    <Pagina
      titulo={conta.nome}
      ferramentas={
        <>
          {acoes.map((acao) => (
            <Botao key={acao.rotulo} variante={acao.rotulo === 'Editar' ? 'destaque' : 'barra'} icone={acao.icone} onClick={acao.aoEscolher}>
              {acao.rotulo}
            </Botao>
          ))}
        </>
      }
      painel={
        <>
          <BlocoPainel titulo="Conta">
            <Tarefa icone={ListOrdered} embreve={!temMovimentacao} ativo={visao === 'movimentacao'} aoClicar={() => setVisao('movimentacao')}>
              Movimentação
            </Tarefa>
            <Tarefa icone={ClipboardList} ativo={visao === 'detalhes'} aoClicar={() => setVisao('detalhes')}>
              Detalhes
            </Tarefa>
            <Tarefa icone={History} embreve={!temMovimentacao} ativo={visao === 'historico'} aoClicar={() => setVisao('historico')}>
              Histórico
            </Tarefa>
          </BlocoPainel>
          <BlocoPainel titulo="Ir para">
            <MenuBotao acoes={menuContas} gatilho={<Tarefa icone={Wallet}>Contas</Tarefa>} />
            <Tarefa icone={ArrowLeft} aoClicar={voltar}>
              Gerenciador de contas
            </Tarefa>
          </BlocoPainel>
        </>
      }
      opcoesSoEstreita
      opcoes={
        <>
          <div className="flex" role="group" aria-label="Visão">
            <Botao
              variante={visao === 'movimentacao' ? 'primario' : 'padrao'}
              icone={ListOrdered}
              disabled={!temMovimentacao}
              title={temMovimentacao ? undefined : 'Em breve'}
              className="rounded-r-none"
              onClick={() => setVisao('movimentacao')}
            >
              Movimentação
            </Botao>
            <Botao
              variante={visao === 'detalhes' ? 'primario' : 'padrao'}
              icone={ClipboardList}
              className="-ml-px rounded-none"
              onClick={() => setVisao('detalhes')}
            >
              Detalhes
            </Botao>
            <Botao
              variante={visao === 'historico' ? 'primario' : 'padrao'}
              icone={History}
              disabled={!temMovimentacao}
              title={temMovimentacao ? undefined : 'Em breve'}
              className="-ml-px rounded-l-none"
              onClick={() => setVisao('historico')}
            >
              Histórico
            </Botao>
          </div>
          <MenuBotao acoes={menuContas} gatilho={<Botao icone={Wallet}>Contas</Botao>} />
        </>
      }
      rodape={
        <>
          <span className="font-semibold">{visao === 'movimentacao' ? 'Saldo final:' : 'Saldo:'}</span>
          <ValorSaldo valor={conta.saldo} moeda={conta.moeda} />
        </>
      }
    >
      {visao === 'movimentacao' && <Movimentacao conta={conta} aoMudar={recarregar} abrirConta={abrirConta} />}
      {visao === 'historico' && (
        <Suspense fallback={<p className="text-suave">Carregando…</p>}>
          <Historico conta={conta} irParaMovimentacao={() => setVisao('movimentacao')} />
        </Suspense>
      )}
      {visao === 'detalhes' && (
        <>
          <div className="mb-4 flex items-center gap-3">
            <SeloTipo tipo={conta.tipo} tamanho="enorme" />
            <div>
              <p className="text-suave">
                {conta.nomeTipo}
                {conta.encerrada && ' · Encerrada'}
              </p>
              <ValorSaldo valor={conta.saldo} moeda={conta.moeda} className="text-2xl" />
            </div>
          </div>

          <Secao titulo="Detalhes da conta">
            <dl>
              <Dado rotulo="Nome">{conta.nome}</Dado>
              <Dado rotulo="Tipo">{conta.nomeTipo}</Dado>
              <Dado rotulo="Instituição">{conta.nomeInstituicao ?? 'Não é mantida em instituição'}</Dado>
              <Dado rotulo="Moeda">
                {conta.moeda.nome} ({conta.moeda.codigo})
              </Dado>
              {conta.tipo !== 'Investimento' && <Dado rotulo="Saldo inicial">{dinheiro(conta.saldoInformado, conta.moeda)}</Dado>}
              <Dado rotulo="Saldo">
                <ValorSaldo valor={conta.saldo} moeda={conta.moeda} />
              </Dado>
              {conta.tipo === 'Investimento' && <Dado rotulo="Conta caixa">{conta.nomeContaCaixa ?? 'Sem conta caixa'}</Dado>}
              <Dado rotulo="Grupo">{conta.nomeGrupo ?? 'Sem grupo'}</Dado>
              <Dado rotulo="Favorita">{conta.favorita ? 'Sim' : 'Não'}</Dado>
              <Dado rotulo="Situação">{conta.encerrada ? 'Encerrada' : 'Aberta'}</Dado>
            </dl>
          </Secao>
        </>
      )}

      {editando && (
        <ContaDialogo
          conta={conta}
          aoFechar={() => setEditando(false)}
          aoSalvar={(salva) => {
            toast.success(`"${salva.nome}" foi alterada.`);
            setEditando(false);
            void recarregar();
          }}
        />
      )}
    </Pagina>
  );
}
