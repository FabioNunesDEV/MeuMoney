// <copyright file="BarraTitulo.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Money.Desktop;

/// <summary>
/// Pinta a barra de título da janela pelo gerenciador de janelas do Windows (DWM).
/// </summary>
internal static class BarraTitulo
{
    private const int AtributoModoEscuro = 20;
    private const int AtributoCorFundo = 35;
    private const int AtributoCorTexto = 36;

    /// <summary>
    /// Deixa a barra de título escura, com botões claros. Vale também no Windows 10, que não aceita cor própria.
    /// </summary>
    /// <param name="janela">Janela cuja barra de título será alterada.</param>
    public static void AtivarModoEscuro(Window janela)
    {
        Definir(janela, AtributoModoEscuro, 1);
    }

    /// <summary>
    /// Pinta o fundo e o título da barra com cores no formato <c>#rrggbb</c>. Cor em outro formato é
    /// ignorada, e no Windows 10 o DWM recusa: nos dois casos fica valendo o modo escuro.
    /// </summary>
    /// <param name="janela">Janela cuja barra de título será pintada.</param>
    /// <param name="fundo">Cor de fundo.</param>
    /// <param name="texto">Cor do título.</param>
    public static void Pintar(Window janela, string? fundo, string? texto)
    {
        if (LerCor(fundo) is int corFundo && LerCor(texto) is int corTexto)
        {
            Definir(janela, AtributoCorFundo, corFundo);
            Definir(janela, AtributoCorTexto, corTexto);
        }
    }

    /// <summary>
    /// Converte <c>#rrggbb</c> para o formato de cor do Windows (COLORREF, 0x00BBGGRR).
    /// </summary>
    /// <param name="cor">Texto da cor.</param>
    /// <returns>A cor convertida, ou <see langword="null"/> se o formato não for <c>#rrggbb</c>.</returns>
    private static int? LerCor(string? cor)
    {
        cor = cor?.Trim();
        if (cor is not { Length: 7 } || cor[0] != '#'
            || !int.TryParse(cor.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var rgb))
        {
            return null;
        }

        var vermelho = (rgb >> 16) & 0xFF;
        var verde = (rgb >> 8) & 0xFF;
        var azul = rgb & 0xFF;
        return vermelho | (verde << 8) | (azul << 16);
    }

    /// <summary>
    /// Grava um atributo da janela no DWM, ignorando a recusa: atributo não suportado deixa a barra como está.
    /// </summary>
    /// <param name="janela">Janela alvo.</param>
    /// <param name="atributo">Número do atributo do DWM.</param>
    /// <param name="valor">Valor do atributo.</param>
    private static void Definir(Window janela, int atributo, int valor)
    {
        var alca = new WindowInteropHelper(janela).Handle;
        if (alca != 0)
        {
            _ = DwmSetWindowAttribute(alca, atributo, ref valor, sizeof(int));
        }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint janela, int atributo, ref int valor, int tamanho);
}
