// <copyright file="OpcoesContaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Services.Contas;

namespace Money.Api.Contas;

/// <summary>Listas fixas que o formulário de conta oferece.</summary>
/// <param name="Tipos">Tipos de conta com descrição.</param>
/// <param name="Moedas">Moedas aceitas.</param>
public sealed record OpcoesContaDto(IReadOnlyList<TipoContaDto> Tipos, IReadOnlyList<MoedaDto> Moedas);
