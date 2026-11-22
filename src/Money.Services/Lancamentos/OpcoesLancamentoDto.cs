// <copyright file="OpcoesLancamentoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Listas que o formulário de lançamento oferece.</summary>
/// <param name="Favorecidos">Favorecidos já usados, com a sugestão do último uso.</param>
/// <param name="Categorias">Categorias com as subcategorias.</param>
/// <param name="Contas">Contas compatíveis com a conta: Dinheiro, abertas e da mesma moeda (ela inclusive).</param>
public sealed record OpcoesLancamentoDto(
    IReadOnlyList<FavorecidoOpcaoDto> Favorecidos,
    IReadOnlyList<CategoriaOpcaoDto> Categorias,
    IReadOnlyList<ContaOpcaoDto> Contas);
