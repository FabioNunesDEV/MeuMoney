import { RotateCcw, Save } from 'lucide-react';
import { type ReactNode, useEffect, useRef } from 'react';
import { toast } from 'sonner';
import type { Conta } from '../api';
import { SeloTipo } from '../comum/tiposConta';
import { AvisoErro, Botao, Secao, ValorSaldo } from '../comum/ui';
import { CamposComplementares, CamposInstituicao, CamposNome, CamposSaldo } from './CamposConta';
import { useFormularioConta } from './useFormularioConta';

function Bloco({ titulo, children }: { titulo: string; children: ReactNode }) {
  return (
    <Secao titulo={titulo}>
      <div className="p-3">{children}</div>
    </Secao>
  );
}

/**
 * Visão Detalhes da página da conta: o cadastro como formulário, como no "Detalhes da conta" do Money.
 * Tipo, situação e saldo ficam só para leitura. `focarNome` (pedido pelo Editar) leva o foco ao Nome.
 * Para voltar aos valores gravados, a página remonta o componente (nova `key`).
 */
export function DetalhesConta(props: {
  conta: Conta;
  focarNome: boolean;
  aoFocar: () => void;
  aoSalvar: () => void;
  aoDesfazer: () => void;
}) {
  const { conta, focarNome, aoFocar } = props;
  const f = useFormularioConta(conta);
  const formulario = useRef<HTMLFormElement>(null);
  const aviso = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (focarNome) {
      // O Nome é o primeiro campo do formulário.
      formulario.current?.querySelector('input')?.focus();
      aoFocar();
    }
  }, [focarNome, aoFocar]);

  // O formulário é longo: a recusa pode aparecer fora da parte visível.
  useEffect(() => {
    if (f.erro) {
      aviso.current?.scrollIntoView({ block: 'nearest' });
    }
  }, [f.erro]);

  async function salvar() {
    const salva = await f.salvar();
    if (salva) {
      toast.success(`"${salva.nome}" foi alterada.`);
      props.aoSalvar();
    }
  }

  return (
    <div className="max-w-[760px]">
      <div className="mb-4 flex items-center gap-3">
        <SeloTipo tipo={conta.tipo} tamanho="enorme" />
        <div>
          <p className="text-suave">
            {conta.nomeTipo} · {conta.encerrada ? 'Encerrada' : 'Aberta'}
          </p>
          <ValorSaldo valor={conta.saldo} moeda={conta.moeda} className="text-2xl" />
        </div>
      </div>

      <form
        ref={formulario}
        aria-label="Detalhes da conta"
        onSubmit={(evento) => {
          evento.preventDefault();
          if (f.alterado) {
            void salvar();
          }
        }}
      >
        <Bloco titulo="Conta">
          <CamposNome f={f} />
        </Bloco>
        <Bloco titulo="Instituição">
          <CamposInstituicao f={f} />
        </Bloco>
        <Bloco titulo={conta.tipo === 'Investimento' ? 'Moeda e conta caixa' : 'Saldo e moeda'}>
          <CamposSaldo f={f} />
        </Bloco>
        <Bloco titulo="Complementos">
          <CamposComplementares f={f} />
        </Bloco>

        {/* Junto dos botões: é ali que está a atenção de quem acabou de clicar em Salvar. */}
        <div ref={aviso}>
          <AvisoErro mensagem={f.erro} />
          <div className="flex justify-end gap-2">
            <Botao icone={RotateCcw} disabled={!f.alterado || f.salvando} onClick={props.aoDesfazer}>
              Desfazer
            </Botao>
            <Botao type="submit" variante="primario" icone={Save} disabled={!f.alterado || f.salvando}>
              Salvar
            </Botao>
          </div>
        </div>
      </form>
    </div>
  );
}
