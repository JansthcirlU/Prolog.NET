namespace Prolog.NET.Swipl.C;

[Flags]
internal enum PL_Q_FLAGS
{
    PL_Q_NORMAL = 0x0002, /* debug/print exceptions, pass bool */
    PL_Q_NODEBUG = 0x0004, /* Run in nodebug mode */
    PL_Q_CATCH_EXCEPTION = 0x0008, /* handle exceptions in C */
    PL_Q_PASS_EXCEPTION = 0x0010, /* pass to parent environment */
    PL_Q_ALLOW_YIELD = 0x0020, /* Support I_YIELD */
    PL_Q_EXT_STATUS = 0x0040, /* Return extended status */
    PL_Q_EXCEPT_HALT = 0x0080, /* Handles unwind(halt(Status)) */
    PL_Q_TRACE_WITH_YIELD = 0x0100, /* Yield for debug actions */
}
