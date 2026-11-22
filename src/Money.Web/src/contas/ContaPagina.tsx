import { ArrowLeft, ClipboardList, History, ListOrdered, Wallet } from 'lucide-react';
import { lazy, Suspense, useState } from 'react';
import { api, type Conta } from '../api';
import { type AcaoMenu, BlocoPainel, Botao, MenuBotao, Pagina, Tarefa, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';
import { Movimentacao } from '../lancamentos/Movimentacao';
import { acoesDaConta } from './acoes';
import { DetalhesConta } from './DetalhesConta';

type Visao = 'movimentacao' | 'detalhes' | 'historico';

// O Histórico usa o recharts: carregado só quando aberto, para não pesar na abertura do aplicativo.
const Historico = lazy(() => import('../lancamentos/Historico').then((m) => ({ default: m.Historico })));

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
  const [escolhida, setVisao] = useState<Visao | null>(null);
  // O Editar pede o foco no Nome dos Detalhes; cada Desfazer remonta os Detalhes com os valores gravados.
  const [focarNome, setFocarNome] = useState(false);
  const [desfeitos, setDesfeitos] = useState(0);

  if (!conta) {
    return <Pagina titulo="Conta" painel={null} children={null} />;
  }

  const temMovimentacao = conta.tipo === 'Dinheiro';
  const visao: Visao = escolhida ?? (temMovimentacao ? 'movimentacao' : 'detalhes');
  const acoes = acoesDaConta(
    conta,
    () => {
      setVisao('detalhes');
      setFocarNome(true);
    },
    recarregar,
  );
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
        <DetalhesConta
          // A conta recarregada (depois de salvar, favoritar ou encerrar) também remonta o formulário.
          key={`${JSON.stringify(conta)}-${desfeitos}`}
          conta={conta}
          focarNome={focarNome}
          aoFocar={() => setFocarNome(false)}
          aoSalvar={() => void recarregar()}
          aoDesfazer={() => setDesfeitos((n) => n + 1)}
        />
      )}
    </Pagina>
  );
}
