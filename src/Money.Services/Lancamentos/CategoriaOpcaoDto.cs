// <copyright file="CategoriaOpcaoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Categoria oferecida no formulário, com as suas subcategorias.</summary>
/// <param name="Nome">Nome da categoria.</param>
/// <param name="Subcategorias">Nomes das subcategorias, em ordem alfabética.</param>
public sealed record CategoriaOpcaoDto(string Nome, IReadOnlyList<string> Subcategorias);
