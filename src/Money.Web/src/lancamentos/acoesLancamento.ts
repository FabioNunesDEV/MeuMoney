import { ArrowRightLeft, CheckCheck, FolderInput, Pencil, Trash2 } from 'lucide-react';
import { api, type Lancamento, type SituacaoLancamento } from '../api';
import type { AcaoMenu } from '../comum/ui';
import { executarComAviso } from '../contas/acoes';

/** O que as ações do lançamento precisam da tela da Movimentação. */
export interface ContextoAcoes {
  encerrada: boolean;
  editar: (lancamento: Lancamento) => void;
  irParaConta: (contaId: string) => void;
  pedirMover: (lancamento: Lancamento) => void;
  pedirExcluir: (lancamento: Lancamento) => void;
  recarregar: () => Promise<void>;
}

const situacoes: { situacao: SituacaoLancamento; rotulo: string }[] = [
  { situacao: 'NaoReconciliado', rotulo: 'Não reconciliado' },
  { situacao: 'Compensado', rotulo: 'Compensado (C)' },
  { situacao: 'Reconciliado', rotulo: 'Reconciliado (R)' },
  { situacao: 'Nulo', rotulo: 'Nulo' },
];

/** Ações de um lançamento, as mesmas no menu de contexto e no botão "Mais ações" da linha. */
export function acoesLancamento(lancamento: Lancamento, contexto: ContextoAcoes): AcaoMenu[] {
  const desativado = contexto.encerrada;
  const acoes: AcaoMenu[] = [{ rotulo: 'Editar', icone: Pencil, desativado, aoEscolher: () => contexto.editar(lancamento) }];

  if (lancamento.outraContaId) {
    const outraContaId = lancamento.outraContaId;
    acoes.push({
      rotulo: `Ir para conta: ${lancamento.nomeOutraConta ?? ''}`,
      icone: ArrowRightLeft,
      aoEscolher: () => contexto.irParaConta(outraContaId),
    });
  }

  acoes.push(
    {
      rotulo: 'Marcar como',
      icone: CheckCheck,
      separar: true,
      desativado,
      submenu: situacoes.map((s) => ({
        rotulo: s.rotulo,
        marcado: lancamento.situacao === s.situacao,
        aoEscolher: () =>
          executarComAviso(
            () => api.marcarLancamento(lancamento.id, s.situacao),
            `Lançamento marcado como ${s.rotulo.toLowerCase()}.`,
            contexto.recarregar,
          ),
      })),
    },
    { rotulo: 'Mover para conta…', icone: FolderInput, desativado, aoEscolher: () => contexto.pedirMover(lancamento) },
    { rotulo: 'Excluir', icone: Trash2, perigo: true, separar: true, desativado, aoEscolher: () => contexto.pedirExcluir(lancamento) },
  );
  return acoes;
}
