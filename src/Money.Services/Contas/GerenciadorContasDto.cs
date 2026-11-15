// <copyright file="GerenciadorContasDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Contas;

/// <summary>Conteúdo do gerenciador de contas.</summary>
/// <param name="Contas">Contas ordenadas por nome; encerradas só se pedidas.</param>
/// <param name="Totais">Total por moeda das contas abertas, real primeiro.</param>
/// <param name="ExistemContas">Indica se há alguma conta cadastrada, aberta ou encerrada.</param>
public sealed record GerenciadorContasDto(
    IReadOnlyList<ContaDto> Contas,
    IReadOnlyList<TotalMoedaDto> Totais,
    bool ExistemContas);
