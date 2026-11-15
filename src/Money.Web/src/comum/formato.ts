import type { Moeda } from '../api';

/** Formata o valor na moeda, no padrão numérico pt-BR, com o sinal antes do símbolo. */
export function dinheiro(valor: number, moeda: Moeda): string {
  const numero = Math.abs(valor).toLocaleString('pt-BR', {
    minimumFractionDigits: moeda.casasDecimais,
    maximumFractionDigits: moeda.casasDecimais,
  });
  return `${valor < 0 ? '−' : ''}${moeda.simbolo} ${numero}`;
}

/** Valor para um campo de texto, no padrão pt-BR ("1.234,56"). */
export function valorParaTexto(valor: number, casas: number): string {
  return valor.toLocaleString('pt-BR', { minimumFractionDigits: casas, maximumFractionDigits: casas });
}

/** Lê um valor digitado no padrão pt-BR. Vazio vale zero; texto inválido devolve null. */
export function lerValor(texto: string): number | null {
  const limpo = texto.trim();
  if (limpo === '') {
    return 0;
  }

  if (!/^-?\d{1,3}(\.\d{3})*(,\d+)?$|^-?\d+(,\d+)?$/.test(limpo)) {
    return null;
  }

  return Number(limpo.replaceAll('.', '').replace(',', '.'));
}

/** Data de hoje por extenso, em pt-BR. */
export function hojePorExtenso(): string {
  return new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
}

/** Junta classes CSS, ignorando valores vazios. */
export function classes(...partes: (string | false | null | undefined)[]): string {
  return partes.filter(Boolean).join(' ');
}
