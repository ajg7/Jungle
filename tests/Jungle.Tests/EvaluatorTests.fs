module Jungle.Tests.EvaluatorTests

open Xunit
open Jungle.Domain
open Jungle.Parser
open Jungle.Evaluator

let private a1 = { Column = 0; Row = 0 }
let private b1 = { Column = 1; Row = 0 }

[<Fact>]
let ``evaluates a literal number`` () =
    Assert.Equal(Number 5.0, evaluate (fun _ -> Error "#REF!") (Const(Number 5.0)))

[<Fact>]
let ``evaluates a reference via lookup`` () =
    let lookup addr = if addr = a1 then Number 2.0 else Error "#REF!"
    Assert.Equal(Number 2.0, evaluate lookup (Reference a1))

[<Fact>]
let ``evaluates a binary operation`` () =
    let expr = BinOp(Add, Const(Number 2.0), Const(Number 3.0))
    Assert.Equal(Number 5.0, evaluate (fun _ -> Error "#REF!") expr)

[<Fact>]
let ``propagates an error from either operand`` () =
    let lookup _ = Error "#DIV/0!"
    let expr = BinOp(Add, Reference a1, Const(Number 1.0))
    Assert.Equal(Error "#DIV/0!", evaluate lookup expr)

[<Fact>]
let ``dividing by zero produces an error`` () =
    let expr = BinOp(Divide, Const(Number 1.0), Const(Number 0.0))
    Assert.Equal(Error "#DIV/0!", evaluate (fun _ -> Error "#REF!") expr)

[<Fact>]
let ``mismatched operand types produce a value error`` () =
    let expr = BinOp(Add, Const(Number 1.0), Const(Text "x"))
    Assert.Equal(Error "#VALUE!", evaluate (fun _ -> Error "#REF!") expr)

[<Fact>]
let ``evaluateCell resolves an empty cell to zero`` () =
    Assert.Equal(Number 0.0, evaluateCell (fun _ -> None) a1)

[<Fact>]
let ``evaluateCell resolves a chain of references through the sheet`` () =
    let sheet = Map [ a1, parse "=B1+1"; b1, parse "5" ]
    Assert.Equal(Number 6.0, evaluateCell (fun addr -> Map.tryFind addr sheet) a1)

[<Fact>]
let ``evaluateCell detects a circular reference`` () =
    let sheet = Map [ a1, parse "=B1"; b1, parse "=A1" ]
    Assert.Equal(Error "#CIRCULAR", evaluateCell (fun addr -> Map.tryFind addr sheet) a1)
