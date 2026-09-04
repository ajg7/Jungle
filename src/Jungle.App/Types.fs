module Jungle.Types

open Jungle.Domain

type State =
    { RowCount: int
      ColCount: int
      RawInput: Map<Address, string>
      Sheet: Jungle.Sheet.Sheet
      Selected: Address option
      EditingText: string
      ShowHelp: bool }

type Msg =
    | SelectCell of Address
    | SetCellInput of string
    | CommitEdit
    | ToggleHelp
