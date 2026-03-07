namespace Prolog.NET.Swipl;

// Query open flags (for PL_open_query)
// Source: SWI-Prolog.h
internal static class PrologNativeConstants
{
    internal const int PL_Q_NORMAL = 0x0002;
    internal const int PL_Q_NODEBUG = 0x0004;
    internal const int PL_Q_CATCH_EXCEPTION = 0x0008;
    internal const int PL_Q_PASS_EXCEPTION = 0x0010;
    internal const int PL_Q_EXT_STATUS = 0x0040;

    // Return values from PL_next_solution when PL_Q_EXT_STATUS is set
    internal const int PL_S_EXCEPTION = -1;
    internal const int PL_S_FALSE = 0;
    internal const int PL_S_TRUE = 1;
    internal const int PL_S_LAST = 2;
}
