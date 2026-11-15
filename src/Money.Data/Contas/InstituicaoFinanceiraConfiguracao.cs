// <copyright file="InstituicaoFinanceiraConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Data.Contas;

/// <summary>Mapeamento da instituição financeira para a tabela InstituicoesFinanceiras.</summary>
internal sealed class InstituicaoFinanceiraConfiguracao : IEntityTypeConfiguration<InstituicaoFinanceira>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<InstituicaoFinanceira> builder)
    {
        builder.ToTable("InstituicoesFinanceiras");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);
    }
}
