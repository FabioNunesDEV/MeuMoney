// <copyright file="LancamentoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Money.Data.Comum;
using Money.Domain.Lancamentos;
using Money.Services.Lancamentos;

namespace Money.Data.Lancamentos;

/// <summary>Repositório de lançamentos sobre o <see cref="MoneyDbContext"/>.</summary>
/// <param name="dbContext">Contexto do escopo atual.</param>
internal sealed class LancamentoRepositorio(MoneyDbContext dbContext) : Repositorio<Lancamento>(dbContext), ILancamentoRepositorio
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Lancamento>> ListarDaContaAsync(Guid contaId, CancellationToken cancellationToken)
    {
        return await this.DbContext.Set<Lancamento>()
            .Where(l => l.ContaId == contaId)
            .OrderBy(l => l.Data)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, decimal>> SomarEfeitosPorContaAsync(CancellationToken cancellationToken)
    {
        // O SQLite não soma decimal: a soma é feita em memória, sobre a leitura sem rastreamento.
        var lancamentos = await this.DbContext.Set<Lancamento>().AsNoTracking().ToListAsync(cancellationToken);
        return lancamentos.GroupBy(l => l.ContaId).ToDictionary(g => g.Key, g => g.Sum(l => l.EfeitoNoSaldo));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Lancamento>> ListarDaTransferenciaAsync(Guid transferenciaId, CancellationToken cancellationToken)
    {
        return await this.DbContext.Set<Lancamento>()
            .Where(l => l.TransferenciaId == transferenciaId)
            .ToListAsync(cancellationToken);
    }
}
