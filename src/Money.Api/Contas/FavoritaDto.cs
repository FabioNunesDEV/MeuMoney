// <copyright file="FavoritaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Api.Contas;

/// <summary>Corpo do pedido de marcar ou desmarcar uma conta como favorita.</summary>
/// <param name="Favorita">Novo estado.</param>
public sealed record FavoritaDto(bool Favorita);
