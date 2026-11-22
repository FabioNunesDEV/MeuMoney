// <copyright file="MovimentacaoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>A movimentação de uma conta: as linhas em ordem, com o saldo de cada uma.</summary>
/// <param name="ContaId">Id da conta.</param>
/// <param name="SaldoInicial">Saldo inicial da conta, de onde parte o saldo linha a linha.</param>
/// <param name="Lancamentos">Lançamentos em ordem de data e registro.</param>
/// <param name="SaldoFinal">Saldo depois do último lançamento.</param>
public sealed record MovimentacaoDto(
    Guid ContaId,
    decimal SaldoInicial,
    IReadOnlyList<LancamentoDto> Lancamentos,
    decimal SaldoFinal);
