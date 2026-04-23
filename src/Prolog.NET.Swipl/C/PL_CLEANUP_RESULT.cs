namespace Prolog.NET.Swipl.C;

internal enum PL_CLEANUP_RESULT
{
    PL_CLEANUP_RECURSIVE = -2,
    PL_CLEANUP_FAILED = -1,
    PL_CLEANUP_CANCELED = 0,
    PL_CLEANUP_SUCCESS = 1
}
