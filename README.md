# Prolog.NET

Prolog.NET is a .NET library suite for embedding and interacting with SWI-Prolog from C#. It provides a low-level P/Invoke engine wrapper, an actor-based layer for cross-process queries, and a schema-first DSL for constructing typed Prolog databases in C# and serializing them to `.pl` files.

## Packages

| Project | Description | README |
|---------|-------------|--------|
| `Prolog.NET.Swipl` | Thin P/Invoke wrapper around the SWI-Prolog native library; marshals all calls to a dedicated background thread | [README](src/Prolog.NET.Swipl/README.md) |
| `Prolog.NET.Model` | Schema-first DSL for constructing typed Prolog databases and serializing them to `.pl` | [README](src/Prolog.NET.Model/README.md) |
| `Prolog.NET.Model.Generator` | Roslyn incremental source generator that emits strongly-typed factory methods from attribute declarations | [README](src/Prolog.NET.Model.Generator/README.md) |

## Internal Tooling

The following projects are part of the solution but are not distributed as packages:

- **`Prolog.NET.Console`** — Interactive CLI host for driving worker processes.
- **`Prolog.NET.Worker`** — Headless worker executable; one instance per loaded knowledge base.
- **`Prolog.NET.Designer`** — Console playground for experimenting with the model DSL.

## Solution Structure

All source lives under `src/`. Tests live under `tests/`. The solution file is `Prolog.NET.slnx`.

## Tests

| Project | Description |
|---------|-------------|
| `Prolog.NET.Model.Tests` | xUnit tests for the model DSL and serializer; includes end-to-end serialization tests matching the README examples |

## Prerequisites

- **.NET 10.0** or later
- **SWI-Prolog** must be installed and its native library discoverable at runtime for projects that depend on `Prolog.NET.Swipl` (`Swipl` and `Actors`). The `Model` and `Model.Generator` packages have no native dependency.

## Development Notes

This project was developed with the assistance of AI coding tools (Claude Code), under the supervision of the author. All design decisions, architecture choices, and code reviews were made by the author.
