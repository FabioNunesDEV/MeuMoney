import { useState } from 'react';
import { toast } from 'sonner';
import {
  api,
  type Conta,
  ErroRegra,
  type Lancamento,
  type NaturezaLancamento,
  type OpcoesLancamento,
  type SaldoNegativo,
  type SalvarLancamento,
} from '../api';
import { hojeIso, lerValor, valorParaTexto } from '../comum/formato';

/**
 * Modo do formulário, como no Money: `vazio` (campos ocultos), `leitura` (lançamento escolhido na
 * grade), `novo` (inclusão) e `edicao` (alteração).
 */
export type ModoFormulario = 'vazio' | 'leitura' | 'novo' | 'edicao';

/** Estado e comandos do formulário de lançamento da Movimentação. */
export type FormularioLancamento = ReturnType<typeof useFormularioLancamento>;

const favorecidoDaTransferencia = 'Transferência';

/** A aba em que um lançamento aparece. */
export function naturezaDe(lancamento: Lancamento): NaturezaLancamento {
  if (lancamento.tipo === 'Gasto' || lancamento.tipo === 'Recebimento') {
    return lancamento.tipo;
  }

  return 'Transferencia';
}

/**
 * Estado do formulário de lançamento: aba, modo, campos, erro e a confirmação de saldo negativo.
 * `aoGravar` recarrega a grade e o saldo depois de gravar.
 */
export function useFormularioLancamento(conta: Conta, opcoes: OpcoesLancamento | undefined, aoGravar: () => Promise<void>) {
  const [modo, setModo] = useState<ModoFormulario>('vazio');
  const [aba, setAba] = useState<NaturezaLancamento>('Gasto');
  const [escolhido, setEscolhido] = useState<Lancamento | null>(null);
  const [favorecido, setFavorecido] = useState('');
  const [categoria, setCategoria] = useState('');
  const [subcategoria, setSubcategoria] = useState('');
  const [memo, setMemo] = useState('');
  const [data, setData] = useState(hojeIso());
  const [montante, setMontante] = useState('');
  const [de, setDe] = useState(conta.id);
  const [para, setPara] = useState('');
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [saldosNegativos, setSaldosNegativos] = useState<SaldoNegativo[] | null>(null);

  const editavel = modo === 'novo' || modo === 'edicao';
  const outraConta = opcoes?.contas.find((c) => c.id !== conta.id)?.id ?? '';
  const subcategorias = opcoes?.categorias.find((c) => mesmoNome(c.nome, categoria))?.subcategorias ?? [];

  function preencher(lancamento: Lancamento | null, natureza: NaturezaLancamento) {
    const transferencia = natureza === 'Transferencia';
    const saida = lancamento?.tipo !== 'TransferenciaEntrada';
    setFavorecido(lancamento?.nomeFavorecido ?? (transferencia ? favorecidoDaTransferencia : ''));
    setCategoria(lancamento?.nomeCategoria ?? '');
    setSubcategoria(lancamento?.nomeSubcategoria ?? '');
    setMemo(lancamento?.memo ?? '');
    setData(lancamento?.data ?? hojeIso());
    setMontante(lancamento ? valorParaTexto(lancamento.valor, conta.moeda.casasDecimais) : '');
    // Na transferência, a conta da página fica do lado em que o lançamento está.
    setDe(lancamento && !saida ? (lancamento.outraContaId ?? '') : conta.id);
    setPara(lancamento ? (saida ? (lancamento.outraContaId ?? '') : conta.id) : outraConta);
    setErro(null);
  }

  /** Abre um lançamento novo na aba escolhida. */
  function novo() {
    if (conta.encerrada || editavel) {
      return;
    }

    setEscolhido(null);
    preencher(null, aba);
    setModo('novo');
  }

  /** Mostra o lançamento escolhido na grade, só para leitura, na aba dele. */
  function escolher(lancamento: Lancamento) {
    if (editavel) {
      return;
    }

    const natureza = naturezaDe(lancamento);
    setEscolhido(lancamento);
    setAba(natureza);
    preencher(lancamento, natureza);
    setModo('leitura');
  }

  /** Libera para alteração o lançamento escolhido. */
  function editar(lancamento?: Lancamento) {
    if (conta.encerrada) {
      return;
    }

    const alvo = lancamento ?? escolhido;
    if (!alvo) {
      return;
    }

    if (lancamento) {
      setEscolhido(lancamento);
      setAba(naturezaDe(lancamento));
      preencher(lancamento, naturezaDe(lancamento));
    }

    setModo('edicao');
  }

  /** Abandona a inclusão ou a alteração sem gravar. */
  function cancelar() {
    setModo(escolhido && modo === 'edicao' ? 'leitura' : 'vazio');
    if (escolhido) {
      preencher(escolhido, naturezaDe(escolhido));
    }

    setErro(null);
    setSaldosNegativos(null);
  }

  /** Troca de aba: livre fora da alteração; num lançamento novo, mantém o que já foi digitado. */
  function trocarAba(nova: NaturezaLancamento) {
    if (modo === 'edicao' || nova === aba) {
      return;
    }

    setAba(nova);
    if (modo === 'novo') {
      if (nova === 'Transferencia' && favorecido.trim() === '') {
        setFavorecido(favorecidoDaTransferencia);
      } else if (nova !== 'Transferencia' && favorecido === favorecidoDaTransferencia) {
        setFavorecido('');
      }

      return;
    }

    setEscolhido(null);
    setModo('vazio');
  }

  /** Ao escolher um favorecido já usado num lançamento novo, traz categoria, subcategoria e montante do último uso. */
  function alterarFavorecido(valor: string) {
    setFavorecido(valor);
    const conhecido = opcoes?.favorecidos.find((f) => mesmoNome(f.nome, valor));
    if (modo !== 'novo' || !conhecido || conhecido.ultimoValor === null) {
      return;
    }

    if (aba !== 'Transferencia') {
      setCategoria(conhecido.ultimaCategoria ?? '');
      setSubcategoria(conhecido.ultimaSubcategoria ?? '');
    }

    setMontante(valorParaTexto(conhecido.ultimoValor, conta.moeda.casasDecimais));
  }

  /**
   * Valida o formato e grava. Lançamento novo todo em branco não grava nada e volta ao início. Se uma
   * conta for ficar negativa, guarda a lista em `saldosNegativos` para o pop-up de confirmação.
   */
  async function inserir(confirmarSaldoNegativo = false) {
    if (!editavel || salvando) {
      return;
    }

    const emBranco = [favorecido === favorecidoDaTransferencia ? '' : favorecido, categoria, subcategoria, memo, montante].every(
      (campo) => campo.trim() === '',
    );
    if (modo === 'novo' && emBranco) {
      cancelar();
      return;
    }

    const valor = lerValor(montante);
    if (valor === null || montante.trim() === '') {
      setErro('Informe o montante no formato 1.234,56.');
      return;
    }

    if (!/^\d{4}-\d{2}-\d{2}$/.test(data)) {
      setErro('Informe uma data válida.');
      return;
    }

    const transferencia = aba === 'Transferencia';
    const dados: SalvarLancamento = {
      natureza: aba,
      contaId: transferencia ? de : conta.id,
      contaDestinoId: transferencia ? para || null : null,
      data,
      valor,
      nomeFavorecido: favorecido.trim() || null,
      nomeCategoria: transferencia ? null : categoria.trim() || null,
      nomeSubcategoria: transferencia ? null : subcategoria.trim() || null,
      memo: memo.trim() || null,
      confirmarSaldoNegativo,
    };

    setSalvando(true);
    try {
      const resultado =
        modo === 'edicao' && escolhido ? await api.alterarLancamento(escolhido.id, dados) : await api.criarLancamento(dados);
      if (!resultado.gravado) {
        setSaldosNegativos(resultado.saldosNegativos);
        return;
      }

      toast.success(modo === 'edicao' ? 'Lançamento alterado.' : 'Lançamento inserido.');
      setSaldosNegativos(null);
      setEscolhido(null);
      setModo('vazio');
      await aoGravar();
    } catch (falha) {
      setSaldosNegativos(null);
      if (!(falha instanceof ErroRegra)) {
        throw falha;
      }

      setErro(falha.message);
    } finally {
      setSalvando(false);
    }
  }

  /** Esquece o lançamento escolhido (ele foi excluído ou movido). */
  function esquecer() {
    if (!editavel) {
      setEscolhido(null);
      setModo('vazio');
    }
  }

  return {
    conta,
    opcoes,
    modo,
    aba,
    escolhido,
    editavel,
    favorecido,
    alterarFavorecido,
    categoria,
    setCategoria,
    subcategoria,
    setSubcategoria,
    subcategorias,
    memo,
    setMemo,
    data,
    setData,
    montante,
    setMontante,
    de,
    setDe,
    para,
    setPara,
    erro,
    salvando,
    saldosNegativos,
    novo,
    escolher,
    editar,
    cancelar,
    trocarAba,
    inserir,
    esquecer,
    descartarConfirmacao: () => setSaldosNegativos(null),
  };
}

function mesmoNome(a: string, b: string): boolean {
  return a.trim().localeCompare(b.trim(), 'pt-BR', { sensitivity: 'accent' }) === 0;
}
