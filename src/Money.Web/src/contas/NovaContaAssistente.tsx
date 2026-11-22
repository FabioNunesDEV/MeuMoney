import { type ReactNode, useState } from 'react';
import type { Conta } from '../api';
import { classes } from '../comum/formato';
import { AvisoErro, Botao, Popup } from '../comum/ui';
import { CamposInstituicao, CamposNome, CamposSaldo, CamposTipo } from './CamposConta';
import { type FormularioConta, useFormularioConta } from './useFormularioConta';

/** Um passo do assistente: a pergunta, os campos e o que impede avançar (só preenchimento; o resto é do servidor). */
interface Passo {
  pergunta: (f: FormularioConta) => string;
  campos: (f: FormularioConta) => ReactNode;
  pendencia?: (f: FormularioConta) => string | null;
}

const passos: Passo[] = [
  {
    pergunta: () => 'Onde esta conta é mantida?',
    campos: (f) => <CamposInstituicao f={f} focar />,
    pendencia: (f) =>
      f.mantida && !f.instituicao.trim() ? 'Informe a instituição ou marque que a conta não é mantida em instituição.' : null,
  },
  {
    pergunta: () => 'Que tipo de conta você quer criar?',
    campos: (f) => <CamposTipo f={f} focar />,
  },
  {
    pergunta: () => 'Que nome deseja dar a esta conta?',
    campos: (f) => <CamposNome f={f} focar />,
    pendencia: (f) => (f.nome.trim() ? null : 'Informe o nome da conta.'),
  },
  {
    pergunta: (f) => (f.tipo === 'Investimento' ? 'Qual é a moeda e a conta caixa?' : 'Qual é o saldo desta conta?'),
    campos: (f) => (
      <>
        {f.tipo !== 'Investimento' && (
          <p className="text-suave">
            Informe o saldo de hoje ou o do último extrato, se for registrar os lançamentos a partir dele. Se não souber, deixe
            zero e ajuste depois.
          </p>
        )}
        <CamposSaldo f={f} focar />
      </>
    ),
  },
];

/** Assistente de nova conta, em passos curtos como no Money: instituição, tipo, nome, saldo e moeda. */
export function NovaContaAssistente(props: { aoFechar: () => void; aoSalvar: (conta: Conta) => void }) {
  const f = useFormularioConta(null);
  const [indice, setIndice] = useState(0);
  const passo = passos[indice];
  const ultimo = indice === passos.length - 1;

  function irPara(novo: number) {
    f.setErro(null);
    setIndice(novo);
  }

  async function avancar() {
    const pendencia = passo.pendencia?.(f) ?? null;
    if (pendencia) {
      f.setErro(pendencia);
      return;
    }

    if (!ultimo) {
      irPara(indice + 1);
      return;
    }

    const salva = await f.salvar();
    if (salva) {
      props.aoSalvar(salva);
    }
  }

  return (
    <Popup
      titulo="Nova conta"
      largura="max-w-[560px]"
      aoFechar={props.aoFechar}
      rodape={
        <>
          <Botao className="mr-auto" onClick={props.aoFechar}>
            Cancelar
          </Botao>
          <Botao disabled={indice === 0} onClick={() => irPara(indice - 1)}>
            Voltar
          </Botao>
          <Botao variante="primario" disabled={f.salvando} onClick={avancar}>
            {ultimo ? 'Concluir' : 'Avançar'}
          </Botao>
        </>
      }
    >
      <form
        onSubmit={(evento) => {
          evento.preventDefault();
          void avancar();
        }}
      >
        <div className="mb-3 flex items-center gap-3">
          <div className="flex flex-1 gap-1" aria-hidden>
            {passos.map((_, i) => (
              <span key={i} className={classes('h-1 flex-1 rounded-full', i <= indice ? 'bg-realce' : 'bg-borda')} />
            ))}
          </div>
          <span className="text-[11.5px] text-suave">
            Passo {indice + 1} de {passos.length}
          </span>
        </div>
        {/* A chave remonta o passo, para o foco ir ao primeiro campo dele. */}
        {/* Altura do passo mais alto (a lista de tipos), para o pop-up não mudar de tamanho entre passos. */}
        <fieldset key={indice} className="grid min-h-[310px] content-start gap-3">
          <legend className="mb-3 text-[15px] font-semibold">{passo.pergunta(f)}</legend>
          <AvisoErro mensagem={f.erro} />
          {passo.campos(f)}
        </fieldset>
        {/* Enter em qualquer campo avança ou conclui. */}
        <button type="submit" hidden />
      </form>
    </Popup>
  );
}
