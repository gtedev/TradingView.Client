module Main

open Expecto
open TradingView.Client

[<EntryPoint>]
let main args =
    let username = ""
    let password = ""

    let result =
        Client.connect username password |> Async.AwaitTask |> Async.RunSynchronously

    Tests.runTestsInAssemblyWithCLIArgs [ No_Spinner ] args
