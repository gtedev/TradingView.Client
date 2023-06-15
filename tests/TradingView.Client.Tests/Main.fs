module Main

open Expecto

[<EntryPoint>]
let main args =
    let adminUserName = ""
    let password = ""

    // Add / Remove data
    let userName = ""
    let pineId = ""

    option {
        let! sessionIdData =
            TradingViewClient.connect adminUserName password
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let! sessionId = sessionIdData.SessionId

        let addAccessResponse =
            TradingViewClient.addAccess pineId userName sessionId (ExpirationType "L")
            |> Async.AwaitTask
            |> Async.RunSynchronously

        let removeAccessResponse =
            TradingViewClient.removeAccess pineId userName sessionId
            |> Async.AwaitTask
            |> Async.RunSynchronously

        return ()
    }
    |> ignore

    Tests.runTestsInAssemblyWithCLIArgs [ No_Spinner ] args
