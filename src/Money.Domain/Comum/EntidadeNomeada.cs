// <copyright file="EntidadeNomeada.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Comum;

/// <summary>Entidade identificada para o usuário por um nome, com a validação comum de nome.</summary>
public abstract class EntidadeNomeada : Entidade
{
    /// <summary>Tamanho máximo do nome, sem os espaços das pontas.</summary>
    public const int TamanhoMaximoNome = 100;

    /// <summary>Inicia uma nova instância da classe <see cref="EntidadeNomeada"/>.</summary>
    /// <param name="nome">Nome informado pelo usuário.</param>
    protected EntidadeNomeada(string nome)
    {
        this.Nome = ValidarNome(nome);
    }

    /// <summary>Inicia uma nova instância da classe <see cref="EntidadeNomeada"/>. Usado pelo EF.</summary>
    protected EntidadeNomeada()
    {
        this.Nome = string.Empty;
    }

    /// <summary>Obtém o nome, sem espaços nas pontas.</summary>
    public string Nome { get; private set; }

    /// <summary>Valida o nome com as regras de toda entidade nomeada.</summary>
    /// <param name="nome">Nome informado pelo usuário.</param>
    /// <returns>O nome sem os espaços das pontas.</returns>
    public static string ValidarNome(string? nome)
    {
        var limpo = nome?.Trim() ?? string.Empty;
        if (limpo.Length == 0)
        {
            throw new RegraNegocioException("O nome é obrigatório.");
        }

        if (limpo.Length > TamanhoMaximoNome)
        {
            throw new RegraNegocioException($"O nome deve ter no máximo {TamanhoMaximoNome} caracteres.");
        }

        return limpo;
    }

    /// <summary>Troca o nome.</summary>
    /// <param name="nome">Novo nome.</param>
    public void Renomear(string nome)
    {
        this.Nome = ValidarNome(nome);
    }
}
