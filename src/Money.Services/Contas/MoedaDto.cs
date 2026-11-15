// <copyright file="MoedaDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Contas;

/// <summary>Moeda aceita nas contas.</summary>
/// <param name="Codigo">Código ISO 4217.</param>
/// <param name="Nome">Nome em pt-BR.</param>
/// <param name="Simbolo">Símbolo exibido antes do valor.</param>
/// <param name="CasasDecimais">Casas decimais que a moeda admite.</param>
public sealed record MoedaDto(string Codigo, string Nome, string Simbolo, int CasasDecimais);
