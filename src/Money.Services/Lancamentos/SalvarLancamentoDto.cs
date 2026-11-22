// <copyright file="SalvarLancamentoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Dados do formulário de lançamento, para incluir ou alterar.</summary>
/// <param name="Natureza">Gasto, recebimento ou transferência; não muda na alteração.</param>
/// <param name="ContaId">Conta do lançamento; na transferência, a conta De.</param>
/// <param name="ContaDestinoId">Na transferência, a conta Para; vazio nos demais.</param>
/// <param name="Data">Data do lançamento.</param>
/// <param name="Valor">Montante, sempre positivo.</param>
/// <param name="NomeFavorecido">"Pagar a": nome existente ou novo.</param>
/// <param name="NomeCategoria">Categoria: nome existente ou novo; ignorada na transferência.</param>
/// <param name="NomeSubcategoria">Subcategoria opcional da categoria.</param>
/// <param name="Memo">Observação opcional.</param>
/// <param name="ConfirmarSaldoNegativo">Grava mesmo que uma conta fique com saldo negativo.</param>
public sealed record SalvarLancamentoDto(
    NaturezaLancamento Natureza,
    Guid ContaId,
    Guid? ContaDestinoId,
    DateOnly Data,
    decimal Valor,
    string? NomeFavorecido,
    string? NomeCategoria,
    string? NomeSubcategoria,
    string? Memo,
    bool ConfirmarSaldoNegativo);
