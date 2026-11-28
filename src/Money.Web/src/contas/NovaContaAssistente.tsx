import { type ReactNode, useState } from 'react';
import { api, type Conta, ErroRegra } from '../api';
import { classes } from '../comum/formato';
import { AvisoErro, Botao, Popup } from '../comum/ui';
import { CampoNumeroConta, CampoSaldoMinimo, CamposInstituicao, CamposNome, CamposSaldo, CamposTipo } from './CamposConta';
import { type FormularioConta, useFormularioConta } from './useFormularioConta';

/**
 * Um passo do assistente: a pergunta, os campos, se ele vale para o tipo escolhido, o que impede avançar
 * (só preenchimento) e a conferência feita no servidor antes de avançar.
 */
interface Passo {
  pergunta: (f: FormularioConta) => string;
  campos: (f: FormularioConta) => ReactNode;
  aplica?: (f: FormularioConta) => boolean;
  pendencia?: (f: FormularioConta) => string | null;
  conferencia?: (f: FormularioConta) => Promise<void>;
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
    conferencia: (f) => api.conferirNomeConta(f.nome),
  },
  {
    pergunta: () => 'Qual é o número desta conta?',
    aplica: (f) => f.infoTipo?.pedeNumeroNaCriacao ?? false,
    campos: (f) => (
      <>
        <p className="text-suave">Opcional: ajuda a identificar a conta, como agência e número. Pode ficar em branco.</p>
        <CampoNumeroConta f={f} focar />
      </>
    ),
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
        {f.infoTipo?.temSaldoMinimo && (
          <div className="grid grid-cols-2 items-end gap-3">
            <CampoSaldoMinimo f={f} />
            <p className="text-[11.5px] text-suave">Opcional. Para o limite do cheque especial, informe o valor negativo.</p>
          </div>
        )}
      </>
    ),
  },
];

/** Assistente de nova conta, em passos curtos como no Money: instituição, tipo, nome, número (se o tipo pede), saldo e moeda. */
export function NovaContaAssistente(props: { aoFechar: () => void; aoSalvar: (conta: Conta) => void }) {
  const f = useFormularioConta(null);
  const [indice, setIndice] = useState(0);
  const [conferindo, setConferindo] = useState(false);
  // O tipo só se escolhe no passo 2, então a lista muda antes de o índice chegar aos passos que somem.
  const visiveis = passos.filter((p) => p.aplica?.(f) ?? true);
  const passo = visiveis[indice];
  const ultimo = indice === visiveis.length - 1;

  function irPara(novo: number) {
    f.setErro(null);
    setIndice(novo);
  }

  async function conferir(): Promise<boolean> {
    if (!passo.conferencia) {
      return true;
    }

    setConferindo(true);
    try {
      await passo.conferencia(f);
      return true;
    } catch (falha) {
      if (!(falha instanceof ErroRegra)) {
        throw falha;
      }

      f.setErro(falha.message);
      return false;
    } finally {
      setConferindo(false);
    }
  }

  async function avancar() {
    const pendencia = passo.pendencia?.(f) ?? null;
    if (pendencia) {
      f.setErro(pendencia);
      return;
    }

    if (!(await conferir())) {
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
          <Botao variante="primario" disabled={f.salvando || conferindo} onClick={avancar}>
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
            {visiveis.map((_, i) => (
              <span key={i} className={classes('h-1 flex-1 rounded-full', i <= indice ? 'bg-realce' : 'bg-borda')} />
            ))}
          </div>
          <span className="text-[11.5px] text-suave">
            Passo {indice + 1} de {visiveis.length}
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
