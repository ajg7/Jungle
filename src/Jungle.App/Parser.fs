module Jungle.Parser

open System
open Jungle.Domain

type Parser<'a> = char list -> ('a * char list) option

let pChar (predicate: char -> bool) : Parser<char> =
    fun input ->
        match input with
        | c :: rest when predicate c -> Some(c, rest)
        | _ -> None

let pDigit: Parser<char> = pChar Char.IsDigit
let pLetter: Parser<char> = pChar Char.IsLetter
let pSymbol (target: char) : Parser<char> = pChar (fun c -> c = target)

let map (f: 'a -> 'b) (parser: Parser<'a>) : Parser<'b> =
    fun input ->
        match parser input with
        | Some(result, rest) -> Some(f result, rest)
        | None -> None

let andThen (p1: Parser<'a>) (p2: Parser<'b>) : Parser<'a * 'b> =
    fun input ->
        match p1 input with
        | Some(result1, rest1) ->
            match p2 rest1 with
            | Some(result2, rest2) -> Some((result1, result2), rest2)
            | None -> None
        | None -> None

let orElse (p1: Parser<'a>) (p2: Parser<'a>) : Parser<'a> =
    fun input ->
        match p1 input with
        | Some _ as result -> result
        | None -> p2 input

let many (parser: Parser<'a>) : Parser<'a list> =
    let rec loop input acc =
        match parser input with
        | Some(value, rest) -> loop rest (value :: acc)
        | None -> (List.rev acc, input)

    fun input -> Some(loop input [])

let many1 (parser: Parser<'a>) : Parser<'a list> =
    fun input ->
        match parser input with
        | None -> None
        | Some(value, rest) ->
            match many parser rest with
            | Some(values, rest2) -> Some(value :: values, rest2)
            | None -> Some([ value ], rest)

let keepLeft (p1: Parser<'a>) (p2: Parser<'b>) : Parser<'a> = andThen p1 p2 |> map fst

let keepRight (p1: Parser<'a>) (p2: Parser<'b>) : Parser<'b> = andThen p1 p2 |> map snd

let between (pOpen: Parser<'a>) (pClose: Parser<'b>) (p: Parser<'c>) : Parser<'c> =
    keepRight pOpen (keepLeft p pClose)

let chainl1 (pOperand: Parser<'a>) (pOperator: Parser<'a -> 'a -> 'a>) : Parser<'a> =
    let rec loop acc input =
        match pOperator input with
        | Some(combine, rest1) ->
            match pOperand rest1 with
            | Some(value, rest2) -> loop (combine acc value) rest2
            | None -> Some(acc, input)
        | None -> Some(acc, input)

    fun input ->
        match pOperand input with
        | Some(first, rest) -> loop first rest
        | None -> None

let pNumber: Parser<float> =
    let pInt = many1 pDigit

    let pFraction =
        andThen (pSymbol '.') pInt |> map (fun (dot, digits) -> dot :: digits)

    let pDigits = orElse (andThen pInt pFraction |> map (fun (a, b) -> a @ b)) pInt

    pDigits |> map (fun chars -> Double.Parse(String(Array.ofList chars)))

let private letterValue (c: char) : int = int (Char.ToUpper c) - int 'A' + 1

let pColumn: Parser<int> =
    many1 pLetter
    |> map (fun letters -> (letters |> List.fold (fun acc c -> acc * 26 + letterValue c) 0) - 1)

let pRow: Parser<int> =
    many1 pDigit
    |> map (fun digits -> Int32.Parse(String(Array.ofList digits)) - 1)

let pAddress: Parser<Address> =
    andThen pColumn pRow |> map (fun (col, row) -> { Column = col; Row = row })

let pConstNumber: Parser<Expr> = pNumber |> map (fun n -> Const(Number n))

let pReference: Parser<Expr> = pAddress |> map Reference

let pAddOp: Parser<Expr -> Expr -> Expr> =
    orElse
        (pSymbol '+' |> map (fun _ -> fun left right -> BinOp(Add, left, right)))
        (pSymbol '-' |> map (fun _ -> fun left right -> BinOp(Subtract, left, right)))

let pMulOp: Parser<Expr -> Expr -> Expr> =
    orElse
        (pSymbol '*' |> map (fun _ -> fun left right -> BinOp(Multiply, left, right)))
        (pSymbol '/' |> map (fun _ -> fun left right -> BinOp(Divide, left, right)))

let rec pExpr (input: char list) = chainl1 pTerm pAddOp input

and pTerm (input: char list) = chainl1 pFactor pMulOp input

and pFactor (input: char list) =
    orElse (orElse pConstNumber pReference) pParenExpr input

and pParenExpr (input: char list) =
    between (pSymbol '(') (pSymbol ')') pExpr input

let parseExpr (input: string) : Expr option =
    match pExpr (List.ofSeq input) with
    | Some(expr, []) -> Some expr
    | _ -> None

let parse (input: string) : Expr =
    let trimmed = input.Trim()

    if trimmed.StartsWith "=" then
        match parseExpr (trimmed.Substring 1) with
        | Some expr -> expr
        | None -> Const(Error "#ERROR")
    else
        match Double.TryParse trimmed with
        | true, n -> Const(Number n)
        | false, _ -> Const(Text trimmed)
