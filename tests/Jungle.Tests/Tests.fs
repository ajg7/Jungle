module Jungle.Tests.ParserTests

open Xunit
open Jungle.Domain
open Jungle.Parser

[<Fact>]
let ``parses a plain integer as a number`` () =
    Assert.Equal(Const(Number 42.0), parse "42")

[<Fact>]
let ``parses a decimal number`` () =
    Assert.Equal(Const(Number 3.14), parse "3.14")

[<Fact>]
let ``parses non-numeric input as text`` () =
    Assert.Equal(Const(Text "hello"), parse "hello")

[<Fact>]
let ``parses a single-letter cell reference`` () =
    Assert.Equal(Reference { Column = 1; Row = 2 }, parse "=B3")

[<Fact>]
let ``parses a multi-letter column reference`` () =
    Assert.Equal(Reference { Column = 26; Row = 0 }, parse "=AA1")

[<Fact>]
let ``parses addition of two references`` () =
    let expected =
        BinOp(Add, Reference { Column = 0; Row = 0 }, Reference { Column = 1; Row = 1 })

    Assert.Equal(expected, parse "=A1+B2")

[<Fact>]
let ``gives multiplication higher precedence than addition`` () =
    let expected =
        BinOp(Add, Const(Number 1.0), BinOp(Multiply, Const(Number 2.0), Const(Number 3.0)))

    Assert.Equal(expected, parse "=1+2*3")

[<Fact>]
let ``parentheses override default precedence`` () =
    let expected =
        BinOp(Multiply, BinOp(Add, Const(Number 1.0), Const(Number 2.0)), Const(Number 3.0))

    Assert.Equal(expected, parse "=(1+2)*3")

[<Fact>]
let ``returns an error for malformed formulas`` () =
    Assert.Equal(Const(Error "#ERROR"), parse "=1+")
