// <copyright file="CadastroSimplesDto.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Contas;

/// <summary>Instituição financeira ou grupo de contas, com quantas contas o usam.</summary>
/// <param name="Id">Id do cadastro.</param>
/// <param name="Nome">Nome do cadastro.</param>
/// <param name="QuantidadeContas">Quantidade de contas associadas, abertas ou encerradas.</param>
public sealed record CadastroSimplesDto(Guid Id, string Nome, int QuantidadeContas);
