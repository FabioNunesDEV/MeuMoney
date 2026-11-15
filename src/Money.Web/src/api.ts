// Tipos e chamadas da API local do MeuMoney. Os nomes espelham os DTOs de Money.Services.

export type TipoConta = 'Corrente' | 'Poupanca' | 'Dinheiro' | 'CartaoCredito' | 'Investimento' | 'Ativo' | 'Passivo';
export type ModoContaCaixa = 'Nenhuma' | 'Existente' | 'Nova';

export interface Moeda {
  codigo: string;
  nome: string;
  simbolo: string;
  casasDecimais: number;
}

export interface TipoContaInfo {
  tipo: TipoConta;
  nome: string;
  descricao: string;
}

export interface Conta {
  id: string;
  nome: string;
  tipo: TipoConta;
  nomeTipo: string;
  moeda: Moeda;
  saldo: number;
  saldoInformado: number;
  instituicaoFinanceiraId: string | null;
  nomeInstituicao: string | null;
  grupoContaId: string | null;
  nomeGrupo: string | null;
  contaCaixaId: string | null;
  nomeContaCaixa: string | null;
  favorita: boolean;
  encerrada: boolean;
}

export interface TotalMoeda {
  moeda: Moeda;
  total: number;
}

export interface Gerenciador {
  contas: Conta[];
  totais: TotalMoeda[];
  existemContas: boolean;
}

export interface OpcoesConta {
  tipos: TipoContaInfo[];
  moedas: Moeda[];
}

export interface Cadastro {
  id: string;
  nome: string;
  quantidadeContas: number;
}

export interface SalvarConta {
  tipo: TipoConta;
  nome: string;
  codigoMoeda: string;
  saldoInformado: number;
  nomeInstituicao: string | null;
  nomeGrupo: string | null;
  favorita: boolean;
  modoCaixa: ModoContaCaixa;
  contaCaixaId: string | null;
  nomeCaixaNova: string | null;
  saldoCaixaNova: number;
}

/** Erro de regra de negócio devolvido pela API; a mensagem vai ao usuário como está. */
export class ErroRegra extends Error {}

async function chamar<T>(metodo: string, caminho: string, corpo?: unknown): Promise<T> {
  const resposta = await fetch(`/api${caminho}`, {
    method: metodo,
    headers: corpo === undefined ? undefined : { 'Content-Type': 'application/json' },
    body: corpo === undefined ? undefined : JSON.stringify(corpo),
  });
  if (resposta.status === 400) {
    const erro = (await resposta.json()) as { mensagem?: string };
    throw new ErroRegra(erro.mensagem ?? 'Não foi possível concluir.');
  }

  if (!resposta.ok) {
    throw new Error(`Falha na chamada ${metodo} ${caminho}: ${resposta.status}`);
  }

  const texto = await resposta.text();
  return (texto ? JSON.parse(texto) : undefined) as T;
}

export type TipoCadastro = 'instituicoes' | 'grupos';

export const api = {
  gerenciador: (encerradas: boolean) => chamar<Gerenciador>('GET', `/contas?encerradas=${encerradas}`),
  conta: (id: string) => chamar<Conta>('GET', `/contas/${id}`),
  opcoes: () => chamar<OpcoesConta>('GET', '/contas/opcoes'),
  caixas: (moeda: string) => chamar<Conta[]>('GET', `/contas/caixas?moeda=${moeda}`),
  criarConta: (dados: SalvarConta) => chamar<Conta>('POST', '/contas', dados),
  atualizarConta: (id: string, dados: SalvarConta) => chamar<Conta>('PUT', `/contas/${id}`, dados),
  definirFavorita: (id: string, favorita: boolean) => chamar<void>('PUT', `/contas/${id}/favorita`, { favorita }),
  encerrarConta: (id: string) => chamar<void>('POST', `/contas/${id}/encerrar`),
  reabrirConta: (id: string) => chamar<void>('POST', `/contas/${id}/reabrir`),
  cadastros: (tipo: TipoCadastro) => chamar<Cadastro[]>('GET', `/${tipo}`),
  renomearCadastro: (tipo: TipoCadastro, id: string, nome: string) => chamar<void>('PUT', `/${tipo}/${id}`, { nome }),
  excluirCadastro: (tipo: TipoCadastro, id: string) => chamar<void>('DELETE', `/${tipo}/${id}`),
};
