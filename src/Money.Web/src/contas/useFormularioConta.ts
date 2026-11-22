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
  const [erro, setErro] = useState<string | null>(null);
  const [salvando, setSalvando] = useState(false);
  const [caixas] = useDados(() => api.caixas(moeda), [moeda]);

  // O nome acompanha "instituição + tipo" até o usuário digitar um nome próprio.
  const nomeDigitado = useRef(original !== null);
  const infoTipo = opcoes?.tipos.find((t) => t.tipo === tipo);
  const nomeInstituicao = mantida ? instituicao.trim() : '';

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
    if (valorSaldo === null || valorCaixa === null) {
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
    erro,
    setErro,
    salvando,
    salvar,
  };
}
