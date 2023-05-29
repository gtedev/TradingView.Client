module Main

open Expecto
open TradingView.Client

[<EntryPoint>]
let main args =

    let login = ""
    let password = ""

    Client.connect login password
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> ignore

    Tests.runTestsInAssemblyWithCLIArgs [ No_Spinner ] args
