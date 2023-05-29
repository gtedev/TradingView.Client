module Main

open Expecto

[<EntryPoint>]
let main args =
    let username = ""
    let password = ""

    let sessionIdData =
        TradingViewClient.connect username password
        |> Async.AwaitTask
        |> Async.RunSynchronously

    Tests.runTestsInAssemblyWithCLIArgs [ No_Spinner ] args
