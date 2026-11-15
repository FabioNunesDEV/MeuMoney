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

/** Aplica ao documento o tema guardado; chamado antes de desenhar a tela, para não piscar. */
export function aplicarTemaGuardado(): void {
  document.documentElement.dataset.tema = resolver(lerTema());
}

/**
 * Tema atual, o tema efetivo (claro ou escuro) e a função que passa para o próximo
 * (sistema → claro → escuro), guardando a escolha.
 */
export function useTema(): { tema: Tema; efetivo: 'claro' | 'escuro'; alternar: () => void } {
  const [tema, setTema] = useState<Tema>(lerTema);
  const [efetivo, setEfetivo] = useState(() => resolver(tema));

  useEffect(() => {
    const aplicar = () => {
      const novo = resolver(tema);
      document.documentElement.dataset.tema = novo;
      setEfetivo(novo);
    };
    aplicar();
    escuroNoSistema.addEventListener('change', aplicar);
    return () => escuroNoSistema.removeEventListener('change', aplicar);
  }, [tema]);

  const alternar = () => {
    const proximo = ordem[(ordem.indexOf(tema) + 1) % ordem.length];
    document.cookie = `tema=${proximo}; max-age=31536000; path=/; samesite=lax`;
    setTema(proximo);
  };

  return { tema, efetivo, alternar };
}
