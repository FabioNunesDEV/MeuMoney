import { useEffect, useState } from 'react';

/** Tema escolhido pelo usuário; "sistema" acompanha o Windows. */
export type Tema = 'sistema' | 'claro' | 'escuro';

const ordem: Tema[] = ['sistema', 'claro', 'escuro'];
const escuroNoSistema = window.matchMedia('(prefers-color-scheme: dark)');

// A escolha fica num cookie, não no localStorage: a porta do servidor local muda a cada abertura,
// o que troca a origem da página; o cookie vale para o endereço, qualquer que seja a porta.
function lerTema(): Tema {
  const achado = /(?:^|; )tema=(sistema|claro|escuro)/.exec(document.cookie);
  return (achado?.[1] as Tema | undefined) ?? 'sistema';
}

function resolver(tema: Tema): 'claro' | 'escuro' {
  return tema === 'sistema' ? (escuroNoSistema.matches ? 'escuro' : 'claro') : tema;
}

/** Canal de mensagens com a janela do aplicativo; só existe dentro do WebView2. */
interface CanalJanela {
  postMessage(mensagem: unknown): void;
}

// A barra de título é da janela (Windows), não da página: as cores da barra de módulos vão para lá.
function avisarJanela(): void {
  const canal = (window as { chrome?: { webview?: CanalJanela } }).chrome?.webview;
  if (!canal) {
    return;
  }
  const estilo = getComputedStyle(document.documentElement);
  canal.postMessage({
    tipo: 'tema',
    corBarra: estilo.getPropertyValue('--barra').trim(),
    corTexto: estilo.getPropertyValue('--sobre-barra').trim(),
  });
}

function aplicar(efetivo: 'claro' | 'escuro'): void {
  document.documentElement.dataset.tema = efetivo;
  avisarJanela();
}

/** Aplica ao documento o tema guardado; chamado antes de desenhar a tela, para não piscar. */
export function aplicarTemaGuardado(): void {
  aplicar(resolver(lerTema()));
}

/**
 * Tema atual, o tema efetivo (claro ou escuro) e a função que passa para o próximo
 * (sistema → claro → escuro), guardando a escolha.
 */
export function useTema(): { tema: Tema; efetivo: 'claro' | 'escuro'; alternar: () => void } {
  const [tema, setTema] = useState<Tema>(lerTema);
  const [efetivo, setEfetivo] = useState(() => resolver(tema));

  useEffect(() => {
    const atualizar = () => {
      const novo = resolver(tema);
      aplicar(novo);
      setEfetivo(novo);
    };
    atualizar();
    escuroNoSistema.addEventListener('change', atualizar);
    return () => escuroNoSistema.removeEventListener('change', atualizar);
  }, [tema]);

  const alternar = () => {
    const proximo = ordem[(ordem.indexOf(tema) + 1) % ordem.length];
    document.cookie = `tema=${proximo}; max-age=31536000; path=/; samesite=lax`;
    setTema(proximo);
  };

  return { tema, efetivo, alternar };
}
