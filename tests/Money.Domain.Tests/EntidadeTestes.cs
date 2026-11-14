// <copyright file="EntidadeTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Tests;

/// <summary>Testes da classe base das entidades.</summary>
public class EntidadeTestes
{
    /// <summary>Entidades novas recebem Id preenchido e diferente entre si.</summary>
    [Fact]
    public void NovaEntidade_TemIdUnicoEPreenchido()
    {
        var a = new EntidadeFake();
        var b = new EntidadeFake();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }

    private sealed class EntidadeFake : Entidade;
}
