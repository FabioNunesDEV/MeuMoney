import { Check, Pencil, Plus, X } from 'lucide-react';
import { type ReactNode, useEffect, useId, useRef } from 'react';
import type { NaturezaLancamento } from '../api';
import { classes, dinheiroContabil } from '../comum/formato';
import { AvisoErro, Botao, Confirmacao, estiloEntrada } from '../comum/ui';
import type { FormularioLancamento as Formulario } from './useFormularioLancamento';

const abas: { natureza: NaturezaLancamento; rotulo: string }[] = [
  { natureza: 'Gasto', rotulo: 'Gasto' },
  { natureza: 'Recebimento', rotulo: 'Recebimento' },
  { natureza: 'Transferencia', rotulo: 'Transferência' },
];

const estiloSoLeitura = 'read-only:border-transparent read-only:bg-transparent read-only:px-0';

function Linha({ rotulo, children }: { rotulo: string; children: ReactNode }) {
  return (
    <label className="grid grid-cols-[88px_minmax(0,1fr)] items-center gap-2">
      <span className="text-[11.5px] font-semibold text-suave">{rotulo}</span>
      {children}
    </label>
  );
}

/**
 * Formulário da Movimentação, embaixo da grade como no Money: abas Gasto, Recebimento e Transferência,
 * comandos Novo, Editar, Inserir e Cancelar, e os campos da aba.
 */
export function FormularioLancamento({ f }: { f: Formulario }) {
  const idFavorecidos = useId();
  const idCategorias = useId();
  const idSubcategorias = useId();
  const primeiroCampo = useRef<HTMLInputElement>(null);
  const transferencia = f.aba === 'Transferencia';
  const leitura = !f.editavel;

  useEffect(() => {
    if (f.editavel) {
      primeiroCampo.current?.focus();
    }
  }, [f.editavel, f.aba]);

  const campoTexto = (valor: string, alterar: (v: string) => void, extra?: { lista?: string; rotulo?: string; ref?: typeof primeiroCampo }) => (
    <input
      ref={extra?.ref}
      className={classes(estiloEntrada, estiloSoLeitura)}
      value={valor}
      readOnly={leitura}
      list={leitura ? undefined : extra?.lista}
      aria-label={extra?.rotulo}
      onChange={(e) => alterar(e.target.value)}
    />
  );

  const contas = f.opcoes?.contas ?? [];
  const seletorConta = (rotulo: string, valor: string, alterar: (v: string) => void) =>
    leitura ? (
      <input
        className={classes(estiloEntrada, estiloSoLeitura)}
        aria-label={rotulo}
        readOnly
        value={contas.find((c) => c.id === valor)?.nome ?? ''}
      />
    ) : (
      <select className={estiloEntrada} aria-label={rotulo} value={valor} onChange={(e) => alterar(e.target.value)}>
        <option value="">Escolha a conta</option>
        {contas.map((c) => (
          <option key={c.id} value={c.id}>
            {c.nome}
          </option>
        ))}
      </select>
    );

  return (
    <section className="border-t-2 border-secao bg-painel">
      <div role="tablist" aria-label="Tipo de lançamento" className="flex gap-0.5 bg-secao px-2 pt-1">
        {abas.map((a) => (
          <button
            key={a.natureza}
            type="button"
            role="tab"
            aria-selected={f.aba === a.natureza}
            disabled={f.modo === 'edicao' && f.aba !== a.natureza}
            onClick={() => f.trocarAba(a.natureza)}
            className={classes(
              'cursor-pointer rounded-t-md px-4 py-1 disabled:cursor-default disabled:opacity-45',
              f.aba === a.natureza ? 'bg-painel font-semibold text-texto' : 'text-sobre-barra hover:bg-white/10',
            )}
          >
            {a.rotulo}
          </button>
        ))}
      </div>

      <form
        className="grid gap-3 px-4 py-3"
        onSubmit={(evento) => {
          evento.preventDefault();
          void f.inserir();
        }}
        onKeyDown={(evento) => {
          if (evento.key === 'Escape' && f.editavel) {
            evento.preventDefault();
            f.cancelar();
          }
        }}
      >
        <div role="toolbar" aria-label="Comandos do lançamento" className="flex flex-wrap gap-2">
          <Botao icone={Plus} title="Novo (Ctrl+N)" disabled={f.editavel || f.conta.encerrada} onClick={f.novo}>
            Novo
          </Botao>
          <Botao icone={Pencil} disabled={f.modo !== 'leitura' || f.conta.encerrada} onClick={() => f.editar()}>
            Editar
          </Botao>
          <Botao variante="primario" icone={Check} type="submit" disabled={!f.editavel || f.salvando}>
            Inserir
          </Botao>
          <Botao icone={X} disabled={!f.editavel} onClick={f.cancelar}>
            Cancelar
          </Botao>
        </div>

        <AvisoErro mensagem={f.erro} />

        {f.modo === 'vazio' ? (
          <p className="py-2 text-suave">
            {f.conta.encerrada ? 'Conta encerrada: os lançamentos ficam só para consulta.' : 'Clique em Novo para registrar um lançamento, ou escolha uma linha da grade.'}
          </p>
        ) : (
          <div className="grid grid-cols-[minmax(0,1fr)_minmax(180px,260px)] gap-x-6 gap-y-2 max-[820px]:grid-cols-1">
            <div className="grid content-start gap-2">
              {transferencia && (
                <>
                  <Linha rotulo="De">{seletorConta('De', f.de, f.setDe)}</Linha>
                  <Linha rotulo="Para">{seletorConta('Para', f.para, f.setPara)}</Linha>
                </>
              )}
              <Linha rotulo="Pagar a">{campoTexto(f.favorecido, f.alterarFavorecido, { lista: idFavorecidos, ref: primeiroCampo })}</Linha>
              {!transferencia && (
                <Linha rotulo="Categoria">
                  <span className="grid grid-cols-2 gap-2">
                    {campoTexto(f.categoria, f.setCategoria, { lista: idCategorias, rotulo: 'Categoria' })}
                    {campoTexto(f.subcategoria, f.setSubcategoria, { lista: idSubcategorias, rotulo: 'Subcategoria (opcional)' })}
                  </span>
                </Linha>
              )}
              <Linha rotulo="Memo">{campoTexto(f.memo, f.setMemo)}</Linha>
            </div>
            <div className="grid content-start gap-2">
              <Linha rotulo="Data">
                <input
                  type="date"
                  className={classes(estiloEntrada, estiloSoLeitura)}
                  value={f.data}
                  readOnly={leitura}
                  onChange={(e) => f.setData(e.target.value)}
                />
              </Linha>
              <Linha rotulo="Montante">
                <input
                  className={classes(estiloEntrada, estiloSoLeitura, 'text-right tabular-nums')}
                  inputMode="decimal"
                  placeholder="0,00"
                  value={f.montante}
                  readOnly={leitura}
                  onChange={(e) => f.setMontante(e.target.value)}
                />
              </Linha>
            </div>
          </div>
        )}

        <datalist id={idFavorecidos}>{f.opcoes?.favorecidos.map((x) => <option key={x.nome} value={x.nome} />)}</datalist>
        <datalist id={idCategorias}>{f.opcoes?.categorias.map((x) => <option key={x.nome} value={x.nome} />)}</datalist>
        <datalist id={idSubcategorias}>{f.subcategorias.map((x) => <option key={x} value={x} />)}</datalist>
      </form>

      {f.saldosNegativos && (
        <Confirmacao
          titulo="Saldo negativo"
          texto={`${f.saldosNegativos
            .map((s) => `O saldo de "${s.nomeConta}" ficará em ${dinheiroContabil(s.saldoFinal, s.moeda)}.`)
            .join(' ')} Está correto?`}
          confirmar="Confirmar"
          aoConfirmar={() => void f.inserir(true)}
          aoCancelar={f.descartarConfirmacao}
        />
      )}
    </section>
  );
}
