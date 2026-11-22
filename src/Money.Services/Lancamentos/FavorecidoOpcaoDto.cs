// <copyright file="FavorecidoOpcaoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Favorecido oferecido no formulário, com o que foi usado no lançamento mais recente dele.</summary>
/// <param name="Nome">Nome do favorecido.</param>
/// <param name="UltimaCategoria">Categoria do lançamento mais recente, se houver.</param>
/// <param name="UltimaSubcategoria">Subcategoria do lançamento mais recente, se houver.</param>
/// <param name="UltimoValor">Montante do lançamento mais recente, se houver.</param>
public sealed record FavorecidoOpcaoDto(string Nome, string? UltimaCategoria, string? UltimaSubcategoria, decimal? UltimoValor);
