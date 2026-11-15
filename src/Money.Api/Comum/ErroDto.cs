// <copyright file="ErroDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Api.Comum;

/// <summary>Corpo da resposta de erro de regra de negócio.</summary>
/// <param name="Mensagem">Mensagem em pt-BR, pronta para exibir.</param>
public sealed record ErroDto(string Mensagem);
