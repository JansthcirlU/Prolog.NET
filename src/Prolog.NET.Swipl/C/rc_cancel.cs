namespace Prolog.NET.Swipl.C;

internal enum rc_cancel : int
{
    PL_THREAD_CANCEL_FAILED = 0,
    PL_THREAD_CANCEL_JOINED = 1,
    PL_THREAD_CANCEL_MUST_JOIN = 2
}
