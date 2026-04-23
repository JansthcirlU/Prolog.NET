using Prolog.NET.Swipl.C;

namespace Prolog.NET.Swipl.Tests;

public sealed unsafe partial class SwiPrologTests : IClassFixture<SwiPrologTestFixture>
{
    private readonly SwiPrologTestFixture _swiPrologTestFixture;

    public SwiPrologTests(SwiPrologTestFixture swiPrologTestFixture)
    {
        _swiPrologTestFixture = swiPrologTestFixture;
    }

    [Fact]
    public void IsInitialised_WhenInsideFixture_ShouldBeTrue()
    {
        // Arrange & act
        int outArgc;
        byte** outArgv;
        bool initialised = SwiProlog.PL_is_initialised(&outArgc, &outArgv);

        // Assert
        Assert.True(initialised);
        Assert.True(outArgc > 0);
        Assert.True(outArgv != null);
    }
}
