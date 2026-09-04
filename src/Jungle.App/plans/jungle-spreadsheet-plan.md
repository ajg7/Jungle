# Jungle Spreadsheet — Learning Roadmap

Based on: [Write Your Own Excel](https://tomasp.net/blog/2018/write-your-own-excel/) by Tomas Petricek.

## Purpose

AJ is new to F# and the Elm architecture. This doc is a staged roadmap, not
an execution plan for an agent — **AJ writes the code, Claude explains
concepts and reviews**. Each milestone below lists what to learn, what
"done" looks like, and a couple of guiding questions. No milestone hands
over finished code; that defeats the point.

## Stack

The blog post (2018) uses Fable + `elmish-react` + Webpack. We're using the
same core ideas with a modern toolchain:

- **F#** compiled to JS via **Fable**
- **Elmish** for the Model-Update-View loop
- **Feliz** for the React DSL (nicer than raw `elmish-react` bindings)
- **Vite** for dev server/bundling (replaces Webpack config)

## Project layout (target state)

```
src/Jungle.App/
  Domain.fs      -- Address, Value, Expr, Sheet types (Fable-agnostic)
  Parser.fs       -- formula string -> Expr
  Evaluator.fs    -- Expr -> Value, circular-ref detection
  Sheet.fs        -- sheet state helpers (get/set cell, recalculation)
  Types.fs        -- Elmish State + Msg
  Update.fs       -- Elmish update function
  View.fs         -- Feliz/React rendering
  App.fs          -- Program.mkSimple wiring, entry point
  Jungle.App.fsproj
  package.json / vite.config.mts / index.html   -- Fable/JS tooling
tests/Jungle.Tests/
  -- unit tests for Domain/Parser/Evaluator/Sheet (pure F#, no Fable needed)
```

`Domain.fs`, `Parser.fs`, `Evaluator.fs`, `Sheet.fs` stay plain F# with no
UI dependency, so they keep compiling and testing under plain .NET (via
`Jungle.Tests`) even though the app itself compiles to JS via Fable.

## How each milestone works

For each milestone:
1. Claude explains the concept(s) involved and what the code needs to
   accomplish — not the code itself.
2. AJ implements it.
3. Claude reviews, explains any bugs/idioms, and only shows a code
   correction if AJ is stuck or asks for it directly.
4. Move to the next milestone once the current one builds/runs/passes.

---

## Milestone 1 — Toolchain setup

**Goal:** get a blank page rendering "Hello, Jungle" through Fable +
Elmish + Feliz + Vite, replacing the console app's role as the entry
point.

**Concepts:** what Fable does (F# → JS compiler), how a Fable project is
wired to npm/Vite, the minimal Elmish bootstrap (`Program.mkSimple`).

**Done when:** `npm run dev` (or equivalent) serves a page in the browser
showing static text produced by F# code.

**Guiding questions:**
- What does `Jungle.App.fsproj` need so `dotnet fable` can compile it?
- What's the smallest `package.json` + `vite.config.mts` that can serve
  Fable's JS output?

## Milestone 2 — Domain model

**Goal:** refine the existing `Domain.fs` stubs (`Address`, `Value`,
`Expr`) into the real types the rest of the app builds on, matching what
the post's domain model looks like.

**Concepts:** discriminated unions, records, why a recursive `Expr` type
lets formulas nest (`=A1+B2*2`).

**Done when:** `Domain.fs` compiles with real cases for numbers, text,
errors, cell references, and at least binary operations in `Expr`.

**Guiding questions:**
- What does a formula like `=A1+B2*2` need `Expr` to represent as a tree?
- What's the difference between a `Value` (evaluated result) and an
  `Expr` (unevaluated formula)?

## Milestone 3 — Elmish skeleton (static grid)

**Goal:** `Types.fs` (State + Msg), a minimal `Update.fs`, and a `View.fs`
that renders a static NxM grid of cells (no editing yet).

**Concepts:** the Model-Update-View loop, why Elmish uses an immutable
`State` and a `Msg` union instead of mutable UI state, `Program.mkSimple`.

**Done when:** the browser shows a grid of empty cells sized from `State`,
with no interactivity required yet.

**Guiding questions:**
- What does `State` need to hold to render a grid? (Hint: think about what
  the post's `Sheet` needs vs. purely UI concerns like "which cell is
  selected".)
- Why does Elmish separate `Msg` (what happened) from `update` (how state
  changes in response)?

## Milestone 4 — Interactivity (selection & raw editing)

**Goal:** clicking a cell selects it; typing updates its *raw* text (no
parsing/evaluation yet — just storing what the user typed).

**Concepts:** Elmish event dispatch (`dispatch` in `View.fs`), how DOM
events become `Msg` values, updating a `Map` immutably.

**Done when:** you can click a cell, type text, and see the raw text
reflected back after committing (e.g. on blur/Enter).

**Guiding questions:**
- What `Msg` cases do you need? (e.g. `SelectCell`, `SetCellInput`,
  `CommitEdit`)
- How do you update one entry in an immutable `Map<Address, _>` without
  mutating it?

## Milestone 5 — Parser

**Goal:** implement `Parser.fs` to turn a formula string (e.g.
`"=A1+B2*2"`) into an `Expr`, using parser combinators as the post does.

**Concepts:** what a parser combinator is, composing small parsers
(number, reference, operator) into bigger ones, handling the leading `=`
that distinguishes formulas from literal text.

**Done when:** unit tests in `Jungle.Tests` cover parsing numbers, text,
references, and at least one binary operation.

**Guiding questions:**
- How do you parse `"A1"` into `Address { Column = 0; Row = 0 }`?
- How does operator precedence show up in how you compose parsers (e.g.
  `*` binding tighter than `+`)?

## Milestone 6 — Evaluator

**Goal:** implement `Evaluator.fs` to turn an `Expr` into a `Value`,
resolving cell references through the sheet and detecting circular
references (as the post covers).

**Concepts:** recursive evaluation, passing a `lookup` function to avoid
coupling the evaluator to sheet storage, cycle detection (e.g. tracking
cells currently being evaluated).

**Done when:** unit tests cover evaluating literals, references, binary
ops, and a circular-reference case producing an `Error`.

**Guiding questions:**
- If `A1` references `B1` which references `A1`, how do you detect that
  before it stack-overflows?
- What should happen when a formula references an empty cell?

## Milestone 7 — Wire it together + polish

**Goal:** connect `Parser` + `Evaluator` + `Sheet` into the Elmish app so
typing a formula in a cell shows its evaluated value; display errors
inline (e.g. `#CIRCULAR`, `#ERROR`).

**Concepts:** recalculation strategy (recompute all vs. dependents only —
the post keeps it simple), showing raw input vs. evaluated value
depending on selection state.

**Done when:** you can build a small working spreadsheet — enter numbers,
reference other cells, see computed results and error states.

**Stretch goals (optional, not required to finish):**
- Basic functions like `SUM(A1:A3)` (range parsing)
- Only recalculating cells that depend on the edited cell

---

## Out of scope

- Saving/loading files
- Cell formatting (fonts, colors, number formats)
- Multiple sheets
- Undo/redo

These can become their own follow-up milestones later if desired — not
part of this roadmap.
