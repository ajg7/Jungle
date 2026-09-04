/// Core types shared across the app: cell addresses, raw values, and
/// the expression tree that formulas parse into.
module Jungle.Domain

/// A cell address, e.g. "B3" is Address(1, 2) (0-indexed column, row).
type Address =
    { Column: int
      Row: int }

/// The value a cell can hold once evaluated.
type Value =
    | Number of float
    | Text of string
    | Error of string

type Operator =
    | Add
    | Subtract
    | Multiply
    | Divide

/// The expression tree a formula string parses into.
// TODO: extend with binary operators, function calls, ranges, etc.
type Expr =
    | Const of Value
    | Reference of Address
    | BinOp of Operator * Expr * Expr
