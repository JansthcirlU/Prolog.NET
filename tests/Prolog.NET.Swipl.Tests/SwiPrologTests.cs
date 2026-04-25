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
    // Engine test: must call PL_destroy_engine at the end to ensure correct fixture disposal
    public void EngineLifecycle_WhenSingleThreaded_ShouldSucceed()
    {
        // Create engine
        PL_engine_t e = SwiProlog.PL_create_engine(0);
        Assert.NotEqual(0, e.handle);

        // Attach the engine (thread should be set)
        PL_ENGINE_RESULT attached = SwiProlog.PL_set_engine(e, out PL_engine_t _);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, attached);
        int engineThread = SwiProlog.PL_thread_self();
        Assert.NotEqual(-1, engineThread);

        // Detach the engine
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(e, 0);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, detached);

        // Destroy the engine (thread should be reset to -1)
        bool destroyed = SwiProlog.PL_destroy_engine(e);
        Assert.True(destroyed);
        int noThread = SwiProlog.PL_thread_self();
        Assert.Equal(-1, noThread);
    }

    [Fact]
    // Engine test: must call PL_destroy_engine at the end to ensure correct fixture disposal
    public void CreatedEngine_WhenDestroyedAfterSet_ShouldSucceed()
    {
        // Create engine
        PL_engine_t e = SwiProlog.PL_create_engine(0);
        Assert.NotEqual(0, e.handle);

        // Attach the engine (thread should be set)
        PL_ENGINE_RESULT attached = SwiProlog.PL_set_engine(e, out PL_engine_t _);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, attached);
        int engineThread = SwiProlog.PL_thread_self();
        Assert.NotEqual(-1, engineThread);

        // Destroy the engine while attached (thread should be reset to -1)
        bool destroyed = SwiProlog.PL_destroy_engine(e);
        Assert.True(destroyed);
        int noThread = SwiProlog.PL_thread_self();
        Assert.Equal(-1, noThread);

        // Detaching the engine should fail
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(e, out PL_engine_t destroyedEngine);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_INVAL, detached);
        Assert.Equal(0, destroyedEngine.handle);
    }

    [Fact]
    // Engine test: must call PL_destroy_engine at the end to ensure correct fixture disposal
    public void TwoEngines_WhenSecondIsSet_ShouldReturnFirstHandle()
    {
        // Create and set first engine
        PL_engine_t firstEngine = SwiProlog.PL_create_engine(0);
        Assert.NotEqual(0, firstEngine.handle);
        PL_ENGINE_RESULT firstAttached = SwiProlog.PL_set_engine(firstEngine, out PL_engine_t _);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, firstAttached);

        // Create and set second engine
        PL_engine_t secondEngine = SwiProlog.PL_create_engine(0);
        Assert.NotEqual(0, secondEngine.handle);
        Assert.NotEqual(firstEngine.handle, secondEngine.handle);
        PL_ENGINE_RESULT secondAttached = SwiProlog.PL_set_engine(secondEngine, out PL_engine_t firstEngineAgain);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, secondAttached);

        // Check if the detached engine is equal to the first one
        Assert.Equal(firstEngine.handle, firstEngineAgain.handle);

        // Destroy both engines created during this test
        Assert.True(SwiProlog.PL_destroy_engine(firstEngine));
        Assert.True(SwiProlog.PL_destroy_engine(secondEngine));
    }
}
