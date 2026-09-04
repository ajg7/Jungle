module Jungle.Sheet

open Jungle.Domain

type Sheet = { Cells: Map<Address, Expr> }

let empty: Sheet = { Cells = Map.empty }

let setCell (address: Address) (input: string) (sheet: Sheet) : Sheet =
    { sheet with Cells = sheet.Cells.Add(address, Jungle.Parser.parse input) }

let getValue (address: Address) (sheet: Sheet) : Value =
    Jungle.Evaluator.evaluateCell (fun a -> Map.tryFind a sheet.Cells) address
