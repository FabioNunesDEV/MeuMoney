// <copyright file="ContasPersistenciaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Domain.Contas;
using Money.Services.Abstracoes;

namespace Money.Data.Tests.Contas;

/// <summary>Testes de gravação e leitura de contas, instituições e grupos no SQLite.</summary>
public sealed class ContasPersistenciaTestes : IAsyncLifetime, IDisposable
{
    private readonly string pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));
    private ServiceProvider? provider;

    private ServiceProvider Provider => this.provider ?? throw new InvalidOperationException("Banco não inicializado.");

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        this.provider = new ServiceCollection()
            .AddMoneySqlite($"Data Source={Path.Combine(this.pasta, "teste.db")}")
            .BuildServiceProvider();
        await this.ExecutarAsync(s => s.GetRequiredService<IInicializadorBanco>().InicializarAsync());
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

    /// <summary>Conta gravada com instituição, grupo e caixa volta igual numa leitura nova.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Gravar_ContaComVinculos_LeIgual()
    {
        var instituicao = new InstituicaoFinanceira("Corretora X");
        var grupo = new GrupoConta("Exterior");
        var caixa = new Conta(TipoConta.Dinheiro, "Caixa", "USD", 12.34m);
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "USD", 0m);
        investimento.DefinirInstituicao(instituicao.Id);
        investimento.DefinirGrupo(grupo.Id);
        investimento.DefinirFavorita(true);
        investimento.VincularCaixa(caixa);
        var cartao = new Conta(TipoConta.CartaoCredito, "Cartão", "BRL", 500m);

        await this.ExecutarAsync(async s =>
        {
            s.GetRequiredService<IRepositorio<InstituicaoFinanceira>>().Adicionar(instituicao);
            s.GetRequiredService<IRepositorio<GrupoConta>>().Adicionar(grupo);
            var contas = s.GetRequiredService<IRepositorio<Conta>>();
            contas.Adicionar(caixa);
            contas.Adicionar(investimento);
            contas.Adicionar(cartao);
            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });

        await this.ExecutarAsync(async s =>
        {
            var contas = s.GetRequiredService<IRepositorio<Conta>>();
            var lido = await contas.ObterPorIdAsync(investimento.Id, CancellationToken.None);
            Assert.NotNull(lido);
            Assert.Equal(TipoConta.Investimento, lido.Tipo);
            Assert.Equal("USD", lido.CodigoMoeda);
            Assert.Equal(instituicao.Id, lido.InstituicaoFinanceiraId);
            Assert.Equal(grupo.Id, lido.GrupoContaId);
            Assert.Equal(caixa.Id, lido.ContaCaixaId);
            Assert.True(lido.Favorita);
            Assert.Equal(12.34m, (await contas.ObterPorIdAsync(caixa.Id, CancellationToken.None))?.SaldoInicial);
            Assert.Equal(-500m, (await contas.ObterPorIdAsync(cartao.Id, CancellationToken.None))?.SaldoInicial);
        });
    }

    /// <summary>Excluir o grupo no banco deixa a conta sem grupo.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Excluir_GrupoComContas_SoltaContas()
    {
        var grupo = new GrupoConta("Exterior");
        var conta = new Conta(TipoConta.Corrente, "Conta", "BRL", 0m);
        conta.DefinirGrupo(grupo.Id);
        await this.GravarAsync(grupo, conta);

        await this.ExecutarAsync(async s =>
        {
            var grupos = s.GetRequiredService<IRepositorio<GrupoConta>>();
            var lido = await grupos.ObterPorIdAsync(grupo.Id, CancellationToken.None);
            Assert.NotNull(lido);
            grupos.Remover(lido);
            await s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync();
        });

        await this.ExecutarAsync(async s =>
        {
            var lida = await s.GetRequiredService<IRepositorio<Conta>>().ObterPorIdAsync(conta.Id, CancellationToken.None);
            Assert.Null(lida?.GrupoContaId);
        });
    }

    /// <summary>O banco recusa excluir uma conta caixa referenciada por investimento.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Excluir_CaixaReferenciada_Falha()
    {
        var caixa = new Conta(TipoConta.Dinheiro, "Caixa", "BRL", 0m);
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "BRL", 0m);
        investimento.VincularCaixa(caixa);
        await this.GravarAsync(null, caixa, investimento);

        await this.ExecutarAsync(async s =>
        {
            var contas = s.GetRequiredService<IRepositorio<Conta>>();
            var lida = await contas.ObterPorIdAsync(caixa.Id, CancellationToken.None);
            Assert.NotNull(lida);
            contas.Remover(lida);
            await Assert.ThrowsAsync<DbUpdateException>(
                () => s.GetRequiredService<IUnidadeDeTrabalho>().SalvarAlteracoesAsync());
        });
    }

    private async Task GravarAsync(GrupoConta? grupo, params Conta[] contas)
    {
        await this.ExecutarAsync(async s =>
        {
            if (grupo is not null)
            {
                s.GetRequiredService<IRepositorio<GrupoConta>>().Adicionar(grupo);
            }

            var repositorio = s.GetRequiredService<IRepositorio<Conta>>();
            foreach (var conta in contas)
            {
                repositorio.Adicionar(conta);
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
