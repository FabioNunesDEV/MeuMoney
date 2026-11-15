import { useCallback, useEffect, useState } from 'react';

/**
 * Carrega dados da API ao montar e sempre que as dependências mudarem.
 * Devolve os dados (undefined enquanto carrega) e a função de recarregar.
 */
export function useDados<T>(carregar: () => Promise<T>, dependencias: unknown[]): [T | undefined, () => Promise<void>] {
  const [dados, setDados] = useState<T>();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  const recarregar = useCallback(async () => setDados(await carregar()), dependencias);

  useEffect(() => {
    void recarregar();
  }, [recarregar]);

  return [dados, recarregar];
}
