import type { ReactNode } from 'react';
import type { Conta } from '../api';
import { AvisoErro, Botao, Popup } from '../comum/ui';
import { CamposInstituicao, CamposNome, CamposSaldo, CamposTipo } from './CamposConta';
import { useFormularioConta } from './useFormularioConta';

function Grupo({ titulo, children }: { titulo: string; children: ReactNode }) {
  return (
    <fieldset className="grid gap-1.5">
      <legend className="mb-1.5 text-[11.5px] font-semibold text-suave">{titulo}</legend>
      {children}
    </fieldset>
  );
}

/** Pop-up de editar conta: todos os campos à vista, com o tipo fixo. A criação usa o assistente. */
export function ContaDialogo(props: { conta: Conta; aoFechar: () => void; aoSalvar: (conta: Conta) => void }) {
  const f = useFormularioConta(props.conta);

  async function salvar() {
    const salva = await f.salvar();
    if (salva) {
      props.aoSalvar(salva);
    }
  }

  return (
    <Popup
      titulo="Editar conta"
      largura="max-w-[520px]"
      aoFechar={props.aoFechar}
      rodape={
        <>
          <Botao onClick={props.aoFechar}>Cancelar</Botao>
          <Botao variante="primario" disabled={f.salvando} onClick={salvar}>
            Salvar
          </Botao>
        </>
      }
    >
      <form
        className="grid gap-4"
        onSubmit={(evento) => {
          evento.preventDefault();
          void salvar();
        }}
      >
        <AvisoErro mensagem={f.erro} />
        <Grupo titulo="Tipo">
          <CamposTipo f={f} somenteLeitura />
        </Grupo>
        <Grupo titulo="Instituição">
          <CamposInstituicao f={f} />
        </Grupo>
        <CamposNome f={f} />
        <CamposSaldo f={f} />
        {/* Permite salvar com Enter em qualquer campo. */}
        <button type="submit" hidden />
      </form>
    </Popup>
  );
}
