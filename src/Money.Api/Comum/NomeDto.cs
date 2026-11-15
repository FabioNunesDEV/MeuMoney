// <copyright file="NomeDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Api.Comum;

/// <summary>Corpo do pedido de renomear um cadastro.</summary>
/// <param name="Nome">Novo nome.</param>
public sealed record NomeDto(string Nome);
