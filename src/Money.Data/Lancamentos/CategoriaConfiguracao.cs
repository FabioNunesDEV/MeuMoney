// <copyright file="CategoriaConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Comum;
using Money.Domain.Lancamentos;

namespace Money.Data.Lancamentos;

/// <summary>Mapeamento da categoria para a tabela Categorias.</summary>
internal sealed class CategoriaConfiguracao : IEntityTypeConfiguration<Categoria>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nome).IsRequired().HasMaxLength(EntidadeNomeada.TamanhoMaximoNome);
    }
}
