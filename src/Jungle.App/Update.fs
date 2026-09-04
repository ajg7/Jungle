module Jungle.Update

open Elmish
open Jungle.Types

let init () =
    { RowCount = 10
      ColCount = 6
      RawInput = Map.empty
      Sheet = Jungle.Sheet.empty
      Selected = None
      EditingText = ""
      ShowHelp = false },
    Cmd.none

let update (msg: Msg) (state: State) =
    match msg with
    | SelectCell address ->
        let existingText =
            state.RawInput |> Map.tryFind address |> Option.defaultValue ""

        { state with Selected = Some address; EditingText = existingText }, Cmd.none

    | SetCellInput text -> { state with EditingText = text }, Cmd.none

    | CommitEdit ->
        match state.Selected with
        | Some address ->
            { state with
                RawInput = state.RawInput.Add(address, state.EditingText)
                Sheet = Jungle.Sheet.setCell address state.EditingText state.Sheet },
            Cmd.none
        | None -> state, Cmd.none

    | ToggleHelp -> { state with ShowHelp = not state.ShowHelp }, Cmd.none
