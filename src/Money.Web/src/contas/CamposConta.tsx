// Seções do formulário de conta. O assistente mostra uma por passo; a edição mostra todas juntas.
import { useId } from 'react';
import type { ModoContaCaixa, TipoConta } from '../api';
import { classes } from '../comum/formato';
import { SeloTipo } from '../comum/tiposConta';
import { Campo, estiloEntrada } from '../comum/ui';
import type { FormularioConta } from './useFormularioConta';

/** Propriedades comuns das seções: o formulário e se o primeiro campo recebe o foco ao aparecer. */
interface PropsCampos {
  f: FormularioConta;
  focar?: boolean;
}

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

const estiloOpcao = 'flex cursor-pointer items-center gap-2';
const estiloRadio = 'accent-realce';

/** Instituição: mantida numa instituição (escolhida ou digitada) ou em nenhuma. */
export function CamposInstituicao({ f, focar }: PropsCampos) {
  const nomeGrupo = useId();
  const idLista = useId();
  return (
    <div className="grid gap-2">
      <label className={estiloOpcao}>
        <input type="radio" name={nomeGrupo} className={estiloRadio} checked={f.mantida} onChange={() => f.setMantida(true)} />
        É mantida no(a)
      </label>
      <input
        className={classes(estiloEntrada, 'disabled:opacity-50')}
        aria-label="Instituição financeira"
        list={idLista}
        value={f.instituicao}
        placeholder="Escolha ou digite uma nova"
        disabled={!f.mantida}
        autoFocus={focar && f.mantida}
        onChange={(e) => f.setInstituicao(e.target.value)}
      />
      <datalist id={idLista}>{f.instituicoes?.map((i) => <option key={i.id} value={i.nome} />)}</datalist>
      <label className={estiloOpcao}>
        <input
          type="radio"
          name={nomeGrupo}
          className={estiloRadio}
          checked={!f.mantida}
          autoFocus={focar && !f.mantida}
          onChange={() => f.setMantida(false)}
        />
        Não é mantida em banco, corretora ou outra instituição
      </label>
    </div>
  );
}

/** Tipo: lista de tipos com a descrição do escolhido ao lado. Na edição, só mostra o tipo, que não muda. */
export function CamposTipo({ f, focar, somenteLeitura }: PropsCampos & { somenteLeitura?: boolean }) {
  const nomeGrupo = useId();
  if (somenteLeitura) {
    return (
      <div className="flex items-center gap-2">
        <SeloTipo tipo={f.tipo} />
        <span className="font-medium">{f.infoTipo?.nome}</span>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-[minmax(0,1fr)_minmax(0,1fr)] gap-3">
      <div className="grid content-start gap-0.5">
        {f.opcoes?.tipos.map((t) => (
          <label
            key={t.tipo}
            className={classes(
              'flex cursor-pointer items-center gap-2 rounded-md border px-2 py-1 has-[:focus-visible]:outline-2 has-[:focus-visible]:outline-realce',
              t.tipo === f.tipo ? 'border-realce bg-realce-fundo font-medium' : 'border-transparent hover:bg-fundo',
            )}
          >
            {/* Rádio nativo escondido: dá as setas e o anúncio do leitor de tela sem código extra. */}
            <input
              type="radio"
              name={nomeGrupo}
              className="sr-only"
              checked={t.tipo === f.tipo}
              autoFocus={focar && t.tipo === f.tipo}
              onChange={() => f.setTipo(t.tipo)}
            />
            <SeloTipo tipo={t.tipo} />
            {t.nome}
          </label>
        ))}
      </div>
      <p aria-live="polite" className="rounded-md border border-borda bg-fundo p-3 text-suave">
        {f.infoTipo?.descricao}
      </p>
    </div>
  );
}

/** Nome da conta, com grupo e favorita opcionais. */
export function CamposNome({ f, focar }: PropsCampos) {
  const idGrupos = useId();
  return (
    <div className="grid gap-3">
      <Campo rotulo="Nome">
        <input
          className={estiloEntrada}
          value={f.nome}
          placeholder="Ex.: Carteira"
          autoFocus={focar}
          onChange={(e) => f.alterarNome(e.target.value)}
        />
      </Campo>
      <div className="grid grid-cols-2 items-end gap-3">
        <Campo rotulo="Grupo (opcional)">
          <input
            className={estiloEntrada}
            list={idGrupos}
            value={f.grupo}
            placeholder="Escolha ou digite um novo"
            onChange={(e) => f.setGrupo(e.target.value)}
          />
          <datalist id={idGrupos}>{f.grupos?.map((g) => <option key={g.id} value={g.nome} />)}</datalist>
        </Campo>
        <label className="flex h-8 cursor-pointer items-center gap-2">
          <input type="checkbox" className="size-4 accent-realce" checked={f.favorita} onChange={(e) => f.setFavorita(e.target.checked)} />
          Conta favorita
        </label>
      </div>
    </div>
  );
}

/** Número da conta, opcional: nos campos complementares e no passo próprio do assistente. */
export function CampoNumeroConta({ f, focar }: PropsCampos) {
  return (
    <Campo rotulo="Número da conta">
      <input className={estiloEntrada} value={f.numeroConta} autoFocus={focar} onChange={(e) => f.setNumeroConta(e.target.value)} />
    </Campo>
  );
}

/** Saldo mínimo, opcional; só aparece nos tipos que o têm. */
export function CampoSaldoMinimo({ f }: PropsCampos) {
  if (!f.infoTipo?.temSaldoMinimo) {
    return null;
  }

  return (
    <Campo rotulo="Saldo mínimo">
      <input
        className={classes(estiloEntrada, 'text-right tabular-nums')}
        inputMode="decimal"
        value={f.saldoMinimo}
        placeholder="Sem saldo mínimo"
        onChange={(e) => f.setSaldoMinimo(e.target.value)}
      />
    </Campo>
  );
}

/** Dados complementares, todos opcionais: número, abreviação, saldo mínimo (nos tipos que o têm) e comentário. */
export function CamposComplementares({ f }: PropsCampos) {
  return (
    <div className="grid gap-3">
      <div className="grid grid-cols-2 gap-3">
        <CampoNumeroConta f={f} />
        <Campo rotulo="Abreviação">
          <input className={estiloEntrada} value={f.abreviacao} onChange={(e) => f.setAbreviacao(e.target.value)} />
        </Campo>
        <CampoSaldoMinimo f={f} />
      </div>
      <Campo rotulo="Comentário">
        <textarea
          className={classes(estiloEntrada, 'h-20 resize-y py-1.5')}
          value={f.comentario}
          onChange={(e) => f.setComentario(e.target.value)}
        />
      </Campo>
    </div>
  );
}

/** Moeda e saldo inicial; no Investimento, a conta caixa no lugar do saldo. */
export function CamposSaldo({ f, focar }: PropsCampos) {
  const ehInvestimento = f.tipo === 'Investimento';
  const nomeModos = useId();
  return (
    <div className="grid gap-3">
      <div className="grid grid-cols-2 gap-3">
        {!ehInvestimento && (
          <Campo rotulo={rotuloSaldo[f.tipo] ?? 'Saldo inicial'}>
            <input
              className={classes(estiloEntrada, 'text-right tabular-nums')}
              inputMode="decimal"
              value={f.saldo}
              placeholder="0,00"
              autoFocus={focar}
              onChange={(e) => f.setSaldo(e.target.value)}
            />
          </Campo>
        )}
        <Campo rotulo="Moeda">
          <select className={estiloEntrada} value={f.moeda} autoFocus={focar && ehInvestimento} onChange={(e) => f.setMoeda(e.target.value)}>
            {f.opcoes?.moedas.map((m) => (
              <option key={m.codigo} value={m.codigo}>
                {m.codigo} — {m.nome}
              </option>
            ))}
          </select>
        </Campo>
      </div>

      {ehInvestimento && (
        <fieldset className="grid gap-2 rounded-lg border border-borda p-3">
          <legend className="px-1 text-[11.5px] font-semibold text-suave">Conta caixa</legend>
          <p className="text-[11.5px] text-suave">Onde fica o dinheiro não investido: recebe as vendas e paga as compras.</p>
          {modosCaixa.map(({ modo, rotulo }) => (
            <label key={modo} className={estiloOpcao}>
              <input type="radio" name={nomeModos} className={estiloRadio} checked={f.modoCaixa === modo} onChange={() => f.setModoCaixa(modo)} />
              {rotulo}
            </label>
          ))}
          {f.modoCaixa === 'Existente' && (
            <select className={estiloEntrada} aria-label="Conta caixa existente" value={f.caixaId} onChange={(e) => f.setCaixaId(e.target.value)}>
              <option value="">Escolha a conta…</option>
              {f.caixasCompativeis.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nome}
                </option>
              ))}
            </select>
          )}
          {f.modoCaixa === 'Nova' && (
            <div className="grid grid-cols-[1fr_140px] gap-2">
              <input
                className={estiloEntrada}
                aria-label="Nome da conta caixa"
                value={f.nomeCaixa}
                placeholder={`${f.nome.trim()} (Caixa)`}
                onChange={(e) => f.setNomeCaixa(e.target.value)}
              />
              <input
                className={classes(estiloEntrada, 'text-right tabular-nums')}
                aria-label="Saldo inicial da conta caixa"
                inputMode="decimal"
                value={f.saldoCaixa}
                placeholder="0,00"
                onChange={(e) => f.setSaldoCaixa(e.target.value)}
              />
            </div>
          )}
        </fieldset>
      )}
    </div>
  );
}
