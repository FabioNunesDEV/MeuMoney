import type { Moeda } from '../api';

/** Formata o valor na moeda, no padrão numérico pt-BR, com o sinal antes do símbolo. */
export function dinheiro(valor: number, moeda: Moeda): string {
  const numero = Math.abs(valor).toLocaleString('pt-BR', {
    minimumFractionDigits: moeda.casasDecimais,
    maximumFractionDigits: moeda.casasDecimais,
  });
  return `${valor < 0 ? '−' : ''}${moeda.simbolo} ${numero}`;
}

/**
 * Saldo no formato contábil: negativo entre parênteses e sem sinal de menos ("(R$ 30,00)"); positivo
 * e zero como em `dinheiro`.
 */
export function dinheiroContabil(valor: number, moeda: Moeda): string {
  const texto = dinheiro(Math.abs(valor), moeda);
  return valor < 0 ? `(${texto})` : texto;
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

/** Data no formato da API (yyyy-MM-dd) mostrada como dd/MM/yyyy. */
export function dataParaTexto(iso: string): string {
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}

/** Data de hoje no formato da API (yyyy-MM-dd), no fuso da máquina. */
export function hojeIso(): string {
  const hoje = new Date();
  const doisDigitos = (n: number) => String(n).padStart(2, '0');
  return `${hoje.getFullYear()}-${doisDigitos(hoje.getMonth() + 1)}-${doisDigitos(hoje.getDate())}`;
}

/** Data de hoje por extenso, em pt-BR. */
export function hojePorExtenso(): string {
  return new Date().toLocaleDateString('pt-BR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
}

/** Junta classes CSS, ignorando valores vazios. */
export function classes(...partes: (string | false | null | undefined)[]): string {
  return partes.filter(Boolean).join(' ');
}
