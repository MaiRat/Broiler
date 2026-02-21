# ADR-002: Use YantraJS for JavaScript Execution

## Status

Accepted

## Context

Broiler needs a JavaScript engine to execute scripts embedded in or referenced by HTML pages. Options include V8 via interop, Jint, Jurassic, or YantraJS.

## Decision

Use [YantraJS](https://github.com/yantrajs/yantra) (v1.2.295) as the JavaScript engine.

## Rationale

- **ES2020+ support**: Handles modern JavaScript features including async/await, generators, modules
- **.NET Standard 2.0/2.1**: Compatible with .NET Framework and modern .NET
- **C# interop**: Easy bridging between .NET objects and JavaScript via CLR integration
- **Async support**: Built-in event loop and promise handling via `ExecuteAsync`
- **Expression compiler**: Compiles JavaScript to .NET expressions for performance

## Consequences

- Full browser API compatibility is not built-in (window, document, etc. must be bridged manually)
- DOM manipulation requires custom bindings between html-renderer's object model and YantraJS
- Performance characteristics differ from native V8 engine
