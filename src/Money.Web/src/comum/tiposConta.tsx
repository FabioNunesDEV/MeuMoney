import { Building2, CreditCard, Landmark, type LucideIcon, PiggyBank, Receipt, TrendingUp, Wallet } from 'lucide-react';
import type { CSSProperties } from 'react';
import type { TipoConta } from '../api';
import { classes } from './formato';

/** Ícone e cor (variável CSS) de cada tipo de conta. */
export const aparenciaTipo: Record<TipoConta, { icone: LucideIcon; cor: string }> = {
  Corrente: { icone: Landmark, cor: 'var(--tipo-corrente)' },
  Poupanca: { icone: PiggyBank, cor: 'var(--tipo-poupanca)' },
  Dinheiro: { icone: Wallet, cor: 'var(--tipo-dinheiro)' },
  CartaoCredito: { icone: CreditCard, cor: 'var(--tipo-cartao)' },
  Investimento: { icone: TrendingUp, cor: 'var(--tipo-investimento)' },
  Ativo: { icone: Building2, cor: 'var(--tipo-ativo)' },
  Passivo: { icone: Receipt, cor: 'var(--tipo-passivo)' },
};

/** Selo quadrado com o ícone do tipo de conta, na cor do tipo. "enorme" é o ícone grande do gerenciador. */
export function SeloTipo({ tipo, tamanho = 'normal' }: { tipo: TipoConta; tamanho?: 'normal' | 'enorme' }) {
  const { icone: Icone, cor } = aparenciaTipo[tipo];
  const enorme = tamanho === 'enorme';
  return (
    <span
      className={classes('selo-tipo grid shrink-0 place-items-center', enorme ? 'size-12 rounded-xl' : 'size-7 rounded-lg')}
      style={{ '--cor': cor } as CSSProperties}
    >
      <Icone className={enorme ? 'size-6' : 'size-4'} aria-hidden />
    </span>
  );
}
