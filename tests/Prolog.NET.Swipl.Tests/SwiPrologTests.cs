using System.Diagnostics;
using Prolog.NET.Swipl.C;

namespace Prolog.NET.Swipl.Tests;

public sealed class SwiPrologTests : IClassFixture<SwiPrologTestFixture>
{
    private readonly SwiPrologTestFixture _swiPrologTestFixture;

    public SwiPrologTests(SwiPrologTestFixture swiPrologTestFixture)
    {
        _swiPrologTestFixture = swiPrologTestFixture;
    }

    [Fact]
    public unsafe void IsInitialised_WhenInsideFixture_ShouldBeTrue()
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

    [Fact]
    public async Task EngineLifecycle_WhenSingleThreaded_ShouldSucceed()
    {
        // Create engine
        PL_engine_t e = SwiProlog.PL_create_engine(0);
        Assert.NotEqual(0, e.handle);
        int engineThread = SwiProlog.PL_thread_self();
        Assert.NotEqual(-1, engineThread);

        // Attach the engine
        PL_ENGINE_RESULT attached = SwiProlog.PL_set_engine(e, out PL_engine_t previous);
        Assert.NotEqual(0, previous.handle);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, attached);

        // Detach the engine
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(e, 0);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, detached);

        // Destroy the engine
        bool destroyed = SwiProlog.PL_destroy_engine(e);
        Assert.True(destroyed);
        int noThread = SwiProlog.PL_thread_self();
        Assert.Equal(-1, noThread);
    }
}
