// <copyright file="LancamentoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Lancamentos;

namespace Money.Services.Lancamentos;

/// <summary>Uma linha da movimentação da conta.</summary>
/// <param name="Id">Id do lançamento.</param>
/// <param name="ContaId">Conta do lançamento.</param>
/// <param name="Tipo">Natureza: gasto, recebimento ou ponta de transferência.</param>
/// <param name="Data">Data do lançamento.</param>
/// <param name="Valor">Montante, sempre positivo.</param>
/// <param name="Situacao">Situação (C, R, Nulo...).</param>
/// <param name="NomeFavorecido">"Pagar a".</param>
/// <param name="NomeCategoria">Categoria, se houver.</param>
/// <param name="NomeSubcategoria">Subcategoria, se houver.</param>
/// <param name="Memo">Memo, se houver.</param>
/// <param name="OutraContaId">Na transferência, a conta do outro lado.</param>
/// <param name="NomeOutraConta">Na transferência, o nome da conta do outro lado.</param>
/// <param name="SaldoApos">Saldo da conta depois desta linha.</param>
public sealed record LancamentoDto(
    Guid Id,
    Guid ContaId,
    TipoLancamento Tipo,
    DateOnly Data,
    decimal Valor,
    SituacaoLancamento Situacao,
    string NomeFavorecido,
    string? NomeCategoria,
    string? NomeSubcategoria,
    string? Memo,
    Guid? OutraContaId,
    string? NomeOutraConta,
    decimal SaldoApos);
