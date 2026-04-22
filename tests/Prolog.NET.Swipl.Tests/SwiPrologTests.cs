using System.Text;
using Prolog.NET.Swipl.C;

namespace Prolog.NET.Swipl.Tests;

public unsafe class SwiPrologTests
{
    [Fact]
    public void PrologBasicLifecycleTest()
    {
        // Arrange
        if (OperatingSystem.IsLinux())
        {
            Environment.SetEnvironmentVariable("SWI_HOME_DIR", "/usr/bin/swipl");
        }
        string firstArgument = "swipl";
        byte[] argumentBytes = Encoding.UTF8.GetBytes(firstArgument + "\0");
        int argc = 1; // Argument count

        // Act
        bool initialised;
        fixed (byte* argv0 = argumentBytes)
        {
            byte** argv = stackalloc byte*[argc]; // Arguments vector
            argv[0] = argv0;
            initialised = SwiProlog.PL_initialise(argc, argv);
        }

        int outArgc;
        byte** outArgv;
        bool confirm = SwiProlog.PL_is_initialised(&outArgc, &outArgv);
        PL_CLEANUP_RESULT cleanup = SwiProlog.PL_cleanup(PL_CLEANUP_STATUS_AND_FLAGS.PL_CLEANUP_NO_CANCEL);

        // Assert
        Assert.True(initialised);
        Assert.True(confirm);
        Assert.Equal(PL_CLEANUP_RESULT.PL_CLEANUP_SUCCESS, cleanup);
    }
}
