// <copyright file="SituacaoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Lancamentos;

namespace Money.Services.Lancamentos;

/// <summary>Situação nova de um lançamento.</summary>
/// <param name="Situacao">Situação escolhida em "Marcar como".</param>
public sealed record SituacaoDto(SituacaoLancamento Situacao);
