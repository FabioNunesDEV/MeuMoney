// <copyright file="PontoSaldoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Um ponto do gráfico do saldo: o saldo da conta ao fim do dia.</summary>
/// <param name="Data">Dia do ponto.</param>
/// <param name="Saldo">Saldo ao fim do dia (no primeiro ponto, no início do período).</param>
public sealed record PontoSaldoDto(DateOnly Data, decimal Saldo);
