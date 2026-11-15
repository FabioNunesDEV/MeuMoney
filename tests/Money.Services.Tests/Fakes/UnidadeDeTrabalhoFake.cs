// <copyright file="UnidadeDeTrabalhoFake.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Services.Abstracoes;

namespace Money.Services.Tests.Fakes;

/// <summary>Unidade de trabalho que só conta as gravações pedidas.</summary>
public sealed class UnidadeDeTrabalhoFake : IUnidadeDeTrabalho
{
    /// <summary>Obtém quantas vezes a gravação foi pedida.</summary>
    public int Gravacoes { get; private set; }

    /// <inheritdoc />
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        this.Gravacoes++;
        return Task.FromResult(0);
    }
}
