module Jungle.Evaluator

open Jungle.Domain

let private applyOp (op: Operator) (left: float) (right: float) : Value =
    match op with
    | Add -> Number(left + right)
    | Subtract -> Number(left - right)
    | Multiply -> Number(left * right)
    | Divide -> if right = 0.0 then Error "#DIV/0!" else Number(left / right)

let rec evaluate (lookup: Address -> Value) (expr: Expr) : Value =
    match expr with
    | Const v -> v
    | Reference addr -> lookup addr
    | BinOp(op, left, right) ->
        match evaluate lookup left, evaluate lookup right with
        | Error e, _ -> Error e
        | _, Error e -> Error e
        | Number l, Number r -> applyOp op l r
        | _ -> Error "#VALUE!"

let evaluateCell (exprOf: Address -> Expr option) (start: Address) : Value =
    let rec go (visiting: Set<Address>) (addr: Address) : Value =
        if Set.contains addr visiting then
            Error "#CIRCULAR"
        else
            match exprOf addr with
            | None -> Number 0.0
            | Some expr -> evaluate (go (Set.add addr visiting)) expr

    go Set.empty start
