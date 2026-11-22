// <copyright file="Subcategoria.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Lancamentos;

/// <summary>Detalhe de uma categoria (ex.: "Combustível" em "Carro"); o nome é único dentro da categoria.</summary>
public sealed class Subcategoria : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="Subcategoria"/>.</summary>
    /// <param name="categoriaId">Id da categoria a que pertence.</param>
    /// <param name="nome">Nome da subcategoria.</param>
    public Subcategoria(Guid categoriaId, string nome)
        : base(nome)
    {
        this.CategoriaId = categoriaId;
    }

    private Subcategoria()
    {
    }

    /// <summary>Obtém o Id da categoria a que a subcategoria pertence.</summary>
    public Guid CategoriaId { get; private set; }
}
