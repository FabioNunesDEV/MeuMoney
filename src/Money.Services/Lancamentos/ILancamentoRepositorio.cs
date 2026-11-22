// <copyright file="ILancamentoRepositorio.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Lancamentos;
using Money.Services.Abstracoes;

namespace Money.Services.Lancamentos;

/// <summary>Acesso aos lançamentos, com as consultas por conta e por transferência.</summary>
public interface ILancamentoRepositorio : IRepositorio<Lancamento>
{
    /// <summary>Lista os lançamentos de uma conta em ordem de data e, na mesma data, de registro.</summary>
    /// <param name="contaId">Id da conta.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Os lançamentos da conta, ordenados.</returns>
    Task<IReadOnlyList<Lancamento>> ListarDaContaAsync(Guid contaId, CancellationToken cancellationToken);

    /// <summary>Soma o efeito no saldo dos lançamentos de cada conta (nulos valem zero).</summary>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>A soma por Id de conta; conta sem lançamento não aparece.</returns>
    Task<IReadOnlyDictionary<Guid, decimal>> SomarEfeitosPorContaAsync(CancellationToken cancellationToken);

    /// <summary>Lista as duas pontas de uma transferência.</summary>
    /// <param name="transferenciaId">Id que liga as pontas.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>As pontas encontradas.</returns>
    Task<IReadOnlyList<Lancamento>> ListarDaTransferenciaAsync(Guid transferenciaId, CancellationToken cancellationToken);
}
