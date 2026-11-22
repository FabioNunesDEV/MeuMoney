// <copyright file="MoverLancamentoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Conta para onde mover o lançamento.</summary>
/// <param name="ContaId">Id da conta de destino.</param>
public sealed record MoverLancamentoDto(Guid ContaId);
