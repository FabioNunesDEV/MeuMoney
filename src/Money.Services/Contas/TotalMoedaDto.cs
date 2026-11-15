// <copyright file="TotalMoedaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Contas;

/// <summary>Soma dos saldos das contas abertas de uma moeda.</summary>
/// <param name="Moeda">Moeda somada.</param>
/// <param name="Total">Soma dos saldos com sinal.</param>
public sealed record TotalMoedaDto(MoedaDto Moeda, decimal Total);
