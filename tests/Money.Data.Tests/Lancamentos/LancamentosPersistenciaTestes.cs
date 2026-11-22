// <copyright file="LancamentosPersistenciaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;
using Money.Services.Abstracoes;
using Money.Services.Lancamentos;

namespace Money.Data.Tests.Lancamentos;

/// <summary>Testes de gravação e leitura de lançamentos no SQLite.</summary>
public sealed class LancamentosPersistenciaTestes : IAsyncLifetime, IDisposable
{
    private static readonly DateOnly Dia = new(2026, 11, 22);

    private readonly string pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));
    private readonly Conta carteira = new(TipoConta.Dinheiro, "Carteira", "BRL", 100m);
    private readonly Conta cofre = new(TipoConta.Dinheiro, "Cofre", "BRL", 0m);
    private readonly Favorecido favorecido = new("Padaria");
    private readonly Categoria categoria = new("Alimentação");
    private ServiceProvider? provider;

    private ServiceProvider Provider => this.provider ?? throw new InvalidOperationException("Banco não inicializado.");

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        this.provider = new ServiceCollection()
            .AddMoneySqlite($"Data Source={Path.Combine(this.pasta, "teste.db")}")
            .BuildServiceProvider();
        await this.ExecutarAsync(async s =>
        {
            await s.GetRequiredService<IInicializadorBanco>().InicializarAsync();
            var contas = s.GetRequiredService<IRepositorio<Conta>>();
            contas.Adicionar(this.carteira);
            contas.Adicionar(this.cofre);
            s.GetRequiredService<IRepositorio<Favorecido>>().Adicionar(this.favorecido);
            s.GetRequiredService<IRepositorio<Categoria>>().Adicionar(this.categoria);
            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (this.provider is not null)
        {
            await this.provider.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(this.pasta, recursive: true);
        }
        catch (IOException)
        {
            // Limpeza best-effort de arquivo temporário.
        }
    }

    /// <summary>Gasto e transferência gravados voltam iguais numa leitura nova.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Gravar_GastoETransferencia_LeIgual()
    {
        var subcategoria = new Subcategoria(this.categoria.Id, "Pão");
        var gasto = Lancamento.CriarGasto(this.carteira, this.Dados(12.5m, subcategoria.Id, "café"));
        gasto.MarcarComo(SituacaoLancamento.Compensado);
        var (saida, entrada) = Lancamento.CriarTransferencia(this.carteira, this.cofre, this.Dados(40m));

        await this.ExecutarAsync(async s =>
        {
            s.GetRequiredService<IRepositorio<Subcategoria>>().Adicionar(subcategoria);
            var lancamentos = s.GetRequiredService<ILancamentoRepositorio>();
            lancamentos.Adicionar(gasto);
            lancamentos.Adicionar(saida);
            lancamentos.Adicionar(entrada);
            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });

        await this.ExecutarAsync(async s =>
        {
            var lancamentos = s.GetRequiredService<ILancamentoRepositorio>();
            var lido = await lancamentos.ObterPorIdAsync(gasto.Id, CancellationToken.None);
            Assert.NotNull(lido);
            Assert.Equal(TipoLancamento.Gasto, lido.Tipo);
            Assert.Equal(Dia, lido.Data);
            Assert.Equal(12.5m, lido.Valor);
            Assert.Equal(this.favorecido.Id, lido.FavorecidoId);
            Assert.Equal(this.categoria.Id, lido.CategoriaId);
            Assert.Equal(subcategoria.Id, lido.SubcategoriaId);
            Assert.Equal("café", lido.Memo);
            Assert.Equal(SituacaoLancamento.Compensado, lido.Situacao);

            var pontas = await lancamentos.ListarDaTransferenciaAsync(saida.TransferenciaId!.Value, CancellationToken.None);
            Assert.Equal(2, pontas.Count);
            Assert.Contains(pontas, p => p.ContaId == this.cofre.Id && p.Tipo == TipoLancamento.TransferenciaEntrada);
        });
    }

    /// <summary>
    /// A lista da conta vem em ordem de data e, na mesma data, de Id (Guid v7, que segue o momento do
    /// registro). Só os lançamentos da conta aparecem.
    /// </summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListarDaContaAsync_OrdenaPorDataEId()
    {
        var mesmoDia1 = Lancamento.CriarGasto(this.carteira, this.Dados(2m));
        var mesmoDia2 = Lancamento.CriarGasto(this.carteira, this.Dados(3m));
        var anterior = Lancamento.CriarGasto(this.carteira, this.Dados(1m) with { Data = Dia.AddDays(-1) });
        var deOutraConta = Lancamento.CriarGasto(this.cofre, this.Dados(9m));
        var esperado = new[] { mesmoDia2, anterior, mesmoDia1 }.OrderBy(l => l.Data).ThenBy(l => l.Id).Select(l => l.Id);

        await this.GravarAsync(mesmoDia2, anterior, deOutraConta, mesmoDia1);

        await this.ExecutarAsync(async s =>
        {
            var lista = await s.GetRequiredService<ILancamentoRepositorio>()
                .ListarDaContaAsync(this.carteira.Id, CancellationToken.None);
            Assert.Equal(esperado, lista.Select(l => l.Id));
            Assert.Equal(anterior.Id, lista[0].Id);
        });
    }

    /// <summary>A soma por conta ignora os lançamentos nulos.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task SomarEfeitosPorContaAsync_IgnoraNulos()
    {
        var recebimento = Lancamento.CriarRecebimento(this.carteira, this.Dados(50m));
        var gasto = Lancamento.CriarGasto(this.carteira, this.Dados(30m));
        var anulado = Lancamento.CriarGasto(this.carteira, this.Dados(20m));
        anulado.MarcarComo(SituacaoLancamento.Nulo);
        var (saida, entrada) = Lancamento.CriarTransferencia(this.carteira, this.cofre, this.Dados(5m));

        await this.GravarAsync(recebimento, gasto, anulado, saida, entrada);

        await this.ExecutarAsync(async s =>
        {
            var somas = await s.GetRequiredService<ILancamentoRepositorio>().SomarEfeitosPorContaAsync(CancellationToken.None);
            Assert.Equal(15m, somas[this.carteira.Id]);
            Assert.Equal(5m, somas[this.cofre.Id]);
        });
    }

    /// <summary>As duas pontas de uma transferência saem numa só gravação.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Remover_PontasDaTransferencia_ExcluiAsDuas()
    {
        var (saida, entrada) = Lancamento.CriarTransferencia(this.carteira, this.cofre, this.Dados(40m));
        await this.GravarAsync(saida, entrada);

        await this.ExecutarAsync(async s =>
        {
            var lancamentos = s.GetRequiredService<ILancamentoRepositorio>();
            foreach (var ponta in await lancamentos.ListarDaTransferenciaAsync(saida.TransferenciaId!.Value, CancellationToken.None))
            {
                lancamentos.Remover(ponta);
            }

            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });

        await this.ExecutarAsync(async s =>
            Assert.Empty(await s.GetRequiredService<ILancamentoRepositorio>().ListarAsync(CancellationToken.None)));
    }

    private DadosLancamento Dados(decimal valor, Guid? subcategoriaId = null, string? memo = null) =>
        new(Dia, valor, this.favorecido.Id, this.categoria.Id, subcategoriaId, memo);

    private async Task GravarAsync(params Lancamento[] novos)
    {
        await this.ExecutarAsync(async s =>
        {
            var lancamentos = s.GetRequiredService<ILancamentoRepositorio>();
            foreach (var lancamento in novos)
            {
                lancamentos.Adicionar(lancamento);
            }

            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });
    }

    private async Task ExecutarAsync(Func<IServiceProvider, Task> acao)
    {
        await using var escopo = this.Provider.CreateAsyncScope();
        await acao(escopo.ServiceProvider);
    }
}
