// <copyright file="Moeda.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Contas;

/// <summary>Moeda em que uma conta é mantida.</summary>
/// <param name="Codigo">Código ISO 4217, como BRL ou USD.</param>
/// <param name="Nome">Nome em pt-BR.</param>
/// <param name="Simbolo">Símbolo exibido antes do valor.</param>
/// <param name="CasasDecimais">Quantidade de casas decimais que a moeda admite.</param>
public sealed record Moeda(string Codigo, string Nome, string Simbolo, int CasasDecimais);
