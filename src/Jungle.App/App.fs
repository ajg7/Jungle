module Jungle.App

open Elmish
open Elmish.React
open Jungle.Update
open Jungle.View

Program.mkProgram init update view
|> Program.withReactSynchronous "app"
|> Program.run