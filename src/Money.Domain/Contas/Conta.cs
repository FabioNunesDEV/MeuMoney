// <copyright file="Conta.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Contas;

/// <summary>Conta do usuário: bancária, cartão, investimento, bem ou dívida, numa moeda.</summary>
public sealed class Conta : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="Conta"/>, aberta e sem vínculos.</summary>
    /// <param name="tipo">Tipo da conta; não muda depois.</param>
    /// <param name="nome">Nome da conta.</param>
    /// <param name="codigoMoeda">Código ISO 4217 da moeda.</param>
    /// <param name="saldoInformado">Saldo inicial como o usuário digita (dívida positiva).</param>
    public Conta(TipoConta tipo, string nome, string codigoMoeda, decimal saldoInformado)
        : base(nome)
    {
        if (!Enum.IsDefined(tipo))
        {
            throw new RegraNegocioException("Tipo de conta inválido.");
        }

        this.Tipo = tipo;
        this.CodigoMoeda = CatalogoMoedas.Obter(codigoMoeda).Codigo;
        this.DefinirSaldoInicial(saldoInformado);
    }

    private Conta()
    {
        this.CodigoMoeda = CatalogoMoedas.CodigoPadrao;
    }

    /// <summary>Obtém o tipo da conta.</summary>
    public TipoConta Tipo { get; private set; }

    /// <summary>Obtém o código ISO 4217 da moeda da conta.</summary>
    public string CodigoMoeda { get; private set; }

    /// <summary>Obtém o saldo inicial com sinal: dívida é negativa.</summary>
    public decimal SaldoInicial { get; private set; }

    /// <summary>Obtém o saldo inicial como o usuário digita: dívida positiva.</summary>
    public decimal SaldoInformado => this.Tipo.RepresentaDivida() ? -this.SaldoInicial : this.SaldoInicial;

    /// <summary>Obtém o Id da instituição que mantém a conta, se houver.</summary>
    public Guid? InstituicaoFinanceiraId { get; private set; }

    /// <summary>Obtém o Id do grupo da conta, se houver.</summary>
    public Guid? GrupoContaId { get; private set; }

    /// <summary>Obtém o Id da conta caixa vinculada, só em conta de investimento.</summary>
    public Guid? ContaCaixaId { get; private set; }

    /// <summary>Obtém um valor que indica se a conta é favorita.</summary>
    public bool Favorita { get; private set; }

    /// <summary>Obtém um valor que indica se a conta está encerrada.</summary>
    public bool Encerrada { get; private set; }

    /// <summary>
    /// Define moeda e saldo inicial juntos, para que o saldo seja validado nas casas da nova moeda.
    /// Investimento com caixa vinculada não troca de moeda.
    /// </summary>
    /// <param name="codigoMoeda">Código ISO 4217 da moeda.</param>
    /// <param name="saldoInformado">Valor digitado; em cartão e passivo, a dívida vem positiva.</param>
    public void DefinirMoedaESaldo(string codigoMoeda, decimal saldoInformado)
    {
        var moeda = CatalogoMoedas.Obter(codigoMoeda);
        if (moeda.Codigo != this.CodigoMoeda && this.ContaCaixaId is not null)
        {
            throw new RegraNegocioException("Desvincule a conta caixa antes de trocar a moeda do investimento.");
        }

        var codigoAnterior = this.CodigoMoeda;
        this.CodigoMoeda = moeda.Codigo;
        try
        {
            this.DefinirSaldoInicial(saldoInformado);
        }
        catch (RegraNegocioException)
        {
            this.CodigoMoeda = codigoAnterior;
            throw;
        }
    }

    /// <summary>Associa a conta a uma instituição ou a deixa sem instituição.</summary>
    /// <param name="instituicaoFinanceiraId">Id da instituição, ou nulo para nenhuma.</param>
    public void DefinirInstituicao(Guid? instituicaoFinanceiraId)
    {
        this.InstituicaoFinanceiraId = instituicaoFinanceiraId;
    }

    /// <summary>Coloca a conta num grupo ou a deixa sem grupo.</summary>
    /// <param name="grupoContaId">Id do grupo, ou nulo para nenhum.</param>
    public void DefinirGrupo(Guid? grupoContaId)
    {
        this.GrupoContaId = grupoContaId;
    }

    /// <summary>Vincula a conta caixa que recebe o dinheiro das compras e vendas do investimento.</summary>
    /// <param name="caixa">Conta Corrente, Poupança ou Dinheiro, aberta e na mesma moeda.</param>
    public void VincularCaixa(Conta caixa)
    {
        if (this.Tipo != TipoConta.Investimento)
        {
            throw new RegraNegocioException("Só conta de investimento tem conta caixa.");
        }

        if (caixa.Id == this.Id || !caixa.Tipo.PodeSerCaixa())
        {
            throw new RegraNegocioException("A conta caixa deve ser Corrente, Poupança ou Dinheiro.");
        }

        if (caixa.Encerrada)
        {
            throw new RegraNegocioException("A conta caixa não pode estar encerrada.");
        }

        if (caixa.CodigoMoeda != this.CodigoMoeda)
        {
            throw new RegraNegocioException("A conta caixa deve ter a mesma moeda do investimento.");
        }

        this.ContaCaixaId = caixa.Id;
    }

    /// <summary>Remove o vínculo com a conta caixa.</summary>
    public void DesvincularCaixa()
    {
        this.ContaCaixaId = null;
    }

    /// <summary>Marca ou desmarca a conta como favorita.</summary>
    /// <param name="favorita">Novo estado.</param>
    public void DefinirFavorita(bool favorita)
    {
        this.Favorita = favorita;
    }

    /// <summary>Encerra a conta: some do gerenciador e do total, mas continua existindo.</summary>
    public void Encerrar()
    {
        this.Encerrada = true;
    }

    /// <summary>Reabre uma conta encerrada.</summary>
    public void Reabrir()
    {
        this.Encerrada = false;
    }

    private void DefinirSaldoInicial(decimal saldoInformado)
    {
        if (this.Tipo == TipoConta.Investimento && saldoInformado != 0)
        {
            throw new RegraNegocioException("Conta de investimento não tem saldo inicial.");
        }

        if (this.Tipo == TipoConta.Ativo && saldoInformado < 0)
        {
            throw new RegraNegocioException("O valor de um ativo não pode ser negativo.");
        }

        var moeda = CatalogoMoedas.Obter(this.CodigoMoeda);
        if (decimal.Round(saldoInformado, moeda.CasasDecimais) != saldoInformado)
        {
            throw new RegraNegocioException(
                $"O valor admite no máximo {moeda.CasasDecimais} casas decimais em {moeda.Nome}.");
        }

        this.SaldoInicial = this.Tipo.RepresentaDivida() ? -saldoInformado : saldoInformado;
    }
}
