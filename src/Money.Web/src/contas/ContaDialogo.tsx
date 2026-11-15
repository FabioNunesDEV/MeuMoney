import { useEffect, useId, useRef, useState } from 'react';
import { api, type Conta, ErroRegra, type ModoContaCaixa, type SalvarConta, type TipoConta } from '../api';
import { classes, lerValor, valorParaTexto } from '../comum/formato';
import { SeloTipo } from '../comum/tiposConta';
import { AvisoErro, Botao, Campo, estiloEntrada, Popup } from '../comum/ui';
import { useDados } from '../comum/useDados';

const rotuloSaldo: Partial<Record<TipoConta, string>> = {
  CartaoCredito: 'Saldo devedor',
  Passivo: 'Saldo devedor',
  Ativo: 'Valor do bem',
};

const modosCaixa: { modo: ModoContaCaixa; rotulo: string }[] = [
  { modo: 'Nenhuma', rotulo: 'Sem conta caixa' },
  { modo: 'Existente', rotulo: 'Usar uma conta existente' },
  { modo: 'Nova', rotulo: 'Criar uma conta caixa' },
];

/** Pop-up de criar ou editar conta. `conta` nula cria uma nova. */
export function ContaDialogo(props: { conta: Conta | null; aoFechar: () => void; aoSalvar: (conta: Conta) => void }) {
  const original = props.conta;
  const [opcoes] = useDados(api.opcoes, []);
  const [instituicoes] = useDados(() => api.cadastros('instituicoes'), []);
  const [grupos] = useDados(() => api.cadastros('grupos'), []);

  const [tipo, setTipo] = useState<TipoConta>(original?.tipo ?? 'Corrente');
  const [nome, setNome] = useState(original?.nome ?? '');
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

  // O nome acompanha "instituição + tipo" até o usuário digitar um nome próprio.
  const nomeDigitado = useRef(original !== null);
  const [caixas] = useDados(() => api.caixas(moeda), [moeda]);
  const idInstituicoes = useId();
  const idGrupos = useId();

  const ehInvestimento = tipo === 'Investimento';
  const nomeTipo = opcoes?.tipos.find((t) => t.tipo === tipo);
  const caixasCompativeis = caixas?.filter((c) => c.id !== original?.id) ?? [];

  useEffect(() => {
    if (!nomeDigitado.current && instituicao.trim() && nomeTipo) {
      setNome(`${instituicao.trim()} ${nomeTipo.nome}`);
    }
  }, [instituicao, nomeTipo]);

  async function salvar() {
    const valorSaldo = lerValor(saldo);
    const valorCaixa = lerValor(saldoCaixa);
    if (valorSaldo === null || valorCaixa === null) {
      setErro('Valor inválido. Use o formato 1.234,56.');
      return;
    }

    const dados: SalvarConta = {
      tipo,
      nome,
      codigoMoeda: moeda,
      saldoInformado: ehInvestimento ? 0 : valorSaldo,
      nomeInstituicao: instituicao || null,
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
      props.aoSalvar(original ? await api.atualizarConta(original.id, dados) : await api.criarConta(dados));
    } catch (falha) {
      if (!(falha instanceof ErroRegra)) {
        throw falha;
      }

      setErro(falha.message);
    } finally {
      setSalvando(false);
    }
  }

  return (
    <Popup
      titulo={original ? 'Editar conta' : 'Nova conta'}
      largura="max-w-[520px]"
      aoFechar={props.aoFechar}
      rodape={
        <>
          <Botao onClick={props.aoFechar}>Cancelar</Botao>
          <Botao variante="primario" disabled={salvando} onClick={salvar}>
            Salvar
          </Botao>
        </>
      }
    >
      <form
        className="grid gap-3"
        onSubmit={(evento) => {
          evento.preventDefault();
          void salvar();
        }}
      >
        <AvisoErro mensagem={erro} />

        <fieldset disabled={original !== null} className="grid gap-1">
          <legend className="mb-1 text-[11.5px] font-semibold text-suave">Tipo</legend>
          <div className="grid grid-cols-4 gap-1.5">
            {opcoes?.tipos.map((t) => (
              <button
                key={t.tipo}
                type="button"
                aria-pressed={t.tipo === tipo}
                title={t.descricao}
                onClick={() => setTipo(t.tipo)}
                className={classes(
                  'grid cursor-pointer justify-items-center gap-1 rounded-lg border px-1 py-2 text-[11px] disabled:cursor-default',
                  t.tipo === tipo ? 'border-realce bg-realce-fundo text-texto' : 'border-borda text-suave hover:border-fraco',
                  original !== null && t.tipo !== tipo && 'opacity-40',
                )}
              >
                <SeloTipo tipo={t.tipo} />
                {t.nome}
              </button>
            ))}
          </div>
          <p className="min-h-8 text-[11.5px] text-suave">{nomeTipo?.descricao}</p>
        </fieldset>

        <Campo rotulo="Instituição (opcional)">
          <input
            className={estiloEntrada}
            list={idInstituicoes}
            value={instituicao}
            placeholder="Escolha ou digite uma nova"
            onChange={(e) => setInstituicao(e.target.value)}
          />
          <datalist id={idInstituicoes}>{instituicoes?.map((i) => <option key={i.id} value={i.nome} />)}</datalist>
        </Campo>

        <Campo rotulo="Nome">
          <input
            className={estiloEntrada}
            value={nome}
            placeholder="Ex.: Conta corrente"
            onChange={(e) => {
              nomeDigitado.current = e.target.value.trim() !== '';
              setNome(e.target.value);
            }}
          />
        </Campo>

        <div className="grid grid-cols-2 gap-3">
          <Campo rotulo="Moeda">
            <select className={estiloEntrada} value={moeda} onChange={(e) => setMoeda(e.target.value)}>
              {opcoes?.moedas.map((m) => (
                <option key={m.codigo} value={m.codigo}>
                  {m.codigo} — {m.nome}
                </option>
              ))}
            </select>
          </Campo>
          {!ehInvestimento && (
            <Campo rotulo={rotuloSaldo[tipo] ?? 'Saldo inicial'}>
              <input
                className={classes(estiloEntrada, 'text-right tabular-nums')}
                inputMode="decimal"
                value={saldo}
                placeholder="0,00"
                onChange={(e) => setSaldo(e.target.value)}
              />
            </Campo>
          )}
        </div>

        {ehInvestimento && (
          <fieldset className="grid gap-2 rounded-lg border border-borda p-3">
            <legend className="px-1 text-[11.5px] font-semibold text-suave">Conta caixa</legend>
            <p className="text-[11.5px] text-suave">Onde fica o dinheiro não investido: recebe as vendas e paga as compras.</p>
            {modosCaixa.map(({ modo, rotulo }) => (
              <label key={modo} className="flex cursor-pointer items-center gap-2">
                <input type="radio" name="modo-caixa" className="accent-realce" checked={modoCaixa === modo} onChange={() => setModoCaixa(modo)} />
                {rotulo}
              </label>
            ))}
            {modoCaixa === 'Existente' && (
              <select className={estiloEntrada} aria-label="Conta caixa existente" value={caixaId} onChange={(e) => setCaixaId(e.target.value)}>
                <option value="">Escolha a conta…</option>
                {caixasCompativeis.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nome}
                  </option>
                ))}
              </select>
            )}
            {modoCaixa === 'Nova' && (
              <div className="grid grid-cols-[1fr_140px] gap-2">
                <input
                  className={estiloEntrada}
                  aria-label="Nome da conta caixa"
                  value={nomeCaixa}
                  placeholder={`${nome.trim()} (Caixa)`}
                  onChange={(e) => setNomeCaixa(e.target.value)}
                />
                <input
                  className={classes(estiloEntrada, 'text-right tabular-nums')}
                  aria-label="Saldo inicial da conta caixa"
                  inputMode="decimal"
                  value={saldoCaixa}
                  placeholder="0,00"
                  onChange={(e) => setSaldoCaixa(e.target.value)}
                />
              </div>
            )}
          </fieldset>
        )}

        <div className="grid grid-cols-2 items-end gap-3">
          <Campo rotulo="Grupo (opcional)">
            <input
              className={estiloEntrada}
              list={idGrupos}
              value={grupo}
              placeholder="Escolha ou digite um novo"
              onChange={(e) => setGrupo(e.target.value)}
            />
            <datalist id={idGrupos}>{grupos?.map((g) => <option key={g.id} value={g.nome} />)}</datalist>
          </Campo>
          <label className="flex h-8 cursor-pointer items-center gap-2">
            <input type="checkbox" className="size-4 accent-realce" checked={favorita} onChange={(e) => setFavorita(e.target.checked)} />
            Conta favorita
          </label>
        </div>
        {/* Permite salvar com Enter em qualquer campo. */}
        <button type="submit" hidden />
      </form>
    </Popup>
  );
}
