// <copyright file="HistoricoSaldoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>O saldo da conta no tempo, para o gráfico em degrau do Histórico.</summary>
/// <param name="ContaId">Id da conta.</param>
/// <param name="De">Primeiro dia do período.</param>
/// <param name="Ate">Último dia do período (estendido até o último lançamento futuro).</param>
/// <param name="Pontos">Saldo no início e ao fim de cada dia com lançamento, em ordem de data.</param>
/// <param name="SaldoInicioPeriodo">Saldo no início do primeiro dia.</param>
/// <param name="Maior">Maior saldo do período.</param>
/// <param name="Menor">Menor saldo do período.</param>
/// <param name="Final">Saldo no fim do período.</param>
/// <param name="TemLancamentos">Indica se a conta tem algum lançamento; sem eles, não há gráfico.</param>
public sealed record HistoricoSaldoDto(
    Guid ContaId,
    DateOnly De,
    DateOnly Ate,
    IReadOnlyList<PontoSaldoDto> Pontos,
    decimal SaldoInicioPeriodo,
    decimal Maior,
    decimal Menor,
    decimal Final,
    bool TemLancamentos);
