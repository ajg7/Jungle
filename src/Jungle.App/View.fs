module Jungle.View

open Feliz
open Jungle.Domain
open Jungle.Types

let private cellStyle =
    [ style.border (1, borderStyle.solid, Theme.border)
      style.width 60
      style.height 24
      style.backgroundColor Theme.surface
      style.color Theme.text ]

let private selectedCellStyle =
    cellStyle @ [ style.border (2, borderStyle.solid, Theme.accent) ]

let private inputStyle =
    [ style.width 56
      style.backgroundColor Theme.inputBackground
      style.color Theme.text
      style.border (1, borderStyle.solid, Theme.accent) ]

let private formatValue (value: Value) : string =
    match value with
    | Number n -> string n
    | Text s -> s
    | Error e -> e

let private displayValue (state: State) (address: Address) : string * bool =
    match state.RawInput |> Map.tryFind address with
    | None
    | Some "" -> "", false
    | Some _ ->
        match Jungle.Sheet.getValue address state.Sheet with
        | Error e -> e, true
        | v -> formatValue v, false

let private cellView (state: State) (dispatch: Msg -> unit) (address: Address) =
    let isSelected = state.Selected = Some address

    if isSelected then
        Html.td [
            prop.style selectedCellStyle
            prop.children [
                Html.input [
                    prop.style inputStyle
                    prop.value state.EditingText
                    prop.autoFocus true
                    prop.onChange (fun (text: string) -> dispatch (SetCellInput text))
                    prop.onBlur (fun _ -> dispatch CommitEdit)
                    prop.onKeyDown (fun e -> if e.key = "Enter" then dispatch CommitEdit)
                ]
            ]
        ]
    else
        let text, isError = displayValue state address

        Html.td [
            prop.style (cellStyle @ [ style.color (if isError then Theme.errorText else Theme.text) ])
            prop.text text
            prop.onClick (fun _ -> dispatch (SelectCell address))
        ]

let private helpItems =
    [ "Click a cell", "Select it"
      "Type", "Edit the raw text or formula"
      "Enter", "Commit the edit"
      "Click elsewhere", "Commit the edit (on blur)"
      "= at the start", "Treat input as a formula, e.g. =A1+B2*2"
      "B3, AA12, ...", "Cell reference syntax: column letters + row number"
      "( )  +  -  *  /", "Supported grouping and operators, usual precedence"
      "#DIV/0!", "Division by zero"
      "#VALUE!", "An operand is not a number"
      "#CIRCULAR", "A formula refers back to itself" ]

let private helpPanel =
    Html.div [
        prop.style [
            style.backgroundColor Theme.surface
            style.border (1, borderStyle.solid, Theme.border)
            style.borderRadius 6
            style.padding 16
            style.width 260
            style.color Theme.text
            style.fontSize 13
        ]
        prop.children [
            Html.h3 [
                prop.style [ style.marginTop 0; style.color Theme.accent ]
                prop.text "🦜 Shortcuts & syntax"
            ]
            Html.ul [
                prop.style [ style.custom ("listStyleType", "none"); style.padding 0; style.margin 0 ]
                prop.children [
                    for key, desc in helpItems do
                        Html.li [
                            prop.style [ style.marginBottom 8 ]
                            prop.children [
                                Html.div [
                                    prop.style [ style.color Theme.accent; style.fontWeight 600 ]
                                    prop.text key
                                ]
                                Html.div [ prop.text desc ]
                            ]
                        ]
                ]
            ]
        ]
    ]

let private sidebar (state: State) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [ style.marginLeft 24; style.display.flex; style.flexDirection.column ]
        prop.children [
            Html.button [
                prop.style [
                    style.backgroundColor Theme.surface
                    style.color Theme.text
                    style.border (1, borderStyle.solid, Theme.accent)
                    style.borderRadius 4
                    style.padding 8
                    style.cursor.pointer
                ]
                prop.text (if state.ShowHelp then "🐍 Hide shortcuts" else "🐍 Shortcuts")
                prop.onClick (fun _ -> dispatch ToggleHelp)
            ]
            if state.ShowHelp then
                Html.div [ prop.style [ style.marginTop 12 ]; prop.children [ helpPanel ] ]
        ]
    ]

let private title =
    Html.h1 [
        prop.style [
            style.color Theme.accent
            style.fontFamily "system-ui, sans-serif"
            style.marginTop 0
            style.marginBottom 16
            style.textAlign.center
        ]
        prop.text "🌴 Jungle 🐒"
    ]

let view (state: State) (dispatch: Msg -> unit) =
    Html.div [
        prop.style [
            style.backgroundColor Theme.background
            style.minHeight (length.vh 100)
            style.padding 24
            style.fontFamily "system-ui, sans-serif"
        ]
        prop.children [
            title
            Html.div [
                prop.style [
                    style.display.flex
                    style.flexDirection.row
                    style.alignItems.flexStart
                    style.justifyContent.center
                ]
                prop.children [
                    Html.table [
                        prop.style [ style.borderCollapse.collapse ]
                        prop.children [
                            Html.tbody [
                                for row in 0 .. state.RowCount - 1 do
                                    Html.tr [
                                        for col in 0 .. state.ColCount - 1 do
                                            cellView state dispatch { Column = col; Row = row }
                                    ]
                            ]
                        ]
                    ]
                    sidebar state dispatch
                ]
            ]
        ]
    ]
