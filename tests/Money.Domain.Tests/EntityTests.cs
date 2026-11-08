using Money.Domain.Common;

namespace Money.Domain.Tests;

public class EntityTests
{
    private sealed class EntidadeFake : Entity;

    [Fact]
    public void NovaEntidade_TemIdUnicoEPreenchido()
    {
        var a = new EntidadeFake();
        var b = new EntidadeFake();

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }
}
