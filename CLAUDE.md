# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Jungle is a from-scratch spreadsheet app in F#, following Tomas Petricek's
"Write Your Own Excel" (https://tomasp.net/blog/2018/write-your-own-excel/).
AJ is new to F# and Elm/Elmish; the working mode is Claude explains
concepts and reviews, AJ writes the code. Don't write implementation code
for the spreadsheet logic or UI unless explicitly asked to — see
`src/Jungle.App/plans/jungle-spreadsheet-plan.md` for the staged
learning roadmap and current milestone.

## Commands

```
dotnet build Jungle.slnx              # build everything
dotnet test Jungle.slnx                # run all tests
dotnet test --filter "FullyQualifiedName~<name>"   # run a single test
dotnet run --project src/Jungle.App    # run the app
```

## Architecture

Currently a plain console app scaffold (`src/Jungle.App`, .NET 10, F#)
with an xUnit test project (`tests/Jungle.Tests`) referencing it. Per the
roadmap, this is planned to become a Fable + Elmish + Feliz browser app
(compiled F# → JS, served via Vite) — that tooling isn't set up yet.

Compile order matters in `Jungle.App.fsproj` (F# compiles top-to-bottom,
no forward references): `Domain.fs` → `Parser.fs` → `Evaluator.fs` →
`Sheet.fs` → `Program.fs`. Keep this order when adding files, and add new
modules to the `.fsproj` `<Compile>` list explicitly — F# projects don't
auto-glob source files.

Module responsibilities (currently stubs, `failwith "not implemented"`):
- `Domain.fs` — core types: `Address`, `Value`, `Expr`. No dependencies on
  other project modules.
- `Parser.fs` — formula string → `Expr`.
- `Evaluator.fs` — `Expr` → `Value`. Takes a `lookup: Address -> Value`
  function parameter rather than depending on `Sheet` directly, so it
  stays decoupled from how cell storage works.
- `Sheet.fs` — sheet state (`Map<Address, Expr>`) and get/set operations.

Once the Fable/Elmish milestones land, `Domain.fs`/`Parser.fs`/
`Evaluator.fs`/`Sheet.fs` are meant to stay Fable-agnostic (no UI
dependency) so they keep unit-testing under plain .NET via
`Jungle.Tests` even though the app itself compiles to JS. UI-specific
Elmish code (state/update/view) lives in separate files layered on top.
