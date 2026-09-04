module Jungle.Tests.SheetTests

open Xunit
open Jungle.Domain
open Jungle.Sheet

let private a1 = { Column = 0; Row = 0 }
let private b1 = { Column = 1; Row = 0 }

[<Fact>]
let ``getValue on an empty sheet returns zero`` () =
    Assert.Equal(Number 0.0, getValue a1 empty)

[<Fact>]
let ``setCell then getValue evaluates a literal`` () =
    let sheet = empty |> setCell a1 "5"
    Assert.Equal(Number 5.0, getValue a1 sheet)

[<Fact>]
let ``setCell then getValue evaluates a formula referencing another cell`` () =
    let sheet = empty |> setCell b1 "5" |> setCell a1 "=B1+1"
    Assert.Equal(Number 6.0, getValue a1 sheet)

[<Fact>]
let ``setCell then getValue detects a circular reference`` () =
    let sheet = empty |> setCell a1 "=B1" |> setCell b1 "=A1"
    Assert.Equal(Error "#CIRCULAR", getValue a1 sheet)
