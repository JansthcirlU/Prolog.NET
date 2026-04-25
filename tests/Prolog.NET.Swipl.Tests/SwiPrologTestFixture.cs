using System.Diagnostics;
using System.Text;
using Prolog.NET.Swipl.C;

namespace Prolog.NET.Swipl.Tests;

public class SwiPrologTestFixture : IAsyncLifetime
{
    private PL_engine_t _fixtureEngine;
    
    public unsafe Task InitializeAsync()
    {
        // swipl check + Unix initialisation
        string? swipl = Utils.Which("swipl").FirstOrDefault();
        ArgumentNullException.ThrowIfNull(swipl);
        if (OperatingSystem.IsLinux())
        {
            Environment.SetEnvironmentVariable("SWI_HOME_DIR", swipl);
        }

        // Initialise SWI-Prolog with quiet option
        byte[] swiplArg = Encoding.UTF8.GetBytes("swipl\0");
        byte[] quietArg = Encoding.UTF8.GetBytes("-q\0");
        int argc = 2;
        fixed (byte* argv0 = swiplArg)
        fixed (byte* argv1 = quietArg)
        {
            byte** argv = stackalloc byte*[argc];
            argv[0] = argv0;
            argv[1] = argv1;
            bool initialise = SwiProlog.PL_initialise(argc, argv);
            Debug.Assert(initialise, "SWI-Prolog failed to initialise.");
        }
        _fixtureEngine = SwiProlog.PL_current_engine();
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        PL_engine_t e = SwiProlog.PL_current_engine();
        if (e.handle == 0)
        {
            SwiProlog.PL_set_engine(_fixtureEngine, 0);
        }
        PL_CLEANUP_RESULT cleanup = SwiProlog.PL_cleanup(PL_CLEANUP_STATUS_AND_FLAGS.PL_CLEANUP_NO_CANCEL);
        Debug.Assert(cleanup == PL_CLEANUP_RESULT.PL_CLEANUP_SUCCESS, "SWI-Prolog cleanup was not successful.");
        return Task.CompletedTask;
    }
}
