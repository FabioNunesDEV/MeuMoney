// Componentes de interface compartilhados: botão, campo, pop-up, menus e página com barra de ferramentas.
import { CircleAlert, type LucideIcon, X } from 'lucide-react';
import { AlertDialog, ContextMenu, Dialog, DropdownMenu } from 'radix-ui';
import type { ButtonHTMLAttributes, ReactNode } from 'react';
import { classes } from './formato';

type VarianteBotao = 'padrao' | 'primario' | 'discreto' | 'perigo' | 'barra' | 'destaque';

const estiloBotao: Record<VarianteBotao, string> = {
  padrao: 'border border-borda bg-painel hover:border-fraco',
  primario: 'border border-realce bg-realce text-white hover:brightness-110',
  discreto: 'border border-transparent text-suave hover:bg-fundo hover:text-texto',
  perigo: 'border border-negativo bg-negativo text-white hover:brightness-110',
  // Sobre as barras escuras: comando comum e ação principal (âmbar).
  barra: 'border border-white/20 text-sobre-barra hover:bg-white/10',
  destaque: 'border border-destaque bg-destaque text-sobre-destaque hover:brightness-105',
};

interface PropsBotao extends ButtonHTMLAttributes<HTMLButtonElement> {
  variante?: VarianteBotao;
  icone?: LucideIcon;
}

/** Botão padrão do aplicativo, com ícone opcional. Sem texto, exige `aria-label`. */
export function Botao({ variante = 'padrao', icone: Icone, children, className, ...resto }: PropsBotao) {
  return (
    <button
      type="button"
      className={classes(
        'inline-flex h-8 cursor-pointer items-center gap-1.5 rounded-md font-medium whitespace-nowrap transition-colors disabled:cursor-default disabled:opacity-50',
        children ? 'px-3' : 'w-8 justify-center',
        estiloBotao[variante],
        className,
      )}
      {...resto}
    >
      {Icone && <Icone className="size-4" aria-hidden />}
      {children}
    </button>
  );
}

/** Rótulo com o controle do formulário logo abaixo. */
export function Campo({ rotulo, children, className }: { rotulo: string; children: ReactNode; className?: string }) {
  return (
    <label className={classes('grid gap-1', className)}>
      <span className="text-[11.5px] font-semibold text-suave">{rotulo}</span>
      {children}
    </label>
  );
}

/** Classe comum de caixas de texto e listas de seleção, sem largura definida (barra de ferramentas). */
export const estiloEntradaBase =
  'h-8 rounded-md border border-borda px-2.5 text-texto placeholder:text-fraco focus:border-realce focus:outline-none';

/** Caixa de texto ou lista de seleção de formulário: ocupa a largura do campo. */
export const estiloEntrada = `${estiloEntradaBase} w-full bg-fundo`;

/** Aviso de erro exibido dentro de um pop-up, junto ao formulário. */
export function AvisoErro({ mensagem }: { mensagem: string | null }) {
  if (!mensagem) {
    return null;
  }

  return (
    <div role="alert" className="mb-3 flex items-start gap-2 rounded-md border border-negativo/40 bg-negativo/10 px-3 py-2 text-negativo">
      <CircleAlert className="mt-px size-4 shrink-0" aria-hidden />
      <span>{mensagem}</span>
    </div>
  );
}

const estiloVeu = 'fixed inset-0 z-40 bg-black/40 backdrop-blur-[2px]';
const estiloJanela =
  'fixed top-1/2 left-1/2 z-50 max-h-[calc(100vh-48px)] w-[calc(100vw-32px)] -translate-x-1/2 -translate-y-1/2 overflow-auto rounded-xl border border-borda bg-painel p-5 shadow-2xl focus:outline-none';

/** Pop-up modal com título, conteúdo e rodapé de botões. */
export function Popup(props: { titulo: string; largura?: string; aoFechar: () => void; children: ReactNode; rodape?: ReactNode }) {
  return (
    <Dialog.Root open onOpenChange={(aberto) => !aberto && props.aoFechar()}>
      <Dialog.Portal>
        <Dialog.Overlay className={estiloVeu} />
        <Dialog.Content className={classes(estiloJanela, props.largura ?? 'max-w-[480px]')} aria-describedby={undefined}>
          <div className="mb-4 flex items-center">
            <Dialog.Title className="text-base font-semibold">{props.titulo}</Dialog.Title>
            <Dialog.Close asChild>
              <Botao variante="discreto" icone={X} aria-label="Fechar" className="-mr-1.5 ml-auto" />
            </Dialog.Close>
          </div>
          {props.children}
          {props.rodape && <div className="mt-5 flex justify-end gap-2">{props.rodape}</div>}
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}

/** Pop-up de confirmação de uma ação que não se desfaz. */
export function Confirmacao(props: { titulo: string; texto: string; confirmar: string; aoConfirmar: () => void; aoCancelar: () => void }) {
  return (
    <AlertDialog.Root open onOpenChange={(aberto) => !aberto && props.aoCancelar()}>
      <AlertDialog.Portal>
        <AlertDialog.Overlay className={estiloVeu} />
        <AlertDialog.Content className={classes(estiloJanela, 'max-w-[400px]')}>
          <AlertDialog.Title className="text-base font-semibold">{props.titulo}</AlertDialog.Title>
          <AlertDialog.Description className="mt-2 text-suave">{props.texto}</AlertDialog.Description>
          <div className="mt-5 flex justify-end gap-2">
            <AlertDialog.Cancel asChild>
              <Botao>Cancelar</Botao>
            </AlertDialog.Cancel>
            <AlertDialog.Action asChild>
              <Botao variante="perigo" onClick={props.aoConfirmar}>
                {props.confirmar}
              </Botao>
            </AlertDialog.Action>
          </div>
        </AlertDialog.Content>
      </AlertDialog.Portal>
    </AlertDialog.Root>
  );
}

/** Uma ação de menu; `separar` desenha uma linha antes dela. */
export interface AcaoMenu {
  rotulo: string;
  icone: LucideIcon;
  aoEscolher: () => void;
  perigo?: boolean;
  separar?: boolean;
}

const estiloMenu = 'z-50 min-w-[190px] rounded-lg border border-borda bg-painel p-1 shadow-xl';
const estiloItem =
  'flex cursor-pointer items-center gap-2 rounded-md px-2.5 py-1.5 outline-none select-none data-[highlighted]:bg-fundo';

// O menu de contexto e o menu do botão "Mais ações" mostram as mesmas ações; só muda a peça do Radix.
function itens(Peca: typeof ContextMenu | typeof DropdownMenu, acoes: AcaoMenu[]) {
  return acoes.map((acao) => (
    <div key={acao.rotulo}>
      {acao.separar && <Peca.Separator className="my-1 h-px bg-borda" />}
      <Peca.Item className={classes(estiloItem, acao.perigo && 'text-negativo')} onSelect={acao.aoEscolher}>
        <acao.icone className="size-4" aria-hidden />
        {acao.rotulo}
      </Peca.Item>
    </div>
  ));
}

/** Menu de contexto (clique direito ou tecla de menu) em volta do conteúdo. */
export function MenuContexto({ acoes, children }: { acoes: AcaoMenu[]; children: ReactNode }) {
  return (
    <ContextMenu.Root>
      <ContextMenu.Trigger asChild>{children}</ContextMenu.Trigger>
      <ContextMenu.Portal>
        <ContextMenu.Content className={estiloMenu}>{itens(ContextMenu, acoes)}</ContextMenu.Content>
      </ContextMenu.Portal>
    </ContextMenu.Root>
  );
}

/** Menu aberto por um botão (o gatilho). */
export function MenuBotao({ acoes, gatilho }: { acoes: AcaoMenu[]; gatilho: ReactNode }) {
  return (
    <DropdownMenu.Root>
      <DropdownMenu.Trigger asChild>{gatilho}</DropdownMenu.Trigger>
      <DropdownMenu.Portal>
        <DropdownMenu.Content align="end" sideOffset={4} className={estiloMenu}>
          {itens(DropdownMenu, acoes)}
        </DropdownMenu.Content>
      </DropdownMenu.Portal>
    </DropdownMenu.Root>
  );
}

/**
 * Página no arranjo do Money: faixa de título com os comandos, painel de tarefas à esquerda,
 * barra de opções de exibição (opcional), conteúdo rolável e barra de situação no rodapé (opcional).
 */
export function Pagina(props: {
  titulo: string;
  ferramentas?: ReactNode;
  painel: ReactNode;
  opcoes?: ReactNode;
  rodape?: ReactNode;
  children: ReactNode;
}) {
  return (
    <div className="grid h-full min-w-0 grid-cols-[190px_minmax(0,1fr)] grid-rows-[minmax(0,1fr)] max-[1000px]:grid-cols-[minmax(0,1fr)]">
      <aside className="overflow-auto bg-lateral px-3 py-4 text-sobre-barra max-[1000px]:hidden">{props.painel}</aside>
      <div className="flex min-h-0 min-w-0 flex-col">
        <header className="flex min-h-11 flex-wrap items-center gap-x-4 gap-y-1.5 bg-faixa px-4 py-1.5 text-sobre-barra">
          <h1 className="mr-auto text-[15px] font-semibold">{props.titulo}</h1>
          {props.ferramentas && <div role="toolbar" className="flex flex-wrap items-center gap-2">{props.ferramentas}</div>}
        </header>
        {props.opcoes && (
          <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5 border-b border-borda bg-painel px-4 py-1.5">{props.opcoes}</div>
        )}
        <div className="flex-1 overflow-auto px-4 py-3">{props.children}</div>
        {props.rodape && (
          <footer className="flex flex-wrap items-center justify-end gap-x-5 border-t border-borda bg-painel px-4 py-1.5 text-suave">
            {props.rodape}
          </footer>
        )}
      </div>
    </div>
  );
}

/** Bloco do painel de tarefas: um título pequeno e os itens abaixo. */
export function BlocoPainel({ titulo, children }: { titulo: string; children: ReactNode }) {
  return (
    <section className="mb-5">
      <h2 className="mb-1.5 px-2 text-[10.5px] font-semibold tracking-wider text-sobre-barra-suave uppercase">{titulo}</h2>
      <div className="grid gap-0.5">{children}</div>
    </section>
  );
}

/** Item do painel de tarefas: uma ação ou uma visão da página; `ativo` marca a visão atual. */
export function Tarefa(props: { icone: LucideIcon; ativo?: boolean; embreve?: boolean; aoClicar?: () => void; children: ReactNode }) {
  return (
    <button
      type="button"
      disabled={props.embreve}
      title={props.embreve ? 'Em breve' : undefined}
      onClick={props.aoClicar}
      className={classes(
        'flex min-h-7 w-full cursor-pointer items-center gap-2 rounded-md px-2 py-1 text-left disabled:cursor-default disabled:opacity-45',
        props.ativo ? 'bg-white/15 font-semibold' : 'hover:bg-white/10',
      )}
    >
      <props.icone className="size-4 shrink-0 text-destaque" aria-hidden />
      <span className="min-w-0 flex-1">{props.children}</span>
    </button>
  );
}

/** Seção do conteúdo: faixa colorida com o título (como as faixas do Money) e o corpo abaixo. */
export function Secao(props: { titulo: string; detalhe?: ReactNode; children: ReactNode; className?: string }) {
  return (
    <section className={classes('mb-4 overflow-hidden rounded-lg border border-borda bg-painel', props.className)}>
      <div className="flex items-center gap-3 bg-secao px-3 py-1 text-sobre-barra">
        <h2 className="text-xs font-semibold">{props.titulo}</h2>
        {props.detalhe && <span className="ml-auto text-[11.5px] text-sobre-barra-suave">{props.detalhe}</span>}
      </div>
      {props.children}
    </section>
  );
}

/** Cartão: superfície com borda para agrupar conteúdo. `espaco` troca o preenchimento interno padrão. */
export function Cartao(props: { titulo?: string; children: ReactNode; className?: string; espaco?: string }) {
  const { titulo, children, className } = props;
  return (
    <section className={classes('rounded-xl border border-borda bg-painel', props.espaco ?? 'p-4', className)}>
      {titulo && <h2 className="mb-3 text-xs font-semibold text-suave">{titulo}</h2>}
      {children}
    </section>
  );
}
