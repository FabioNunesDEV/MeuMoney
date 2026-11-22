// <copyright file="SubcategoriaConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Lancamentos;

namespace Money.Data.Lancamentos;

/// <summary>Mapeamento da subcategoria para a tabela Subcategorias.</summary>
internal sealed class SubcategoriaConfiguracao : IEntityTypeConfiguration<Subcategoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Subcategoria> builder)
    {
        builder.ToTable("Subcategorias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);
        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(e => e.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
