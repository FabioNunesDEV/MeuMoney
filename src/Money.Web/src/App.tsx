import { type LucideIcon, Monitor, Moon, Sun, Wallet } from 'lucide-react';
import { useState } from 'react';
import { Toaster } from 'sonner';
import { classes } from './comum/formato';
import { type Tema, useTema } from './comum/tema';
import { ContaPagina } from './contas/ContaPagina';
import { Contas } from './contas/Contas';
import { Inicio } from './inicio/Inicio';

type Modulo = 'inicio' | 'contas';
type Local = { modulo: Modulo; contaId?: string };

// Barra de módulos, na ordem do Money. Os que ainda não existem aparecem desativados.
const modulos: { chave?: Modulo; nome: string }[] = [
  { chave: 'inicio', nome: 'Início' },
  { chave: 'contas', nome: 'Contas' },
  { nome: 'Contas a pagar' },
  { nome: 'Investimentos' },
  { nome: 'Planejador' },
  { nome: 'Relatórios' },
  { nome: 'Categorias' },
];

const temas: Record<Tema, { nome: string; icone: LucideIcon }> = {
  sistema: { nome: 'Tema: do Windows', icone: Monitor },
  claro: { nome: 'Tema: claro', icone: Sun },
  escuro: { nome: 'Tema: escuro', icone: Moon },
};

/** Casca do aplicativo: barra de módulos no topo e, abaixo, a página do módulo atual. */
export function App() {
  const [local, setLocal] = useState<Local>({ modulo: 'inicio' });
  const { tema, efetivo, alternar } = useTema();
  const IconeTema = temas[tema].icone;
  const abrirConta = (contaId: string) => setLocal({ modulo: 'contas', contaId });
  const irParaContas = () => setLocal({ modulo: 'contas' });

  return (
    <>
      <div className="grid h-full grid-rows-[auto_minmax(0,1fr)]">
        <nav aria-label="Módulos" className="flex items-center gap-1 bg-barra px-3 text-sobre-barra">
          <span className="mr-3 flex items-center gap-2 py-2">
            <span className="grid size-6 place-items-center rounded-md bg-destaque text-sobre-destaque">
              <Wallet className="size-4" aria-hidden />
            </span>
            <b className="text-sm max-[820px]:hidden">Meu Money</b>
          </span>
          <div className="flex min-w-0 flex-1 items-stretch overflow-hidden">
            {modulos.map((modulo) => {
              const atual = modulo.chave === local.modulo;
              return (
                <button
                  key={modulo.nome}
                  type="button"
                  disabled={!modulo.chave}
                  title={modulo.chave ? undefined : 'Em breve'}
                  aria-current={atual ? 'page' : undefined}
                  onClick={() => modulo.chave && setLocal({ modulo: modulo.chave })}
                  className={classes(
                    'cursor-pointer border-b-2 px-3 py-2.5 whitespace-nowrap disabled:cursor-default disabled:opacity-40',
                    atual ? 'border-destaque font-semibold text-white' : 'border-transparent text-sobre-barra-suave hover:text-white',
                  )}
                >
                  {modulo.nome}
                </button>
              );
            })}
          </div>
          <button
            type="button"
            title={`${temas[tema].nome} (clique para trocar)`}
            aria-label={`${temas[tema].nome}. Clique para trocar.`}
            onClick={alternar}
            className="flex h-7 cursor-pointer items-center gap-1.5 rounded-md px-2 text-sobre-barra-suave hover:bg-white/10 hover:text-white"
          >
            <IconeTema className="size-4" aria-hidden />
            <span className="max-[1000px]:hidden">{temas[tema].nome}</span>
          </button>
        </nav>
        <main className="min-h-0 min-w-0">
          {local.modulo === 'inicio' && <Inicio irParaContas={irParaContas} abrirConta={abrirConta} />}
          {local.modulo === 'contas' && !local.contaId && <Contas abrirConta={abrirConta} />}
          {local.modulo === 'contas' && local.contaId && <ContaPagina id={local.contaId} voltar={irParaContas} />}
        </main>
      </div>
      <Toaster position="bottom-center" theme={efetivo === 'escuro' ? 'dark' : 'light'} richColors closeButton />
    </>
  );
}
