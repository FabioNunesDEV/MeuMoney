// <copyright file="LancamentoConfiguracao.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;

namespace Money.Data.Lancamentos;

/// <summary>Mapeamento do lançamento para a tabela Lancamentos.</summary>
internal sealed class LancamentoConfiguracao : IEntityTypeConfiguration<Lancamento>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("Lancamentos");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        // Texto em vez de número: legível no banco e imune à reordenação do enum.
        builder.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(25);
        builder.Property(l => l.Situacao).HasConversion<string>().HasMaxLength(20);
        builder.Property(l => l.Valor).HasPrecision(18, 2);
        builder.Property(l => l.Memo).HasMaxLength(Lancamento.TamanhoMaximoMemo);

        builder.HasOne<Conta>()
            .WithMany()
            .HasForeignKey(l => l.ContaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Favorecido>()
            .WithMany()
            .HasForeignKey(l => l.FavorecidoId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Categoria>()
            .WithMany()
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Subcategoria>()
            .WithMany()
            .HasForeignKey(l => l.SubcategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => new { l.ContaId, l.Data });

        // Liga as duas pontas da transferência sem chave estrangeira: uma apontando para a outra
        // formaria um ciclo que impediria excluir as duas na mesma gravação.
        builder.HasIndex(l => l.TransferenciaId);
    }
}
