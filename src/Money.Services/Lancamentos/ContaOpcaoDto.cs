// <copyright file="ContaOpcaoDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>Conta oferecida em De, Para e Mover para conta.</summary>
/// <param name="Id">Id da conta.</param>
/// <param name="Nome">Nome da conta.</param>
public sealed record ContaOpcaoDto(Guid Id, string Nome);
