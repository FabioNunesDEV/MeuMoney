// <copyright file="LancamentoTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;

namespace Money.Domain.Tests.Lancamentos;

/// <summary>Testes das regras do lançamento.</summary>
public class LancamentoTestes
{
    private static readonly DateOnly Hoje = new(2026, 11, 22);
    private static readonly Guid Favorecido = Guid.CreateVersion7();
    private static readonly Guid Categoria = Guid.CreateVersion7();

    /// <summary>Gasto nasce não reconciliado e subtrai o valor do saldo.</summary>
    [Fact]
    public void CriarGasto_DadosValidos_SubtraiDoSaldo()
    {
        var conta = Dinheiro("Carteira");

        var gasto = Lancamento.CriarGasto(conta, Dados(30m, memo: "  pão  "));

        Assert.Equal(conta.Id, gasto.ContaId);
        Assert.Equal(TipoLancamento.Gasto, gasto.Tipo);
        Assert.Equal(SituacaoLancamento.NaoReconciliado, gasto.Situacao);
        Assert.Equal("pão", gasto.Memo);
        Assert.Equal(-30m, gasto.EfeitoNoSaldo);
        Assert.False(gasto.EhTransferencia);
    }

    /// <summary>Recebimento soma o valor ao saldo.</summary>
    [Fact]
    public void CriarRecebimento_DadosValidos_SomaAoSaldo()
    {
        var recebimento = Lancamento.CriarRecebimento(Dinheiro("Carteira"), Dados(200m));

        Assert.Equal(200m, recebimento.EfeitoNoSaldo);
    }

    /// <summary>Montante zero ou negativo é recusado.</summary>
    /// <param name="valor">Montante testado.</param>
    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void CriarGasto_MontanteNaoPositivo_LancaRegraNegocio(decimal valor)
    {
        var erro = Assert.Throws<RegraNegocioException>(() => Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(valor)));

        Assert.Equal("O montante deve ser maior que zero.", erro.Message);
    }

    /// <summary>Montante com mais casas do que a moeda admite é recusado.</summary>
    [Fact]
    public void CriarGasto_CasasDemais_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(1.234m)));
    }

    /// <summary>Gasto e recebimento exigem categoria.</summary>
    [Fact]
    public void CriarGasto_SemCategoria_LancaRegraNegocio()
    {
        var erro = Assert.Throws<RegraNegocioException>(
            () => Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(10m) with { CategoriaId = null }));

        Assert.Equal("A categoria é obrigatória.", erro.Message);
    }

    /// <summary>Favorecido é obrigatório.</summary>
    [Fact]
    public void CriarGasto_SemFavorecido_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(
            () => Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(10m) with { FavorecidoId = Guid.Empty }));
    }

    /// <summary>Memo acima do limite é recusado.</summary>
    [Fact]
    public void CriarGasto_MemoLongo_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(
            () => Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(10m, memo: new string('a', 256))));
    }

    /// <summary>Conta encerrada ou de outro tipo não aceita lançamento.</summary>
    [Fact]
    public void CriarGasto_ContaQueNaoAceita_LancaRegraNegocio()
    {
        var encerrada = Dinheiro("Velha");
        encerrada.Encerrar();
        var corrente = new Conta(TipoConta.Corrente, "Banco", "BRL", 0m);

        Assert.Throws<RegraNegocioException>(() => Lancamento.CriarGasto(encerrada, Dados(10m)));
        Assert.Throws<RegraNegocioException>(() => Lancamento.CriarGasto(corrente, Dados(10m)));
    }

    /// <summary>Transferência gera duas pontas ligadas, sem categoria, que se anulam na soma.</summary>
    [Fact]
    public void CriarTransferencia_ContasValidas_GeraPontasLigadas()
    {
        var carteira = Dinheiro("Carteira");
        var cofre = Dinheiro("Cofre");

        var (saida, entrada) = Lancamento.CriarTransferencia(carteira, cofre, Dados(40m));

        Assert.Equal(carteira.Id, saida.ContaId);
        Assert.Equal(cofre.Id, entrada.ContaId);
        Assert.NotNull(saida.TransferenciaId);
        Assert.Equal(saida.TransferenciaId, entrada.TransferenciaId);
        Assert.Null(saida.CategoriaId);
        Assert.Equal(-40m, saida.EfeitoNoSaldo);
        Assert.Equal(40m, entrada.EfeitoNoSaldo);
    }

    /// <summary>Transferência para a mesma conta ou entre moedas diferentes é recusada.</summary>
    [Fact]
    public void CriarTransferencia_ContasInvalidas_LancaRegraNegocio()
    {
        var carteira = Dinheiro("Carteira");
        var dolar = new Conta(TipoConta.Dinheiro, "Dólares", "USD", 0m);

        Assert.Throws<RegraNegocioException>(() => Lancamento.CriarTransferencia(carteira, carteira, Dados(1m)));
        var erro = Assert.Throws<RegraNegocioException>(() => Lancamento.CriarTransferencia(carteira, dolar, Dados(1m)));
        Assert.Equal("A transferência exige contas da mesma moeda.", erro.Message);
    }

    /// <summary>Nulo continua registrado, mas não conta no saldo; voltar faz contar de novo.</summary>
    [Fact]
    public void MarcarComo_Nulo_ZeraEfeito()
    {
        var gasto = Lancamento.CriarGasto(Dinheiro("Carteira"), Dados(30m));

        gasto.MarcarComo(SituacaoLancamento.Nulo);
        Assert.Equal(0m, gasto.EfeitoNoSaldo);

        gasto.MarcarComo(SituacaoLancamento.Compensado);
        Assert.Equal(-30m, gasto.EfeitoNoSaldo);
    }

    /// <summary>Alterar troca os dados com as mesmas validações.</summary>
    [Fact]
    public void Alterar_DadosValidos_TrocaValorEData()
    {
        var conta = Dinheiro("Carteira");
        var gasto = Lancamento.CriarGasto(conta, Dados(30m));

        gasto.Alterar(conta, Dados(45.5m) with { Data = Hoje.AddDays(-1) });

        Assert.Equal(45.5m, gasto.Valor);
        Assert.Equal(Hoje.AddDays(-1), gasto.Data);
        Assert.Throws<RegraNegocioException>(() => gasto.Alterar(conta, Dados(-1m)));
    }

    /// <summary>Mover leva o lançamento para outra conta da mesma moeda.</summary>
    [Fact]
    public void MoverPara_ContaDaMesmaMoeda_TrocaConta()
    {
        var carteira = Dinheiro("Carteira");
        var cofre = Dinheiro("Cofre");
        var gasto = Lancamento.CriarGasto(carteira, Dados(30m));

        gasto.MoverPara(carteira, cofre);

        Assert.Equal(cofre.Id, gasto.ContaId);
    }

    /// <summary>Mover para outra moeda ou para a própria conta é recusado.</summary>
    [Fact]
    public void MoverPara_ContaInvalida_LancaRegraNegocio()
    {
        var carteira = Dinheiro("Carteira");
        var dolar = new Conta(TipoConta.Dinheiro, "Dólares", "USD", 0m);
        var gasto = Lancamento.CriarGasto(carteira, Dados(30m));

        Assert.Throws<RegraNegocioException>(() => gasto.MoverPara(carteira, dolar));
        Assert.Throws<RegraNegocioException>(() => gasto.MoverPara(carteira, carteira));
    }

    private static Conta Dinheiro(string nome) => new(TipoConta.Dinheiro, nome, "BRL", 0m);

    private static DadosLancamento Dados(decimal valor, string? memo = null) =>
        new(Hoje, valor, Favorecido, Categoria, null, memo);
}
