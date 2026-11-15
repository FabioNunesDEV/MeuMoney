// <copyright file="InstituicaoFinanceira.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Contas;

/// <summary>Banco, corretora ou outra instituição que mantém contas.</summary>
public sealed class InstituicaoFinanceira : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="InstituicaoFinanceira"/>.</summary>
    /// <param name="nome">Nome da instituição.</param>
    public InstituicaoFinanceira(string nome)
        : base(nome)
    {
    }

    private InstituicaoFinanceira()
    {
    }
}
