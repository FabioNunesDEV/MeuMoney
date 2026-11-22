import { ChartLine, ListOrdered } from 'lucide-react';
import { useEffect, useState } from 'react';
import { CartesianGrid, Line, LineChart, ReferenceLine, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { api, type Conta, type Moeda } from '../api';
import { classes, dataParaTexto, hojeIso } from '../comum/formato';
import { Botao, estiloEntradaBase, Secao, ValorSaldo } from '../comum/ui';
import { useDados } from '../comum/useDados';

type Periodo = 1 | 3 | 6 | 12 | 'tudo';

const periodos: { valor: Periodo; rotulo: string }[] = [
  { valor: 1, rotulo: 'Último mês' },
  { valor: 3, rotulo: 'Últimos 3 meses' },
  { valor: 6, rotulo: 'Últimos 6 meses' },
  { valor: 12, rotulo: 'Últimos 12 meses' },
  { valor: 'tudo', rotulo: 'Desde o primeiro lançamento' },
];

const umDia = 86_400_000;

// O eixo de datas é numérico (dias), para que o espaço entre os pontos seja proporcional ao tempo.
function paraDia(iso: string): number {
  const [ano, mes, dia] = iso.split('-').map(Number);
  return Date.UTC(ano, mes - 1, dia) / umDia;
}

function diaParaIso(dia: number): string {
  return new Date(Math.round(dia) * umDia).toISOString().slice(0, 10);
}

function mesesAtras(iso: string, meses: number): string {
  const [ano, mes, dia] = iso.split('-').map(Number);
  const data = new Date(Date.UTC(ano, mes - 1 - meses, dia));
  return data.toISOString().slice(0, 10);
}

// Rótulo curto do eixo de saldo, sem centavos e no formato contábil: "R$ 1.500", "(R$ 200)".
function saldoCurto(valor: number, moeda: Moeda): string {
  const texto = `${moeda.simbolo} ${Math.round(Math.abs(valor)).toLocaleString('pt-BR')}`;
  return valor < 0 ? `(${texto})` : texto;
}

// Marcas do eixo de saldo em valores redondos (passo 1, 2, 2,5 ou 5 vezes uma potência de 10), sempre
// incluindo o zero. O padrão do recharts divide o intervalo em partes iguais e gera marcas como 55 e 110.
// O passo é inteiro porque o rótulo do eixo não mostra centavos.
function marcasRedondas(menor: number, maior: number): number[] {
  const inferior = Math.min(0, menor);
  const superior = Math.max(0, maior);
  const bruto = (superior - inferior) / 5 || 1;
  const potencia = 10 ** Math.floor(Math.log10(bruto));
  const passo = [1, 2, 2.5, 5, 10].map((m) => m * potencia).find((p) => p >= bruto && Number.isInteger(p)) ?? 1;
  const marcas: number[] = [];
  for (let i = Math.floor(inferior / passo); i <= Math.ceil(superior / passo); i++) {
    marcas.push(i * passo);
  }
  return marcas;
}

/** Cores do tema atual, lidas das variáveis do `estilo.css` e relidas a cada troca de tema. */
function useCores() {
  const ler = () => {
    const estilo = getComputedStyle(document.documentElement);
    const cor = (nome: string) => estilo.getPropertyValue(nome).trim();
    return { linha: cor('--realce'), grade: cor('--borda'), texto: cor('--suave'), zero: cor('--negativo') };
  };
  const [cores, setCores] = useState(ler);

  useEffect(() => {
    const observador = new MutationObserver(() => setCores(ler()));
    observador.observe(document.documentElement, { attributes: true, attributeFilter: ['data-tema'] });
    return () => observador.disconnect();
  }, []);

  return cores;
}

function Resumo({ rotulo, valor, moeda }: { rotulo: string; valor: number; moeda: Moeda }) {
  return (
    <div className="rounded-lg border border-borda bg-painel px-3 py-2">
      <p className="text-[11.5px] text-suave">{rotulo}</p>
      <ValorSaldo valor={valor} moeda={moeda} className="text-base" />
    </div>
  );
}

/**
 * Histórico da conta, como o "Movimentação do saldo" do Money: o saldo no tempo em degraus, com o
 * período escolhido, o resumo e a dica de cada ponto. Carregado sob demanda, por causa do `recharts`.
 */
export function Historico({ conta, irParaMovimentacao }: { conta: Conta; irParaMovimentacao: () => void }) {
  const [periodo, setPeriodo] = useState<Periodo>(3);
  const hoje = hojeIso();
  const de = periodo === 'tudo' ? null : mesesAtras(hoje, periodo);
  const [historico] = useDados(() => api.historico(conta.id, de, hoje), [conta.id, de, hoje]);
  const cores = useCores();

  if (!historico) {
    return <p className="text-suave">Carregando…</p>;
  }

  if (!historico.temLancamentos) {
    return (
      <div className="mx-auto mt-16 grid max-w-sm justify-items-center gap-2 text-center">
        <span className="grid size-12 place-items-center rounded-full bg-painel text-fraco">
          <ChartLine className="size-6" aria-hidden />
        </span>
        <p className="font-semibold">Ainda não há histórico</p>
        <p className="text-suave">O gráfico do saldo aparece depois do primeiro lançamento da conta.</p>
        <Botao variante="primario" icone={ListOrdered} className="mt-2" onClick={irParaMovimentacao}>
          Ir para a Movimentação
        </Botao>
      </div>
    );
  }

  const dados = historico.pontos.map((p) => ({ dia: paraDia(p.data), saldo: p.saldo }));
  const marcas = marcasRedondas(historico.menor, historico.maior);

  return (
    <div className="grid gap-4">
      <div className="flex flex-wrap items-end gap-3">
        <div className="mr-auto">
          <h2 className="text-base font-semibold">Movimentação do saldo</h2>
          <p className="text-suave">
            {dataParaTexto(historico.de)} até {dataParaTexto(historico.ate)}
          </p>
        </div>
        <label className="flex items-center gap-2">
          <span className="text-suave">Período</span>
          <select
            className={classes(estiloEntradaBase, 'bg-painel')}
            aria-label="Período"
            value={String(periodo)}
            onChange={(e) => setPeriodo(e.target.value === 'tudo' ? 'tudo' : (Number(e.target.value) as Periodo))}
          >
            {periodos.map((p) => (
              <option key={p.rotulo} value={String(p.valor)}>
                {p.rotulo}
              </option>
            ))}
          </select>
        </label>
      </div>

      <div className="grid grid-cols-4 gap-3 max-[820px]:grid-cols-2">
        <Resumo rotulo="Início do período" valor={historico.saldoInicioPeriodo} moeda={conta.moeda} />
        <Resumo rotulo="Maior saldo" valor={historico.maior} moeda={conta.moeda} />
        <Resumo rotulo="Menor saldo" valor={historico.menor} moeda={conta.moeda} />
        <Resumo rotulo="Saldo final" valor={historico.final} moeda={conta.moeda} />
      </div>

      <Secao titulo="Saldo no tempo" detalhe={conta.moeda.nome}>
        <div className="h-[340px] p-3" role="img" aria-label={`Gráfico do saldo de ${conta.nome} no período`}>
          <ResponsiveContainer width="100%" height="100%">
            <LineChart data={dados} margin={{ top: 8, right: 16, bottom: 4, left: 8 }}>
              <CartesianGrid stroke={cores.grade} vertical={false} />
              <XAxis
                dataKey="dia"
                type="number"
                domain={['dataMin', 'dataMax']}
                allowDecimals={false}
                tickCount={8}
                tickFormatter={(dia: number) => dataParaTexto(diaParaIso(dia)).slice(0, 5)}
                stroke={cores.texto}
                tick={{ fill: cores.texto, fontSize: 11 }}
              />
              <YAxis
                width={84}
                // Sem folga, o trecho no menor saldo cai em cima da linha do eixo e some.
                padding={{ bottom: 8 }}
                domain={[marcas[0], marcas[marcas.length - 1]]}
                ticks={marcas}
                tickFormatter={(valor: number) => saldoCurto(valor, conta.moeda)}
                stroke={cores.texto}
                tick={{ fill: cores.texto, fontSize: 11 }}
              />
              {historico.menor < 0 && <ReferenceLine y={0} stroke={cores.zero} strokeDasharray="4 4" />}
              <Tooltip
                cursor={{ stroke: cores.grade }}
                content={({ active, payload }) => {
                  const ponto = payload?.[0]?.payload as { dia: number; saldo: number } | undefined;
                  if (!active || !ponto) {
                    return null;
                  }

                  return (
                    <div className="rounded-md border border-borda bg-painel px-2.5 py-1.5 shadow-lg">
                      <p className="text-[11.5px] text-suave">{dataParaTexto(diaParaIso(ponto.dia))}</p>
                      <ValorSaldo valor={ponto.saldo} moeda={conta.moeda} />
                    </div>
                  );
                }}
              />
              <Line type="stepAfter" dataKey="saldo" stroke={cores.linha} strokeWidth={2} dot={false} isAnimationActive={false} />
            </LineChart>
          </ResponsiveContainer>
        </div>
      </Secao>
    </div>
  );
}
