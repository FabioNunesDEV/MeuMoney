// <copyright file="GrupoContaConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Data.Contas;

/// <summary>Mapeamento do grupo de contas para a tabela GruposConta.</summary>
internal sealed class GrupoContaConfiguracao : IEntityTypeConfiguration<GrupoConta>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<GrupoConta> builder)
    {
        builder.ToTable("GruposConta");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedNever();
        builder.Property(g => g.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);
    }
}
