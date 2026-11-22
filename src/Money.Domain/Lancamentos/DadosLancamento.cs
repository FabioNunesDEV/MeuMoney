// <copyright file="DadosLancamento.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Lancamentos;

/// <summary>Dados digitados num lançamento, com favorecido e categoria já resolvidos para Id.</summary>
/// <param name="Data">Data do lançamento.</param>
/// <param name="Valor">Montante, sempre positivo.</param>
/// <param name="FavorecidoId">Id do favorecido ("Pagar a").</param>
/// <param name="CategoriaId">Id da categoria; obrigatório fora da transferência.</param>
/// <param name="SubcategoriaId">Id da subcategoria, opcional.</param>
/// <param name="Memo">Observação livre, opcional.</param>
public sealed record DadosLancamento(
    DateOnly Data,
    decimal Valor,
    Guid FavorecidoId,
    Guid? CategoriaId,
    Guid? SubcategoriaId,
    string? Memo);
