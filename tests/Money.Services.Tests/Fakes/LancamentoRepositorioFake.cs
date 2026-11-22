// <copyright file="LancamentoRepositorioFake.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Lancamentos;
using Money.Services.Lancamentos;

namespace Money.Services.Tests.Fakes;

/// <summary>Repositório de lançamentos em memória para testes de serviço.</summary>
public sealed class LancamentoRepositorioFake : ILancamentoRepositorio
{
    /// <summary>Obtém os lançamentos guardados.</summary>
    public List<Lancamento> Itens { get; } = [];

    /// <inheritdoc />
    public Task<IReadOnlyList<Lancamento>> ListarAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Lancamento>>(this.Itens.ToList());
    }

    /// <inheritdoc />
    public Task<Lancamento?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Itens.FirstOrDefault(l => l.Id == id));
    }

    /// <inheritdoc />
    public void Adicionar(Lancamento entidade)
    {
        this.Itens.Add(entidade);
    }

    /// <inheritdoc />
    public void Remover(Lancamento entidade)
    {
        this.Itens.Remove(entidade);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Lancamento>> ListarDaContaAsync(Guid contaId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Lancamento>>(
            this.Itens.Where(l => l.ContaId == contaId).OrderBy(l => l.Data).ThenBy(l => l.Id).ToList());
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, decimal>> SomarEfeitosPorContaAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyDictionary<Guid, decimal>>(
            this.Itens.GroupBy(l => l.ContaId).ToDictionary(g => g.Key, g => g.Sum(l => l.EfeitoNoSaldo)));
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Lancamento>> ListarDaTransferenciaAsync(Guid transferenciaId, CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<Lancamento>>(
            this.Itens.Where(l => l.TransferenciaId == transferenciaId).ToList());
    }
}
