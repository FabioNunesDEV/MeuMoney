import { Plus, Star, Wallet } from 'lucide-react';
import type { CSSProperties } from 'react';
import { api, type TipoConta } from '../api';
import { dinheiro, hojePorExtenso } from '../comum/formato';
import { aparenciaTipo, SeloTipo } from '../comum/tiposConta';
import { BlocoPainel, Botao, Pagina, Secao, Tarefa, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';

/** Página Início, como a home page financeira do Money: saldos, distribuição e contas favoritas. */
export function Inicio(props: { irParaContas: () => void; abrirConta: (id: string) => void }) {
  const [gerenciador] = useDados(() => api.gerenciador(false), []);
  const painel = (
    <>
      <div className="mb-5 px-2">
        <p className="text-lg leading-tight font-bold">Meu Money</p>
        <p className="mt-2 text-[11.5px] text-sobre-barra-suave first-letter:uppercase">{hojePorExtenso()}</p>
      </div>
      <BlocoPainel titulo="Ir para">
        <Tarefa icone={Wallet} aoClicar={props.irParaContas}>
          Gerenciador de contas
        </Tarefa>
      </BlocoPainel>
    </>
  );

  if (!gerenciador) {
    return <Pagina titulo="Início" painel={painel} children={null} />;
  }

  if (gerenciador.contas.length === 0) {
    return (
      <Pagina titulo="Início" painel={painel}>
        <div className="mx-auto mt-20 grid max-w-sm justify-items-center gap-2 text-center">
          <span className="grid size-14 place-items-center rounded-2xl bg-realce-fundo text-realce">
            <Wallet className="size-7" aria-hidden />
          </span>
          <h2 className="mt-2 text-base font-semibold">Bem-vindo ao Meu Money</h2>
          <p className="text-suave">Cadastre suas contas para acompanhar aqui os saldos e as suas contas favoritas.</p>
          <Botao variante="primario" icone={Plus} className="mt-3" onClick={props.irParaContas}>
            Cadastrar contas
          </Botao>
        </div>
      </Pagina>
    );
  }

  // A distribuição considera a moeda principal (a primeira dos totais) e só saldos positivos.
  const principal = gerenciador.totais[0].moeda;
  const positivas = gerenciador.contas.filter((c) => c.moeda.codigo === principal.codigo && c.saldo > 0);
  const totalPositivo = positivas.reduce((soma, c) => soma + c.saldo, 0);
  const fatias = (Object.keys(aparenciaTipo) as TipoConta[])
    .map((tipo) => {
      const doTipo = positivas.filter((c) => c.tipo === tipo);
      return { tipo, nome: doTipo[0]?.nomeTipo, valor: doTipo.reduce((soma, c) => soma + c.saldo, 0) };
    })
    .filter((f) => f.valor > 0);
  const dividas = gerenciador.contas
    .filter((c) => c.moeda.codigo === principal.codigo && c.saldo < 0)
    .reduce((soma, c) => soma + c.saldo, 0);
  const favoritas = gerenciador.contas.filter((c) => c.favorita);

  return (
    <Pagina titulo="Início" painel={painel}>
      <div className="grid grid-cols-[minmax(0,1.15fr)_minmax(0,1fr)] items-start gap-4 max-[1100px]:grid-cols-1">
        <Secao titulo="Contas favoritas" detalhe="Saldo final">
          {favoritas.length === 0 ? (
            <div className="flex items-center gap-3 px-3 py-4 text-suave">
              <Star className="size-5 text-fraco" aria-hidden />
              <p>Nenhuma conta favorita. No gerenciador de contas, abra o menu de uma conta e escolha "Marcar como favorita".</p>
            </div>
          ) : (
            <ul>
              {favoritas.map((conta) => (
                <li key={conta.id} className="border-b border-borda last:border-b-0">
                  <button
                    type="button"
                    onClick={() => props.abrirConta(conta.id)}
                    className="flex w-full cursor-pointer items-center gap-3 px-3 py-1.5 text-left hover:bg-fundo"
                  >
                    <SeloTipo tipo={conta.tipo} />
                    <span className="min-w-0 flex-1">
                      <span className="block truncate font-medium text-realce underline-offset-2 hover:underline">{conta.nome}</span>
                      <span className="block truncate text-[11.5px] text-suave">{conta.nomeInstituicao ?? 'Sem instituição'}</span>
                    </span>
                    <ValorSaldo valor={conta.saldo} moeda={conta.moeda} />
                  </button>
                </li>
              ))}
            </ul>
          )}
        </Secao>

        <div>
          <Secao titulo="Saldos">
            <dl>
              {gerenciador.totais.map((total) => (
                <div key={total.moeda.codigo} className="flex items-baseline gap-3 border-b border-borda px-3 py-2 last:border-b-0">
                  <dt className="flex-1 text-suave">Total em {total.moeda.nome.toLowerCase()}</dt>
                  <dd className="text-lg">
                    <ValorSaldo valor={total.total} moeda={total.moeda} />
                  </dd>
                </div>
              ))}
              {dividas < 0 && (
                <div className="flex items-baseline gap-3 px-3 py-2">
                  <dt className="flex-1 text-suave">Dívidas em {principal.nome.toLowerCase()}</dt>
                  <dd>
                    <ValorSaldo valor={dividas} moeda={principal} />
                  </dd>
                </div>
              )}
            </dl>
          </Secao>

          {fatias.length > 0 && (
            <Secao titulo="Onde está o dinheiro" detalhe={principal.nome}>
              <div className="p-3">
                <div className="flex h-3 gap-0.5 overflow-hidden rounded-full">
                  {fatias.map((f) => (
                    <span key={f.tipo} style={{ width: `${(f.valor / totalPositivo) * 100}%`, background: aparenciaTipo[f.tipo].cor }} />
                  ))}
                </div>
                <ul className="mt-3 grid gap-1">
                  {fatias.map((f) => (
                    <li key={f.tipo} className="flex items-center gap-2">
                      <span className="size-2.5 rounded-full" style={{ background: aparenciaTipo[f.tipo].cor } as CSSProperties} />
                      <span className="flex-1 text-suave">{f.nome}</span>
                      <span className="tabular-nums">{dinheiro(f.valor, principal)}</span>
                      <b className="w-10 text-right font-semibold tabular-nums">{Math.round((f.valor / totalPositivo) * 100)}%</b>
                    </li>
                  ))}
                </ul>
              </div>
            </Secao>
          )}
        </div>
      </div>
    </Pagina>
  );
}
