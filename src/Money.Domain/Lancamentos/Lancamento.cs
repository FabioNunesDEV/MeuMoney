// <copyright file="Lancamento.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Domain.Lancamentos;

/// <summary>
/// Movimento de dinheiro numa conta: gasto, recebimento ou uma das pontas de uma transferência.
/// O valor é sempre positivo; o tipo diz se ele entra ou sai.
/// </summary>
public sealed class Lancamento : Entidade
{
    /// <summary>Tamanho máximo do memo.</summary>
    public const int TamanhoMaximoMemo = 255;

    private Lancamento(Conta conta, TipoLancamento tipo, Guid? transferenciaId)
    {
        conta.GarantirAceitaLancamento();
        this.ContaId = conta.Id;
        this.Tipo = tipo;
        this.TransferenciaId = transferenciaId;
    }

    private Lancamento()
    {
    }

    /// <summary>Obtém o Id da conta do lançamento.</summary>
    public Guid ContaId { get; private set; }

    /// <summary>Obtém a natureza do lançamento.</summary>
    public TipoLancamento Tipo { get; private set; }

    /// <summary>Obtém a data do lançamento.</summary>
    public DateOnly Data { get; private set; }

    /// <summary>Obtém o valor, sempre maior que zero.</summary>
    public decimal Valor { get; private set; }

    /// <summary>Obtém o Id do favorecido ("Pagar a").</summary>
    public Guid FavorecidoId { get; private set; }

    /// <summary>Obtém o Id da categoria; vazio na transferência.</summary>
    public Guid? CategoriaId { get; private set; }

    /// <summary>Obtém o Id da subcategoria, se houver.</summary>
    public Guid? SubcategoriaId { get; private set; }

    /// <summary>Obtém o memo, se houver.</summary>
    public string? Memo { get; private set; }

    /// <summary>Obtém a situação (C, R, Nulo...).</summary>
    public SituacaoLancamento Situacao { get; private set; }

    /// <summary>Obtém o Id que liga as duas pontas de uma transferência; vazio nos demais.</summary>
    public Guid? TransferenciaId { get; private set; }

    /// <summary>Obtém um valor que indica se o lançamento é uma ponta de transferência.</summary>
    public bool EhTransferencia => this.TransferenciaId is not null;

    /// <summary>Obtém um valor que indica se o lançamento tira dinheiro da conta.</summary>
    public bool EhSaida => this.Tipo is TipoLancamento.Gasto or TipoLancamento.TransferenciaSaida;

    /// <summary>Obtém quanto o lançamento soma ao saldo da conta: negativo na saída, zero se nulo.</summary>
    public decimal EfeitoNoSaldo => this.Situacao == SituacaoLancamento.Nulo ? 0m : (this.EhSaida ? -this.Valor : this.Valor);

    /// <summary>Cria um gasto.</summary>
    /// <param name="conta">Conta de onde sai o dinheiro.</param>
    /// <param name="dados">Dados digitados.</param>
    /// <returns>O lançamento criado.</returns>
    public static Lancamento CriarGasto(Conta conta, DadosLancamento dados)
    {
        var lancamento = new Lancamento(conta, TipoLancamento.Gasto, null);
        lancamento.Alterar(conta, dados);
        return lancamento;
    }

    /// <summary>Cria um recebimento.</summary>
    /// <param name="conta">Conta onde entra o dinheiro.</param>
    /// <param name="dados">Dados digitados.</param>
    /// <returns>O lançamento criado.</returns>
    public static Lancamento CriarRecebimento(Conta conta, DadosLancamento dados)
    {
        var lancamento = new Lancamento(conta, TipoLancamento.Recebimento, null);
        lancamento.Alterar(conta, dados);
        return lancamento;
    }

    /// <summary>Cria as duas pontas de uma transferência, ligadas pelo mesmo <see cref="TransferenciaId"/>.</summary>
    /// <param name="de">Conta de onde sai o dinheiro.</param>
    /// <param name="para">Conta onde entra o dinheiro.</param>
    /// <param name="dados">Dados digitados; categoria e subcategoria são ignoradas.</param>
    /// <returns>A ponta de saída e a de entrada.</returns>
    public static (Lancamento Saida, Lancamento Entrada) CriarTransferencia(Conta de, Conta para, DadosLancamento dados)
    {
        GarantirContasDaTransferencia(de, para);
        var transferenciaId = Guid.CreateVersion7();
        var saida = new Lancamento(de, TipoLancamento.TransferenciaSaida, transferenciaId);
        var entrada = new Lancamento(para, TipoLancamento.TransferenciaEntrada, transferenciaId);
        saida.Alterar(de, dados);
        entrada.Alterar(para, dados);
        return (saida, entrada);
    }

    /// <summary>Recusa uma transferência entre a mesma conta ou entre moedas diferentes.</summary>
    /// <param name="de">Conta de onde sai o dinheiro.</param>
    /// <param name="para">Conta onde entra o dinheiro.</param>
    public static void GarantirContasDaTransferencia(Conta de, Conta para)
    {
        if (de.Id == para.Id)
        {
            throw new RegraNegocioException("Escolha contas diferentes em De e Para.");
        }

        if (de.CodigoMoeda != para.CodigoMoeda)
        {
            throw new RegraNegocioException("A transferência exige contas da mesma moeda.");
        }

        de.GarantirAceitaLancamento();
        para.GarantirAceitaLancamento();
    }

    /// <summary>Troca os dados digitados, com as validações da criação.</summary>
    /// <param name="conta">Conta do lançamento, para as casas decimais da moeda.</param>
    /// <param name="dados">Dados novos; na transferência, categoria e subcategoria são ignoradas.</param>
    public void Alterar(Conta conta, DadosLancamento dados)
    {
        this.GarantirDaConta(conta);
        conta.GarantirAceitaLancamento();
        if (dados.Valor <= 0)
        {
            throw new RegraNegocioException("O montante deve ser maior que zero.");
        }

        var moeda = CatalogoMoedas.Obter(conta.CodigoMoeda);
        if (decimal.Round(dados.Valor, moeda.CasasDecimais) != dados.Valor)
        {
            throw new RegraNegocioException(
                $"O montante admite no máximo {moeda.CasasDecimais} casas decimais em {moeda.Nome}.");
        }

        if (dados.FavorecidoId == Guid.Empty)
        {
            throw new RegraNegocioException("Informe a quem se paga (Pagar a).");
        }

        if (!this.EhTransferencia && dados.CategoriaId is null)
        {
            throw new RegraNegocioException("A categoria é obrigatória.");
        }

        var memo = string.IsNullOrWhiteSpace(dados.Memo) ? null : dados.Memo.Trim();
        if (memo?.Length > TamanhoMaximoMemo)
        {
            throw new RegraNegocioException($"O memo deve ter no máximo {TamanhoMaximoMemo} caracteres.");
        }

        this.Data = dados.Data;
        this.Valor = dados.Valor;
        this.FavorecidoId = dados.FavorecidoId;
        this.CategoriaId = this.EhTransferencia ? null : dados.CategoriaId;
        this.SubcategoriaId = this.EhTransferencia ? null : dados.SubcategoriaId;
        this.Memo = memo;
    }

    /// <summary>Marca a situação do lançamento.</summary>
    /// <param name="situacao">Situação nova.</param>
    public void MarcarComo(SituacaoLancamento situacao)
    {
        if (!Enum.IsDefined(situacao))
        {
            throw new RegraNegocioException("Situação inválida.");
        }

        this.Situacao = situacao;
    }

    /// <summary>Leva o lançamento para outra conta da mesma moeda.</summary>
    /// <param name="atual">Conta onde o lançamento está.</param>
    /// <param name="destino">Conta para onde vai.</param>
    public void MoverPara(Conta atual, Conta destino)
    {
        this.GarantirDaConta(atual);
        atual.GarantirAceitaLancamento();
        destino.GarantirAceitaLancamento();
        if (destino.Id == atual.Id)
        {
            throw new RegraNegocioException("O lançamento já está nesta conta.");
        }

        if (destino.CodigoMoeda != atual.CodigoMoeda)
        {
            throw new RegraNegocioException("O lançamento só pode ir para uma conta da mesma moeda.");
        }

        this.ContaId = destino.Id;
    }

    private void GarantirDaConta(Conta conta)
    {
        if (conta.Id != this.ContaId)
        {
            throw new RegraNegocioException("O lançamento não pertence a esta conta.");
        }
    }
}
