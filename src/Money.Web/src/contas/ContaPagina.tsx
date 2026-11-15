import { ArrowLeft, ClipboardList, History, ListOrdered, type LucideIcon } from 'lucide-react';
import { type ReactNode, useState } from 'react';
import { toast } from 'sonner';
import { api } from '../api';
import { classes, dinheiro } from '../comum/formato';
import { SeloTipo } from '../comum/tiposConta';
import { BlocoPainel, Botao, Pagina, Secao, Tarefa } from '../comum/ui';
import { useDados } from '../comum/useDados';
import { acoesDaConta } from './acoes';
import { ContaDialogo } from './ContaDialogo';

function Dado({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <div className="grid grid-cols-[150px_1fr] gap-3 border-b border-borda px-3 py-1.5 last:border-b-0">
      <dt className="text-suave">{rotulo}</dt>
      <dd className="font-medium">{children}</dd>
    </div>
  );
}

function EmBreve({ icone: Icone, texto }: { icone: LucideIcon; texto: string }) {
  return (
    <div className="flex items-center gap-3 px-3 py-4 text-suave">
      <Icone className="size-5 text-fraco" aria-hidden />
      <p>{texto}</p>
    </div>
  );
}

/** Página de uma conta, como no Money: movimentação, detalhes e histórico. Por ora, só os detalhes. */
export function ContaPagina({ id, voltar }: { id: string; voltar: () => void }) {
  const [conta, recarregar] = useDados(() => api.conta(id), [id]);
  const [editando, setEditando] = useState(false);

  if (!conta) {
    return <Pagina titulo="Conta" painel={null} children={null} />;
  }

  const acoes = acoesDaConta(conta, () => setEditando(true), recarregar);

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
            <Tarefa icone={ListOrdered} embreve>
              Movimentação
            </Tarefa>
            <Tarefa icone={ClipboardList} ativo>
              Detalhes
            </Tarefa>
            <Tarefa icone={History} embreve>
              Histórico
            </Tarefa>
          </BlocoPainel>
          <BlocoPainel titulo="Ir para">
            <Tarefa icone={ArrowLeft} aoClicar={voltar}>
              Gerenciador de contas
            </Tarefa>
          </BlocoPainel>
        </>
      }
      rodape={
        <>
          <span className="font-semibold">Saldo:</span>
          <span className={classes('font-semibold text-texto tabular-nums', conta.saldo < 0 && 'text-negativo')}>{dinheiro(conta.saldo, conta.moeda)}</span>
        </>
      }
    >
      <div className="mb-4 flex items-center gap-3">
        <SeloTipo tipo={conta.tipo} tamanho="enorme" />
        <div>
          <p className="text-suave">
            {conta.nomeTipo}
            {conta.encerrada && ' · Encerrada'}
          </p>
          <p className={classes('text-2xl font-semibold tabular-nums', conta.saldo < 0 && 'text-negativo')}>{dinheiro(conta.saldo, conta.moeda)}</p>
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
          {conta.tipo !== 'Investimento' && <Dado rotulo="Saldo inicial">{dinheiro(conta.saldo, conta.moeda)}</Dado>}
          {conta.tipo === 'Investimento' && <Dado rotulo="Conta caixa">{conta.nomeContaCaixa ?? 'Sem conta caixa'}</Dado>}
          <Dado rotulo="Grupo">{conta.nomeGrupo ?? 'Sem grupo'}</Dado>
          <Dado rotulo="Favorita">{conta.favorita ? 'Sim' : 'Não'}</Dado>
          <Dado rotulo="Situação">{conta.encerrada ? 'Encerrada' : 'Aberta'}</Dado>
        </dl>
      </Secao>

      <Secao titulo="Movimentação">
        <EmBreve icone={ListOrdered} texto="Os lançamentos desta conta aparecerão aqui quando o módulo Lançamentos for entregue." />
      </Secao>

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
