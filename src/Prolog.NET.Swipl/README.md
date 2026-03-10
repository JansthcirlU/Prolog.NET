# Prolog.NET.Swipl

Thin P/Invoke wrapper around the SWI-Prolog native library. All SWI-Prolog calls are marshaled to a single dedicated background thread to satisfy SWI-Prolog's threading constraints.

## Prerequisites

SWI-Prolog must be installed and its native library must be discoverable at runtime (e.g. on `PATH` or `LD_LIBRARY_PATH`).

## Key Type

### `PrologEngine`

A singleton engine wrapper. Lifecycle:

```csharp
engine.Initialize();   // must be called once before use
// ...
engine.Dispose();      // shuts down the background thread
```

Key methods:

| Method | Description |
|--------|-------------|
| `Call(string goal)` | Assert a goal and return true/false |
| `OpenQuery(string goal)` | Open a lazy query; returns a query handle |
| `LoadFile(string path)` | Consult a `.pl` file |

## Dependency Injection

```csharp
services.AddPrologEngine();
// or, passing SWI-Prolog initialization arguments:
services.AddPrologEngine(new[] { "--stack-limit=256m" });
```

Extension method provided by `PrologEngineExtensions`.
