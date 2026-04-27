namespace Prolog.NET.Swipl.C;

[Flags]
internal enum PL_Q_EXT_STATUS
{
    PL_S_NOT_INNER = 2, /* Query is not inner query */
    PL_S_EXCEPTION = 1, /* Query raised exception */
    PL_S_FALSE = 0, /* Query failed (=false) */
    PL_S_TRUE = 1, /* Query succeeded with choicepoint (=true)*/
    PL_S_LAST = 2, /* Query succeeded without CP */
    PL_S_YIELD_DEBUG = 254, /* Yield on behalf of the debugger */
    PL_S_YIELD = 255, /* Foreign yield */
}
