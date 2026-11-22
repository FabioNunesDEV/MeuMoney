// <copyright file="LancamentoServico.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;
using Money.Services.Abstracoes;
using Money.Services.Comum;
using Money.Services.Contas;

namespace Money.Services.Lancamentos;

/// <summary>
/// Casos de uso da movimentação das contas: incluir, alterar, marcar, mover e excluir lançamentos,
/// mantendo juntas as duas pontas de uma transferência.
/// </summary>
/// <param name="lancamentos">Repositório de lançamentos.</param>
/// <param name="contas">Repositório de contas.</param>
/// <param name="favorecidos">Repositório de favorecidos.</param>
/// <param name="categorias">Repositório de categorias.</param>
/// <param name="subcategorias">Repositório de subcategorias.</param>
/// <param name="unidadeDeTrabalho">Grava as alterações.</param>
public sealed class LancamentoServico(
    ILancamentoRepositorio lancamentos,
    IRepositorio<Conta> contas,
    IRepositorio<Favorecido> favorecidos,
    IRepositorio<Categoria> categorias,
    IRepositorio<Subcategoria> subcategorias,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    /// <summary>Monta a movimentação da conta, com o saldo depois de cada linha.</summary>
    /// <param name="contaId">Id da conta.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>As linhas em ordem e o saldo final.</returns>
    public async Task<MovimentacaoDto> ObterMovimentacaoAsync(Guid contaId, CancellationToken cancellationToken)
    {
        var conta = await this.ObterContaAsync(contaId, cancellationToken);
        var daConta = await lancamentos.ListarDaContaAsync(contaId, cancellationToken);
        var cadastros = await this.CarregarCadastrosAsync(cancellationToken);
        var outrasPontas = daConta.Any(l => l.EhTransferencia)
            ? (await lancamentos.ListarAsync(cancellationToken))
                .Where(l => l.EhTransferencia && l.ContaId != contaId)
                .ToDictionary(l => l.TransferenciaId!.Value, l => l.ContaId)
            : [];

        var saldo = conta.SaldoInicial;
        var linhas = new List<LancamentoDto>(daConta.Count);
        foreach (var lancamento in daConta)
        {
            saldo += lancamento.EfeitoNoSaldo;
            Guid? outraContaId = lancamento.TransferenciaId is Guid id && outrasPontas.TryGetValue(id, out var outra) ? outra : null;
            linhas.Add(new LancamentoDto(
                lancamento.Id,
                lancamento.ContaId,
                lancamento.Tipo,
                lancamento.Data,
                lancamento.Valor,
                lancamento.Situacao,
                cadastros.NomeFavorecido(lancamento.FavorecidoId) ?? string.Empty,
                cadastros.NomeCategoria(lancamento.CategoriaId),
                cadastros.NomeSubcategoria(lancamento.SubcategoriaId),
                lancamento.Memo,
                outraContaId,
                cadastros.NomeConta(outraContaId),
                saldo));
        }

        return new MovimentacaoDto(contaId, conta.SaldoInicial, linhas, saldo);
    }

    /// <summary>
    /// Monta o saldo da conta no tempo: o saldo no início do período e ao fim de cada dia com
    /// lançamento, o mesmo cálculo da Movimentação (nulos não contam).
    /// </summary>
    /// <param name="contaId">Id da conta.</param>
    /// <param name="de">Primeiro dia do período; vazio começa no primeiro lançamento.</param>
    /// <param name="ate">Último dia do período (hoje, para a tela); vai além se houver lançamento depois.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Os pontos do gráfico e o resumo do período.</returns>
    public async Task<HistoricoSaldoDto> ObterHistoricoAsync(Guid contaId, DateOnly? de, DateOnly ate, CancellationToken cancellationToken)
    {
        var conta = await this.ObterContaAsync(contaId, cancellationToken);
        var daConta = await lancamentos.ListarDaContaAsync(contaId, cancellationToken);
        if (daConta.Count == 0)
        {
            var inicio = de ?? ate;
            return new HistoricoSaldoDto(contaId, inicio, ate, [], conta.SaldoInicial, conta.SaldoInicial, conta.SaldoInicial, conta.SaldoInicial, false);
        }

        var primeiroDia = de ?? daConta[0].Data;
        var ultimoDia = daConta[^1].Data > ate ? daConta[^1].Data : ate;
        if (primeiroDia > ultimoDia)
        {
            throw new RegraNegocioException("O início do período deve ser anterior ao fim.");
        }

        var saldo = conta.SaldoInicial + daConta.Where(l => l.Data < primeiroDia).Sum(l => l.EfeitoNoSaldo);
        var pontos = new List<PontoSaldoDto> { new(primeiroDia, saldo) };
        foreach (var dia in daConta.Where(l => l.Data >= primeiroDia && l.Data <= ultimoDia).GroupBy(l => l.Data))
        {
            saldo += dia.Sum(l => l.EfeitoNoSaldo);
            pontos.Add(new PontoSaldoDto(dia.Key, saldo));
        }

        if (pontos[^1].Data < ultimoDia)
        {
            pontos.Add(new PontoSaldoDto(ultimoDia, saldo));
        }

        return new HistoricoSaldoDto(
            contaId,
            primeiroDia,
            ultimoDia,
            pontos,
            pontos[0].Saldo,
            pontos.Max(p => p.Saldo),
            pontos.Min(p => p.Saldo),
            saldo,
            true);
    }

    /// <summary>Monta as listas do formulário: favorecidos com sugestão, categorias e contas compatíveis.</summary>
    /// <param name="contaId">Conta da página, que define a moeda das contas compatíveis.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>As opções do formulário.</returns>
    public async Task<OpcoesLancamentoDto> ObterOpcoesAsync(Guid contaId, CancellationToken cancellationToken)
    {
        var conta = await this.ObterContaAsync(contaId, cancellationToken);
        var cadastros = await this.CarregarCadastrosAsync(cancellationToken);
        var maisRecentePorFavorecido = (await lancamentos.ListarAsync(cancellationToken))
            .GroupBy(l => l.FavorecidoId)
            .ToDictionary(g => g.Key, g => g.OrderBy(l => l.Data).ThenBy(l => l.Id).Last());

        var listaFavorecidos = cadastros.Favorecidos
            .OrderBy(f => f.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(f => maisRecentePorFavorecido.TryGetValue(f.Id, out var ultimo)
                ? new FavorecidoOpcaoDto(f.Nome, cadastros.NomeCategoria(ultimo.CategoriaId), cadastros.NomeSubcategoria(ultimo.SubcategoriaId), ultimo.Valor)
                : new FavorecidoOpcaoDto(f.Nome, null, null, null))
            .ToList();
        var listaCategorias = cadastros.Categorias
            .OrderBy(c => c.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(c => new CategoriaOpcaoDto(
                c.Nome,
                cadastros.Subcategorias
                    .Where(s => s.CategoriaId == c.Id)
                    .Select(s => s.Nome)
                    .OrderBy(n => n, StringComparer.InvariantCultureIgnoreCase)
                    .ToList()))
            .ToList();
        var compativeis = cadastros.Contas
            .Where(c => c.Tipo == TipoConta.Dinheiro && !c.Encerrada && c.CodigoMoeda == conta.CodigoMoeda)
            .OrderBy(c => c.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(c => new ContaOpcaoDto(c.Id, c.Nome))
            .ToList();

        return new OpcoesLancamentoDto(listaFavorecidos, listaCategorias, compativeis);
    }

    /// <summary>
    /// Inclui um gasto, um recebimento ou uma transferência. Se alguma conta ficar negativa e não houver
    /// confirmação, nada é gravado e o resultado traz as contas.
    /// </summary>
    /// <param name="dados">Dados do formulário.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Gravado, ou as contas que pedem confirmação.</returns>
    public async Task<ResultadoLancamentoDto> CriarAsync(SalvarLancamentoDto dados, CancellationToken cancellationToken)
    {
        var conta = await this.ObterContaAsync(dados.ContaId, cancellationToken);
        var somas = await lancamentos.SomarEfeitosPorContaAsync(cancellationToken);
        var digitados = await this.ResolverDadosAsync(dados, cancellationToken);
        var novos = new List<(Conta Conta, Lancamento Lancamento)>();
        switch (dados.Natureza)
        {
            case NaturezaLancamento.Gasto:
                novos.Add((conta, Lancamento.CriarGasto(conta, digitados)));
                break;
            case NaturezaLancamento.Recebimento:
                novos.Add((conta, Lancamento.CriarRecebimento(conta, digitados)));
                break;
            case NaturezaLancamento.Transferencia:
                var para = await this.ObterContaDestinoAsync(dados, cancellationToken);
                var (saida, entrada) = Lancamento.CriarTransferencia(conta, para, digitados);
                novos.Add((conta, saida));
                novos.Add((para, entrada));
                break;
            default:
                throw new RegraNegocioException("Tipo de lançamento inválido.");
        }

        var variacoes = novos.Select(n => (n.Conta, Variacao: n.Lancamento.EfeitoNoSaldo));
        var negativos = VerificarSaldos(variacoes, somas);
        if (negativos.Count > 0 && !dados.ConfirmarSaldoNegativo)
        {
            return new ResultadoLancamentoDto(false, negativos);
        }

        foreach (var (_, lancamento) in novos)
        {
            lancamentos.Adicionar(lancamento);
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        return new ResultadoLancamentoDto(true, []);
    }

    /// <summary>
    /// Altera um lançamento; na transferência, as duas pontas. Se alguma conta ficar negativa e não houver
    /// confirmação, nada é gravado e o resultado traz as contas.
    /// </summary>
    /// <param name="id">Id do lançamento (qualquer ponta, na transferência).</param>
    /// <param name="dados">Dados do formulário.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Gravado, ou as contas que pedem confirmação.</returns>
    public async Task<ResultadoLancamentoDto> AlterarAsync(Guid id, SalvarLancamentoDto dados, CancellationToken cancellationToken)
    {
        var lancamento = await this.ObterLancamentoAsync(id, cancellationToken);
        var naturezaAtual = lancamento.Tipo switch
        {
            TipoLancamento.Gasto => NaturezaLancamento.Gasto,
            TipoLancamento.Recebimento => NaturezaLancamento.Recebimento,
            _ => NaturezaLancamento.Transferencia,
        };
        if (dados.Natureza != naturezaAtual)
        {
            throw new RegraNegocioException("O tipo do lançamento não muda na alteração.");
        }

        // Os saldos atuais são lidos antes de mexer nos lançamentos.
        var somas = await lancamentos.SomarEfeitosPorContaAsync(cancellationToken);
        var digitados = await this.ResolverDadosAsync(dados, cancellationToken);
        var pontas = lancamento.EhTransferencia
            ? await lancamentos.ListarDaTransferenciaAsync(lancamento.TransferenciaId!.Value, cancellationToken)
            : [lancamento];
        var antes = pontas.Select(p => (p.ContaId, Efeito: p.EfeitoNoSaldo)).ToList();

        var alteradas = new List<Conta>();
        if (lancamento.EhTransferencia)
        {
            var de = await this.ObterContaAsync(dados.ContaId, cancellationToken);
            var para = await this.ObterContaDestinoAsync(dados, cancellationToken);
            Lancamento.GarantirContasDaTransferencia(de, para);
            await this.AlterarPontaAsync(pontas.Single(p => p.Tipo == TipoLancamento.TransferenciaSaida), de, digitados, cancellationToken);
            await this.AlterarPontaAsync(pontas.Single(p => p.Tipo == TipoLancamento.TransferenciaEntrada), para, digitados, cancellationToken);
            alteradas.AddRange([de, para]);
        }
        else
        {
            var conta = await this.ObterContaAsync(lancamento.ContaId, cancellationToken);
            if (dados.ContaId != conta.Id)
            {
                throw new RegraNegocioException("Para levar o lançamento a outra conta, use Mover para conta.");
            }

            lancamento.Alterar(conta, digitados);
            alteradas.Add(conta);
        }

        // Variação de cada conta: o efeito novo menos o antigo (uma ponta pode ter mudado de conta).
        var todasAsContas = alteradas.Concat(await this.ObterContasAsync(antes.Select(a => a.ContaId), cancellationToken))
            .DistinctBy(c => c.Id);
        var variacoes = todasAsContas.Select(c => (
            Conta: c,
            Variacao: pontas.Where(p => p.ContaId == c.Id).Sum(p => p.EfeitoNoSaldo) - antes.Where(a => a.ContaId == c.Id).Sum(a => a.Efeito)));
        var negativos = VerificarSaldos(variacoes, somas);
        if (negativos.Count > 0 && !dados.ConfirmarSaldoNegativo)
        {
            // Sem gravar, as alterações feitas em memória morrem com o escopo da requisição.
            return new ResultadoLancamentoDto(false, negativos);
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
        return new ResultadoLancamentoDto(true, []);
    }

    /// <summary>
    /// Marca a situação. Na transferência, Nulo vale para as duas pontas, e sair de Nulo tira as duas
    /// de Nulo; Compensado e Reconciliado valem só para a ponta marcada.
    /// </summary>
    /// <param name="id">Id do lançamento.</param>
    /// <param name="situacao">Situação nova.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task MarcarAsync(Guid id, SituacaoLancamento situacao, CancellationToken cancellationToken)
    {
        var lancamento = await this.ObterLancamentoAsync(id, cancellationToken);
        var pontas = await this.ObterPontasAsync(lancamento, cancellationToken);
        foreach (var ponta in pontas)
        {
            if (ponta.Id == lancamento.Id || situacao == SituacaoLancamento.Nulo)
            {
                ponta.MarcarComo(situacao);
            }
            else if (ponta.Situacao == SituacaoLancamento.Nulo)
            {
                ponta.MarcarComo(SituacaoLancamento.NaoReconciliado);
            }
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Leva o lançamento para outra conta Dinheiro aberta da mesma moeda.</summary>
    /// <param name="id">Id do lançamento.</param>
    /// <param name="contaDestinoId">Conta de destino.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task MoverAsync(Guid id, Guid contaDestinoId, CancellationToken cancellationToken)
    {
        var lancamento = await this.ObterLancamentoAsync(id, cancellationToken);
        if (lancamento.EhTransferencia)
        {
            var outraPonta = (await lancamentos.ListarDaTransferenciaAsync(lancamento.TransferenciaId!.Value, cancellationToken))
                .FirstOrDefault(p => p.Id != lancamento.Id);
            if (outraPonta?.ContaId == contaDestinoId)
            {
                throw new RegraNegocioException("A transferência não pode ir para a conta do outro lado.");
            }
        }

        var atual = await this.ObterContaAsync(lancamento.ContaId, cancellationToken);
        var destino = await this.ObterContaAsync(contaDestinoId, cancellationToken);
        lancamento.MoverPara(atual, destino);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Exclui o lançamento; na transferência, as duas pontas.</summary>
    /// <param name="id">Id do lançamento.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
    {
        var lancamento = await this.ObterLancamentoAsync(id, cancellationToken);
        foreach (var ponta in await this.ObterPontasAsync(lancamento, cancellationToken))
        {
            lancamentos.Remover(ponta);
        }

        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>
    /// Lista as contas que terminariam com saldo negativo depois de somar a variação, contando só as
    /// que perdem dinheiro: receber numa conta já negativa não pede confirmação.
    /// </summary>
    /// <param name="variacoes">Quanto cada conta ganha ou perde com a gravação.</param>
    /// <param name="somas">Soma atual dos lançamentos de cada conta, lida antes das alterações.</param>
    /// <returns>As contas que ficariam negativas.</returns>
    private static List<SaldoNegativoDto> VerificarSaldos(
        IEnumerable<(Conta Conta, decimal Variacao)> variacoes,
        IReadOnlyDictionary<Guid, decimal> somas)
    {
        return variacoes
            .GroupBy(v => v.Conta.Id)
            .Select(g => (Conta: g.First().Conta, Variacao: g.Sum(v => v.Variacao)))
            .Where(v => v.Variacao < 0)
            .Select(v => (v.Conta, Final: v.Conta.SaldoInicial + somas.GetValueOrDefault(v.Conta.Id) + v.Variacao))
            .Where(v => v.Final < 0)
            .Select(v => new SaldoNegativoDto(v.Conta.Id, v.Conta.Nome, v.Final, ContaServico.ParaDto(CatalogoMoedas.Obter(v.Conta.CodigoMoeda))))
            .ToList();
    }

    /// <summary>
    /// Devolve as pontas do lançamento (ele só, ou as duas da transferência), garantindo que todas as
    /// contas envolvidas aceitam alteração.
    /// </summary>
    /// <param name="lancamento">Lançamento escolhido.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>As pontas.</returns>
    private async Task<IReadOnlyList<Lancamento>> ObterPontasAsync(Lancamento lancamento, CancellationToken cancellationToken)
    {
        var pontas = lancamento.EhTransferencia
            ? await lancamentos.ListarDaTransferenciaAsync(lancamento.TransferenciaId!.Value, cancellationToken)
            : [lancamento];
        foreach (var conta in await this.ObterContasAsync(pontas.Select(p => p.ContaId), cancellationToken))
        {
            conta.GarantirAceitaLancamento();
        }

        return pontas;
    }

    private async Task AlterarPontaAsync(Lancamento ponta, Conta conta, DadosLancamento dados, CancellationToken cancellationToken)
    {
        if (ponta.ContaId != conta.Id)
        {
            ponta.MoverPara(await this.ObterContaAsync(ponta.ContaId, cancellationToken), conta);
        }

        ponta.Alterar(conta, dados);
    }

    /// <summary>Resolve favorecido, categoria e subcategoria pelo nome, criando os que não existirem.</summary>
    /// <param name="dados">Dados do formulário.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Os dados com os Ids.</returns>
    private async Task<DadosLancamento> ResolverDadosAsync(SalvarLancamentoDto dados, CancellationToken cancellationToken)
    {
        var favorecidoId = await Nomes.ResolverPorNomeAsync(favorecidos, dados.NomeFavorecido, n => new Favorecido(n), cancellationToken);
        Guid? categoriaId = null;
        Guid? subcategoriaId = null;
        if (dados.Natureza != NaturezaLancamento.Transferencia)
        {
            categoriaId = await Nomes.ResolverPorNomeAsync(categorias, dados.NomeCategoria, n => new Categoria(n), cancellationToken);
            if (categoriaId is Guid idCategoria)
            {
                subcategoriaId = await Nomes.ResolverPorNomeAsync(
                    subcategorias,
                    dados.NomeSubcategoria,
                    n => new Subcategoria(idCategoria, n),
                    cancellationToken,
                    s => s.CategoriaId == idCategoria);
            }
        }

        return new DadosLancamento(dados.Data, dados.Valor, favorecidoId ?? Guid.Empty, categoriaId, subcategoriaId, dados.Memo);
    }

    private async Task<Conta> ObterContaDestinoAsync(SalvarLancamentoDto dados, CancellationToken cancellationToken)
    {
        if (dados.ContaDestinoId is not Guid destinoId)
        {
            throw new RegraNegocioException("Escolha a conta Para da transferência.");
        }

        return await this.ObterContaAsync(destinoId, cancellationToken);
    }

    private async Task<Conta> ObterContaAsync(Guid id, CancellationToken cancellationToken)
    {
        return await contas.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException("Conta não encontrada.");
    }

    private async Task<IReadOnlyList<Conta>> ObterContasAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var lista = new List<Conta>();
        foreach (var id in ids.Distinct())
        {
            lista.Add(await this.ObterContaAsync(id, cancellationToken));
        }

        return lista;
    }

    private async Task<Lancamento> ObterLancamentoAsync(Guid id, CancellationToken cancellationToken)
    {
        return await lancamentos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException("Lançamento não encontrado.");
    }

    private async Task<Cadastros> CarregarCadastrosAsync(CancellationToken cancellationToken)
    {
        return new Cadastros(
            await contas.ListarAsync(cancellationToken),
            await favorecidos.ListarAsync(cancellationToken),
            await categorias.ListarAsync(cancellationToken),
            await subcategorias.ListarAsync(cancellationToken));
    }

    /// <summary>Cadastros carregados uma vez, para trocar Ids por nomes nas linhas.</summary>
    /// <param name="Contas">Todas as contas.</param>
    /// <param name="Favorecidos">Todos os favorecidos.</param>
    /// <param name="Categorias">Todas as categorias.</param>
    /// <param name="Subcategorias">Todas as subcategorias.</param>
    private sealed record Cadastros(
        IReadOnlyList<Conta> Contas,
        IReadOnlyList<Favorecido> Favorecidos,
        IReadOnlyList<Categoria> Categorias,
        IReadOnlyList<Subcategoria> Subcategorias)
    {
        /// <summary>Nome da conta.</summary>
        /// <param name="id">Id da conta, ou nulo.</param>
        /// <returns>O nome, ou nulo.</returns>
        public string? NomeConta(Guid? id) => this.Contas.FirstOrDefault(c => c.Id == id)?.Nome;

        /// <summary>Nome do favorecido.</summary>
        /// <param name="id">Id do favorecido.</param>
        /// <returns>O nome, ou nulo.</returns>
        public string? NomeFavorecido(Guid id) => this.Favorecidos.FirstOrDefault(f => f.Id == id)?.Nome;

        /// <summary>Nome da categoria.</summary>
        /// <param name="id">Id da categoria, ou nulo.</param>
        /// <returns>O nome, ou nulo.</returns>
        public string? NomeCategoria(Guid? id) => this.Categorias.FirstOrDefault(c => c.Id == id)?.Nome;

        /// <summary>Nome da subcategoria.</summary>
        /// <param name="id">Id da subcategoria, ou nulo.</param>
        /// <returns>O nome, ou nulo.</returns>
        public string? NomeSubcategoria(Guid? id) => this.Subcategorias.FirstOrDefault(s => s.Id == id)?.Nome;
    }
}
