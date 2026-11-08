using Money.Domain.Comum;

namespace Money.Domain.Tests;

public class EntidadeTestes
{
    private sealed class EntidadeFake : Entidade;

    [Fact]
    public void NovaEntidade_TemIdUnicoEPreenchido()
    {
        var a = new EntidadeFake();
        var b = new EntidadeFake();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }
}
