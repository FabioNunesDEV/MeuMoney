import { Archive, ArchiveRestore, Pencil, Star, StarOff } from 'lucide-react';
import { toast } from 'sonner';
import { api, type Conta, ErroRegra } from '../api';
import type { AcaoMenu } from '../comum/ui';

/** Executa uma ação na API, avisa o resultado e recarrega a tela. Regra violada vira aviso de erro. */
export async function executarComAviso(operacao: () => Promise<void>, confirmacao: string, recarregar: () => Promise<void>) {
  try {
    await operacao();
    toast.success(confirmacao);
    await recarregar();
  } catch (falha) {
    if (!(falha instanceof ErroRegra)) {
      throw falha;
    }

    toast.error(falha.message);
  }
}

/** Ações de uma conta, as mesmas no menu de contexto, no botão "Mais ações" e na página da conta. */
export function acoesDaConta(conta: Conta, editar: () => void, recarregar: () => Promise<void>): AcaoMenu[] {
  const executar = (operacao: () => Promise<void>, confirmacao: string) => executarComAviso(operacao, confirmacao, recarregar);
  return [
    { rotulo: 'Editar', icone: Pencil, aoEscolher: editar },
    conta.favorita
      ? {
          rotulo: 'Remover das favoritas',
          icone: StarOff,
          aoEscolher: () => executar(() => api.definirFavorita(conta.id, false), `"${conta.nome}" deixou de ser favorita.`),
        }
      : {
          rotulo: 'Marcar como favorita',
          icone: Star,
          aoEscolher: () => executar(() => api.definirFavorita(conta.id, true), `"${conta.nome}" marcada como favorita.`),
        },
    conta.encerrada
      ? {
          rotulo: 'Reabrir',
          icone: ArchiveRestore,
          separar: true,
          aoEscolher: () => executar(() => api.reabrirConta(conta.id), `"${conta.nome}" foi reaberta.`),
        }
      : {
          rotulo: 'Encerrar',
          icone: Archive,
          separar: true,
          aoEscolher: () => executar(() => api.encerrarConta(conta.id), `"${conta.nome}" foi encerrada.`),
        },
  ];
}
