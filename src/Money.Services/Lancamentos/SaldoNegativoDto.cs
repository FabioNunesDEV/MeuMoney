// <copyright file="SaldoNegativoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Services.Contas;

namespace Money.Services.Lancamentos;

/// <summary>Conta que ficaria com saldo negativo se o lançamento fosse gravado.</summary>
/// <param name="ContaId">Id da conta.</param>
/// <param name="NomeConta">Nome da conta.</param>
/// <param name="SaldoFinal">Saldo que a conta teria.</param>
/// <param name="Moeda">Moeda da conta.</param>
public sealed record SaldoNegativoDto(Guid ContaId, string NomeConta, decimal SaldoFinal, MoedaDto Moeda);
