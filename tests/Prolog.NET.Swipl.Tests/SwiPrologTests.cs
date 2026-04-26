using Prolog.NET.Swipl.C;
using Prolog.NET.Threading;

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
    public void MainEngine_ShouldNotBeDestroyed()
    {
        Assert.False(SwiProlog.PL_destroy_engine(PL_engine_t.PL_ENGINE_MAIN));
    }

    [Fact]
    // This test creates an engine: must call PL_destroy_engine at the end to ensure correct fixture disposal
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
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(PL_engine_t.NULL, 0);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, detached);

        // Destroy the engine (thread should be reset to -1)
        bool destroyed = SwiProlog.PL_destroy_engine(e);
        Assert.True(destroyed);
        int noThread = SwiProlog.PL_thread_self();
        Assert.Equal(-1, noThread);
    }

    [Fact]
    // This test creates an engine: must call PL_destroy_engine at the end to ensure correct fixture disposal
    public void CreatedEngine_WhenDestroyed_ShouldNotSet()
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

        // Setting the engine again should fail
        PL_ENGINE_RESULT setAgain = SwiProlog.PL_set_engine(e, out PL_engine_t destroyedEngine);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_INVAL, setAgain);
        Assert.Equal(0, destroyedEngine.handle);

        // Detaching should still succeed
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(PL_engine_t.NULL, out _);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, detached);
    }

    [Fact]
    // This test creates an engine: must call PL_destroy_engine at the end to ensure correct fixture disposal
    public void CreatedEngine_WhenSetMultipleTimes_ShouldBeIdempotent()
    {
        // Create engine
        PL_engine_t e = SwiProlog.PL_create_engine(0);

        // Set engine for the first time
        PL_ENGINE_RESULT set1 = SwiProlog.PL_set_engine(e, out _);
        PL_ENGINE_RESULT set2 = SwiProlog.PL_set_engine(e, out PL_engine_t previous);

        // Detach and destroy engine
        PL_ENGINE_RESULT detached = SwiProlog.PL_set_engine(PL_engine_t.NULL, out PL_engine_t old);
        bool destroyed = SwiProlog.PL_destroy_engine(old);
        PL_engine_t current = SwiProlog.PL_current_engine();

        // Assert
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, set1);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, set2);
        Assert.Equal(e.handle, previous.handle);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, detached);
        Assert.Equal(e.handle, old.handle);
        Assert.True(destroyed);
        Assert.Equal(0, current.handle);
    }

    [Fact]
    // This test creates engines: must call PL_destroy_engine at the end to ensure correct fixture disposal
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

    [Fact]
    public async Task TwoEngines_WhenSettingFromOtherThread_ShouldBeInUse()
    {
        // Create and attach first engine on a dedicated thread
        ThreadWorker worker1 = new();
        TaskCompletionSource<(PL_engine_t, PL_ENGINE_RESULT, int)> worker1Job1 = new();
        worker1.AddJob(() =>
        {
            PL_engine_t e = SwiProlog.PL_create_engine(0);
            PL_ENGINE_RESULT attached = SwiProlog.PL_set_engine(e, out _);
            int tid = SwiProlog.PL_thread_self();
            worker1Job1.SetResult((e, attached, tid));
        });
        PL_engine_t worker1Engine = PL_engine_t.PL_ENGINE_NONE;
        PL_ENGINE_RESULT worker1Attached = PL_ENGINE_RESULT.PL_ENGINE_INVAL;
        int worker1Thread = -1;
        (worker1Engine, worker1Attached, worker1Thread) = await worker1Job1.Task;

        // Try to attach and destroy the first engine from a different thread
        ThreadWorker worker2 = new();
        PL_ENGINE_RESULT worker2Attached = PL_ENGINE_RESULT.PL_ENGINE_SET;
        bool worker2Destroyed = true;
        int worker2Thread = 0xCAFE;
        worker2.AddJob(() =>
        {
            worker2Attached = SwiProlog.PL_set_engine(worker1Engine, out _);
            worker2Thread = SwiProlog.PL_thread_self();
            worker2Destroyed = SwiProlog.PL_destroy_engine(worker1Engine);
        });
        worker2.Dispose();

        // Detach and destroy first engine to ensure proper cleanup
        PL_ENGINE_RESULT worker1Detached = PL_ENGINE_RESULT.PL_ENGINE_INVAL;
        bool worker1Destroyed = false;
        worker1.AddJob(() =>
        {
            worker1Detached = SwiProlog.PL_set_engine(PL_engine_t.NULL, out _);
            worker1Destroyed = SwiProlog.PL_destroy_engine(worker1Engine);
        });
        worker1.Dispose();

        //// Assertions
        
        // Worker 1 create and set
        Assert.NotEqual(-1, worker1Engine.handle);
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, worker1Attached);
        Assert.NotEqual(-1, worker1Thread);

        // Worker 2 set and destroy
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_INUSE, worker2Attached);
        Assert.False(worker2Destroyed);
        Assert.Equal(-1, worker2Thread);

        // Worker 1 detached and destroyed
        Assert.Equal(PL_ENGINE_RESULT.PL_ENGINE_SET, worker1Detached);
        Assert.True(worker1Destroyed);
    }
}
