import { useEffect, useRef, useState } from 'react';
import { api, type Conta, ErroRegra, type ModoContaCaixa, type SalvarConta, type TipoConta } from '../api';
import { lerValor, valorParaTexto } from '../comum/formato';
import { useDados } from '../comum/useDados';

/** Estado e gravação do formulário de conta, compartilhados pelo assistente de criação e pela edição. */
export type FormularioConta = ReturnType<typeof useFormularioConta>;

/** Estado do formulário de conta. `original` nulo prepara uma conta nova. */
export function useFormularioConta(original: Conta | null) {
  const [opcoes] = useDados(api.opcoes, []);
  const [instituicoes] = useDados(() => api.cadastros('instituicoes'), []);
  const [grupos] = useDados(() => api.cadastros('grupos'), []);

  const [tipo, setTipo] = useState<TipoConta>(original?.tipo ?? 'Corrente');
  const [nome, setNome] = useState(original?.nome ?? '');
  // Como no Money, a conta nova começa marcada como mantida numa instituição.
  const [mantida, setMantida] = useState(original === null || original.nomeInstituicao !== null);
  const [instituicao, setInstituicao] = useState(original?.nomeInstituicao ?? '');
  const [grupo, setGrupo] = useState(original?.nomeGrupo ?? '');
  const [moeda, setMoeda] = useState(original?.moeda.codigo ?? 'BRL');
  const [saldo, setSaldo] = useState(original ? valorParaTexto(original.saldoInformado, original.moeda.casasDecimais) : '');
  const [favorita, setFavorita] = useState(original?.favorita ?? false);
  const [modoCaixa, setModoCaixa] = useState<ModoContaCaixa>(original?.contaCaixaId ? 'Existente' : 'Nenhuma');
  const [caixaId, setCaixaId] = useState(original?.contaCaixaId ?? '');
  const [nomeCaixa, setNomeCaixa] = useState('');
  const [saldoCaixa, setSaldoCaixa] = useState('');
  const [numeroConta, setNumeroConta] = useState(original?.numeroConta ?? '');
  const [abreviacao, setAbreviacao] = useState(original?.abreviacao ?? '');
  const [comentario, setComentario] = useState(original?.comentario ?? '');
  const [saldoMinimo, setSaldoMinimo] = useState(
    original?.saldoMinimo != null ? valorParaTexto(original.saldoMinimo, original.moeda.casasDecimais) : '',
  );
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [caixas] = useDados(() => api.caixas(moeda), [moeda]);

  // O nome acompanha "instituição + tipo" até o usuário digitar um nome próprio.
  const nomeDigitado = useRef(original !== null);
  const infoTipo = opcoes?.tipos.find((t) => t.tipo === tipo);
  const nomeInstituicao = mantida ? instituicao.trim() : '';

  // Tudo o que o usuário pode mudar, como digitado: comparado com o primeiro valor, diz se há alteração.
  const digitado = JSON.stringify([
    nome,
    mantida,
    instituicao,
    grupo,
    moeda,
    saldo,
    favorita,
    modoCaixa,
    caixaId,
    nomeCaixa,
    saldoCaixa,
    numeroConta,
    abreviacao,
    comentario,
    saldoMinimo,
  ]);
  const [digitadoInicial] = useState(digitado);

  useEffect(() => {
    if (!nomeDigitado.current && nomeInstituicao && infoTipo) {
      setNome(`${nomeInstituicao} ${infoTipo.nome}`);
    }
  }, [nomeInstituicao, infoTipo]);

  function alterarNome(valor: string) {
    nomeDigitado.current = valor.trim() !== '';
    setNome(valor);
  }

  /** Grava a conta. Devolve a conta salva, ou null quando o dado é recusado (a mensagem fica em `erro`). */
  async function salvar(): Promise<Conta | null> {
    const ehInvestimento = tipo === 'Investimento';
    const valorSaldo = lerValor(saldo);
    const valorCaixa = lerValor(saldoCaixa);
    // Saldo mínimo vazio é "sem saldo mínimo", não zero; em tipo sem saldo mínimo, nunca vai.
    const textoMinimo = infoTipo?.temSaldoMinimo ? saldoMinimo.trim() : '';
    const valorMinimo = textoMinimo === '' ? null : lerValor(textoMinimo);
    // Na conta nova, o número só vai se o tipo o pergunta: o digitado antes de trocar o tipo é descartado.
    const numero = original || infoTipo?.pedeNumeroNaCriacao ? numeroConta : '';
    if (valorSaldo === null || valorCaixa === null || (textoMinimo !== '' && valorMinimo === null)) {
      setErro('Valor inválido. Use o formato 1.234,56.');
      return null;
    }

    const dados: SalvarConta = {
      tipo,
      nome,
      codigoMoeda: moeda,
      saldoInformado: ehInvestimento ? 0 : valorSaldo,
      nomeInstituicao: nomeInstituicao || null,
      nomeGrupo: grupo || null,
      favorita,
      modoCaixa: ehInvestimento ? modoCaixa : 'Nenhuma',
      contaCaixaId: caixaId || null,
      nomeCaixaNova: nomeCaixa || null,
      saldoCaixaNova: valorCaixa,
      numeroConta: numero || null,
      abreviacao: abreviacao || null,
      comentario: comentario || null,
      saldoMinimo: valorMinimo,
    };

    setSalvando(true);
    setErro(null);
    try {
      return original ? await api.atualizarConta(original.id, dados) : await api.criarConta(dados);
    } catch (falha) {
      if (!(falha instanceof ErroRegra)) {
        throw falha;
      }

      setErro(falha.message);
      return null;
    } finally {
      setSalvando(false);
    }
  }

  return {
    original,
    opcoes,
    instituicoes,
    grupos,
    caixasCompativeis: caixas?.filter((c) => c.id !== original?.id) ?? [],
    infoTipo,
    tipo,
    setTipo,
    nome,
    alterarNome,
    mantida,
    setMantida,
    instituicao,
    setInstituicao,
    grupo,
    setGrupo,
    moeda,
    setMoeda,
    saldo,
    setSaldo,
    favorita,
    setFavorita,
    modoCaixa,
    setModoCaixa,
    caixaId,
    setCaixaId,
    nomeCaixa,
    setNomeCaixa,
    saldoCaixa,
    setSaldoCaixa,
    numeroConta,
    setNumeroConta,
    abreviacao,
    setAbreviacao,
    comentario,
    setComentario,
    saldoMinimo,
    setSaldoMinimo,
    alterado: digitado !== digitadoInicial,
    erro,
    setErro,
    salvando,
    salvar,
  };
}
