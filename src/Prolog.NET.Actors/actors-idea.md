# Architecting a concurrent, multi-reader, file-based Prolog database server

## General ideas

### Application and infrastructural side

The *server* is a host machine (emulated OS, VPS, cloud machine, some Raspberry Pi connected to the internet, ...) which contains a few Prolog files, this abstraction is purely infrastructural.
It runs a *Prolog server*, which is a program that manages *Prolog workers* to load said Prolog files in isolation using `swipl`, by limiting one file per *Prolog worker* (if possible, multiple *Prolog workers* may need to access the same file).
Each *Prolog worker* can then spawn one or more *Prolog actors* to manage concurrent queries, assuming multiple threads can query the same file within a *Prolog worker* at the same time.

### Consumer side

A *consumer* is any entity that connect to the hosting *server* and communicate with the hosted *Prolog server* using one of the supported *communication protocols* (such as REST or gRPC).
It should be able to query specific files (with an optional grace period to load the file first) and receive *messages* one by one.
Messages may fall into the *Prolog Worker* category, which includes solutions to queries, warnings, errors, and so on, or they may fall into the *Prolog Server* category which includes operational semantics and status notifications concerning the server rather than the Prolog processing itself.

## Current architecture and concerns

### What already exists today

There are already a few types that somewhat map onto the aforementioned application and infrastructural concepts:
- `PrologWorker` inside `Prolog.NET.Console` is equivalent to the *Prolog server* (confusing naming)
- `WorkerHost` inside `Prolog.NET.Worker` is equivalent to a *Prolog worker*
- `PrologActor` inside `Prolog.NET.Actor` is equivalent to a *Prolog actor*
- `CliActor` inside `Prolog.NET.Actor` is one communication protocol implementation to handle consumer/server interaction

### Why these abstractions don't work

The `CliActor` was originally created specifically to interface using the CLI for a locally running process, when it should be a completely external wrapper altogether.
Moreover, it is hard-coded to allow up to four processes to open and load files, when in reality this number likely needs to be bumped up or calculated based on the hosting server's resources.
The `WorkerHost` currently spawns a single `PrologActor` to handle all requests as if they come from a single reader.
In a multi-reader setting, actors should be orchestrated to allow for distinct reader queries:
- if two readers each send a query that has multiple answers, and each reader wants to stream the answers one-by-one, two actors will be needed
- for ephemeral queries that yield a single answer and immediately notify no further solutions exist, any idle actor will do
- for this reason, at least one idle actor should exist at all times for each *Prolog worker* to handle ephemeral queries and to enable load balancing
- enabling multiple concurrent streaming queries can be enabled by increasing the number of *Prolog actors* per *Prolog worker* or by increasing the number of *Prolog workers* as well (i.e. process isolation)
- once we understand the performance and throughput profiles of the average server, we can more accurately estimate how to dynamically increase or decrease the number of servers needed to handle API load (assuming each server runs one *Prolog server*)

### Questions to be answered before proceeding

1. given the current implementation of `PrologEngine` (inside `Prolog.NET.Swipl`), can multiple threads query the same loaded knowledge base?
2. given the current implementation of `PrologEngine`, can multiple processes load the same file?
3. given the current implementation of `WorkerHost`, is it possible to refactor it to spawn multiple actors dynamically?

## Feedback

Add your own thoughts and concerns here.