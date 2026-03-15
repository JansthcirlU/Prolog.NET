# Possibly relevant documentation for multithreaded apps

## 10.6 Multithreaded mixed C and Prolog applications

All foreign code linked to the multithreading version of SWI-Prolog should be thread-safe (reentrant) or guarded in Prolog using with_mutex/2 from simultaneous access from multiple Prolog threads. If you want to write mixed multithreaded C and Prolog applications you should first familiarise yourself with writing multithreaded applications in C (C++).

If you are using SWI-Prolog as an embedded engine in a multithreaded application you can access the Prolog engine from multiple threads by creating an engine in each thread from which you call Prolog. Without creating an engine, a thread can only use functions that do not use the term_t type (for example PL_new_atom()).

The system supports two models. Section 10.6.1 describes the original one-to-one mapping. In this schema a native thread attaches a Prolog thread if it needs to call Prolog and detaches it when finished, as opposed to the model from section 10.6.2, where threads temporarily use a Prolog engine. 

### 10.6.1 A Prolog thread for each native thread (one-to-one)

In the one-to-one model, the thread that called PL_initialise() has a Prolog engine attached. If another C thread in the system wishes to call Prolog it must first attach an engine using PL_thread_attach_engine() and call PL_thread_destroy_engine() after all Prolog work is finished. This model is especially suitable with long running threads that need to do Prolog work regularly. See section 10.6.2 for the alternative many-to-many model. 

#### `int PL_thread_self()`

Returns the integer Prolog identifier of the engine or -1 if the calling thread has no Prolog engine. This function is also provided in the single-threaded version of SWI-Prolog, where it returns -2.

#### `int PL_unify_thread_id(term_t t, int i)`

Unify t with the Prolog thread identifier for thread i. Thread identifiers are normally returned from PL_thread_self(). Returns -1 if the thread does not exist or the unification fails.

#### `int PL_thread_attach_engine(const PL_thread_attr_t *attr)`

Creates a new Prolog engine in the calling thread. If the calling thread already has an engine the reference count of the engine is incremented. The attr argument can be NULL to create a thread with default attributes. Otherwise it is a pointer to a structure as defined below. The structure must be fully initialized, including hidden fields. For any field with value‘0’, the default is used. The cancel field may be filled with a pointer to a function that is called when PL_cleanup() terminates the running Prolog engines. The function is called with the thread id (see PL_thread_self()) as argument and must return PL_THREAD_CANCEL_JOINED if the thread was reclaimed successfully, PL_THREAD_CANCEL_MUST_JOIN if the thread as cancelled, but must still be joined or PL_THREAD_CANCEL_FAILED if the request cannot be honoured. If this function is not present or returns PL_THREAD_CANCEL_FAILED pthread_cancel() is used. The new thread inherits is properties from Prolog's main thread. The flags field defines the following flags: 

| Flag name | Description |
| --- | --- |
| `PL_THREAD_NO_DEBUG` | If this flag is present, the thread starts in normal no-debug status. By default, the debug status is inherited from the main thread. |
| `PL_THREAD_NOT_DETACHED`  By default the new thread is created in detached mode. With this flag it is created normally, allowing Prolog to join the thread. |
| `PL_THREAD_CUR_STREAMS` | By default the current_input and current_output are set to user_input and user_output of the main thread. Using this flag, these streams are copied from the main thread. See also the inherited_from option of thread_create/3. |

```c
typedef struct
{ size_t    stack_limit;                /* Total stack limit (bytes) */
size_t    table_space;                /* Total tabling space limit (bytes) */
char *    alias;                      /* alias name */
int       (*cancel)(int thread);      /* cancel function */
intptr_t  flags;                      /* PL_THREAD_* flags */
size_t    max_queue_size;             /* Max size of associated queue */
char *    thread_class;               /* Class property of the thread */
} PL_thread_attr_t;
```

The structure may be destroyed after PL_thread_attach_engine() has returned. On success it returns the Prolog identifier for the thread (as returned by PL_thread_self()). If an error occurs, -1 is returned. If this Prolog is not compiled for multithreading, -2 is returned.

#### `bool PL_thread_destroy_engine()`

Destroy the Prolog engine in the calling thread. Only takes effect if PL_thread_destroy_engine() is called as many times as PL_thread_attach_engine() in this thread. Returns TRUE on success and FALSE if the calling thread has no engine or this Prolog does not support threads.

Please note that construction and destruction of engines are relatively expensive operations. Only destroy an engine if performance is not critical and memory is a critical resource.

#### `bool PL_thread_at_exit(void (*function)(void *), void *closure, bool global)`

Register a handle to be called as the Prolog engine is destroyed. The handler function is called with one void * argument holding closure. If global is true, the handler is installed for all threads. Globally installed handlers are executed after the thread-local handlers. If the handler is installed local for the current thread only (global == false) it is stored in the same FIFO queue as used by thread_at_exit/1.

### 10.6.2 Using Prolog engines from C

Prolog engines live as entities that are independent from threads. They are always supported in the multi-threaded version and may be enabled in the single threaded version by providing -DENGINES=ON during the cmake configuration. Multiple threads may use a pool of engines for handling calls to Prolog. A single thread may use multiple engines to achieve coroutining. Engines are suitable in the following identified cases: 

| Use case | Description |
| -------- | ----------- |
| *Many native threads with infrequent Prolog work* | Prolog threads are expensive in terms of memory and time to create and destroy them. For systems that use a large number of threads that only infrequently need to call Prolog, it is better to take an engine from a pool and return it there. |
| *Prolog status must be handed to another thread* | This situation has been identified by Uwe Lesta when creating a .NET interface for SWI-Prolog. .NET distributes work for an active internet connection over a pool of threads. If a Prolog engine contains the state for a connection, it must be possible to detach the engine from a thread and re-attach it to another thread handling the same connection. |
| *Achieving coroutines* | A single thread may use engines to implement coroutining. This is notably interesting when combined with yielding as described in section 12.4.1.2. |

#### `PL_engine_t PL_current_engine()`

Returns the current engine of the calling thread or NULL if the thread has no Prolog engine.

#### `PL_engine_t PL_create_engine(PL_thread_attr_t *attributes)`

Create a new Prolog engine. attributes is described with PL_thread_attach_engine(). Any thread can make this call after PL_initialise() returns success. The returned engine is not attached to any thread and lives until PL_destroy_engine() is used on the returned handle.

In the single-threaded version this call always returns NULL, indicating failure.

#### `bool PL_destroy_engine(PL_engine_t e)`

Destroy the given engine. Destroying an engine is only allowed if the engine is not attached to any thread or attached to the calling thread. On success this function returns TRUE, on failure the return value is FALSE.

#### `int PL_set_engine(PL_engine_t engine, PL_engine_t *old)`

Make the calling thread ready to use engine. If old is non-NULL the current engine associated with the calling thread is stored at the given location. If engine equals PL_ENGINE_MAIN the initial engine is attached to the calling thread. If engine is PL_ENGINE_CURRENT the engine is not changed. This can be used to query the current engine. This call returns PL_ENGINE_SET if the engine was switched successfully, PL_ENGINE_INVAL if engine is not a valid engine handle and PL_ENGINE_INUSE if the engine is currently in use by another thread.

Engines can be changed at any time. For example, it is allowed to select an engine to initiate a Prolog goal, detach it and at a later moment execute the goal from another thread. Note, however, that the term_t, qid_t and fid_t types are interpreted relative to the engine for which they are created. Behaviour when passing one of these types from one engine to another is undefined. The engine to which a query belongs can be requested using PL_query_engine()

In versions that do not support engines this call only succeeds if engine refers to the main engine.

#### `void PL_WITH_ENGINE(PL_engine_t e)`

This macro implements a C for-loop where the body is executed with the engine e as current engine. The body is executed exactly once. After completion of the body the current engine of the calling thread is restored to old state (either the old current engine or no engine). The user may use break to terminate the body early. The user may not use return. Using return does not reset the old engine.

### 12.4.1.2 Yielding from foreign predicates

Starting with SWI-Prolog 8.5.5 we provide an experimental interface that allows using a SWI-Prolog engine for asynchronous processing. The idea is that an engine that calls a foreign predicate which would need to block may be suspended and later resumed. For example, consider an application that listens to a large number of network connections (sockets). SWI-Prolog offers three scenarios to deal with this:

1. Using a thread per connection. This model fits Prolog well as it allows to keep state in e.g. a DCG using phrase_from_stream/2. Maintaining an operating system thread per connection uses a significant amount of resources though.

2. Using wait_for_input/3 a single thread can wait for many connections. Each time input arrives we must associate this with a state engine and advance this engine using a chunk of input of unknown size. Programming a state engine in Prolog is typically a tedious job. Although we can use delimited continuations (see section 4.9) in some scenarios this is not a universal solution.

3. Using the primitives from this section we can create an engine (see PL_engine_create()) to handle a connection with the same benefits as using threads. When the engine calls a foreign predicate that would need to block it calls PL_yield_address() to suspend the engine. An overall scheduler watches for ready connections and calls PL_next_solution() to resume the suspended engine. This approach allows processing many connections on the same operating system thread.

As is, these features can only used through the foreign language interface. It was added after discussion with with Mattijs van Otterdijk aiming for using SWI-Prolog together with Rust's asynchronous programming support. Note that this feature is related to the engine API as described in section 11. It is different though. Where the Prolog engine API allows for communicating with a Prolog engine, the facilities of this section merely allow an engine to suspend, to be resumed later.

To prepare a query for asynchronous usage we first create an engine using PL_create_engine(). Next, we create a query in the engine using PL_open_query() with the flags PL_Q_ALLOW_YIELD and PL_Q_EXT_STATUS. A foreign predicate that needs to be capable of suspending must be registered using PL_register_foreign() and the flags PL_FA_VARARGS and PL_FA_NONDETERMINISTIC; i.e., only non-det predicates can yield. This is no restriction as non-det predicate can always return TRUE to indicate deterministic success. Finally, PL_yield_address() allows the predicate to yield control, preparing to resume similar to PL_retry_address() does for non-deterministic results. PL_next_solution() returns PL_S_YIELD if a predicate calls PL_yield_address() and may be resumed by calling PL_next_solution() using the same query id (qid). We illustrate the above using some example fragments.

First, let us create a predicate that can read the available input from a Prolog stream and yield if it would block. Note that our predicate must the PL_FA_VARARGS interface, which implies the first argument is in a0, the second in a0+1, etc.

```c
/** read_or_block(+Stream, -String) is det.
*/

#define BUFSIZE 4096

static foreign_t
read_or_block(term_t a0, int arity, void *context)
{ IOSTREAM *s;

switch(PL_foreign_control(context))
{ case PL_FIRST_CALL:
    if ( PL_get_stream(a0, &s, SIO_INPUT) )
    { Sset_timeout(s, 0);
    break;
    }
    return FALSE;
case PL_RESUME:
    s = PL_foreign_context_address(context);
    break;
case PL_PRUNED:
    return PL_release_stream(s);
default:
    assert(0);
    return FALSE;
}

char buf[BUFSIZE];

size_t n = Sfread(buf, sizeof buf[0], sizeof buf / sizeof buf[0], s);
if ( n == 0 )                     // timeout or error
{ if ( (s->flags&SIO_TIMEOUT) )
    PL_yield_address(s);        // timeout: yield
else
    return PL_release_stream(s);  // raise error
} else
{ return ( PL_release_stream(s) &&
            PL_unify_chars(a0+1, PL_STRING|REP_ISO_LATIN_1, n, buf) );
}
}
```

This function must be registered using PL_register_foreign():

```c
PL_register_foreign("read_or_block", 2, read_or_block,
                    PL_FA_VARARGS|PL_FA_NONDETERMINISTIC);
```

Next, create an engine to run handle_connection/1 on a Prolog stream. Note that we omitted most of the error checking for readability. Also note that we must make our engine current using PL_set_engine() before we can interact with it. 

```c
qid_t
start_connection(IOSTREAM *c)
{ predicate_t p = PL_predicate("handle_connection", 1, "user");

PL_engine_t e = PL_create_engine(NULL);
qid_t q = NULL;
PL_WITH_ENGINE(e)
{ term_t av = PL_new_term_refs(1);
PL_unify_stream(av+0, c);
q = PL_open_query(e, NULL,
                    PL_Q_CATCH_EXCEPTION|
                    PL_Q_ALLOW_YIELD|
                    PL_Q_EXT_STATUS,
                    p, av);
}
return q;
}
```

Finally, our foreign code must manage this engine. Normally it will do so together with many other engines. First, we write a function that runs a query in the engine to which it belongs.

```c
int
PL_engine_next_solution(qid_t qid)
{ int rc = FALSE;

PL_WITH_ENGINE(PL_query_engine(qid))
{ rc = PL_next_solution(qid);
}

return rc;
}
```

Now we can simply handle a connection using the loop below which restarts the query as long as it yields. Realistic code manages multiple queries and will (in this case) use the POSIX poll() or select() interfaces to activate the next query that can continue without blocking. 

```c
int rc;
do
{ rc = PL_engine_next_solution(qid);
} while( rc == PL_S_YIELD );
```

After the query completes it must be closed using PL_close_query() or PL_cut_query(). The engine may be destroyed using PL_engine_destroy() or reused for a new query.

#### `(return) foreign_t PL_yield_address(void *)`

Cause PL_next_solution() of the active query to return with PL_S_YIELD. A subsequent call to PL_next_solution() on the same query calls the foreign predicate again with the control status set to PL_RESUME, after which PL_foreign_context_address() retrieves the address passed to this function. The state of the Prolog engine is maintained, including term_t handles. If the passed address needs to be invalidated the predicate must do so when returning either TRUE or FALSE. If the engine terminates the predicate the predicate is called with status PL_PRUNED, in which case the predicate must cleanup.

#### `bool PL_can_yield(void)`

Returns TRUE when called from inside a foreign predicate if the query that (indirectly) calls this foreign predicate can yield using PL_yield_address(). Returns FALSE when either there is no current query or the query cannot yield.

#### Discussion

Asynchronous processing has become popular with modern programming languages, especially those aiming at network communication. Asynchronous processing uses fewer resources than threads while avoiding most of the complications associated with thread synchronization if only a single thread is used to manage the various states. The lack of good support for destructive state updates in Prolog makes it attractive to use threads for dealing with multiple inputs. The fact that Prolog discourages using shared global data such as dynamic predicates typically makes multithreaded code easy to manage.

It is not clear how much scalability we gain using Prolog engines instead of Prolog threads. The only difference between the two is the operating system task. Prolog engines are still rather memory intensive, mainly depending on the stack sizes. Global garbage collection (atoms and clauses) need to process all the stacks of all the engines and thus limit scalability.

One possible future direction is to allow all (possibly) blocking Prolog predicates to use the yield facility and provide a Prolog API to manage sets of engines that use this type of yielding. As is, these features are designed to allow SWI-Prolog for cooperating with languages that provide asynchronous functions. 