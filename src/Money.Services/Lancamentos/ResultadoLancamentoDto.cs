// <copyright file="ResultadoLancamentoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>
/// Resultado de incluir ou alterar: gravado, ou à espera de confirmação porque alguma conta ficaria
/// negativa.
/// </summary>
/// <param name="Gravado">Indica se o lançamento foi gravado.</param>
/// <param name="SaldosNegativos">Contas que ficariam negativas; vazia quando gravado.</param>
public sealed record ResultadoLancamentoDto(bool Gravado, IReadOnlyList<SaldoNegativoDto> SaldosNegativos);
