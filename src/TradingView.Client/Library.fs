namespace TradingView.Client

open System
open System.Net.Http

module Client =

    type SessionIdData =
        { SessionId: string option
          Expires: DateTimeOffset option
          Domain: string option }

    let getSessionIdData (sessionIdData: string) =
        let result =
            sessionIdData.Split(";")
            |> List.ofArray
            |> List.map (fun x -> x.Split("="))
            |> List.choose (fun x ->
                match x with
                | x when x.Length = 2 -> Some(x[0].Trim().ToLowerInvariant(), x[1].Trim().ToLowerInvariant())
                | _ -> None)
            |> fun values ->

                { SessionId = values |> List.tryFind (fun (key, _) -> key = "sessionid") |> Option.map snd
                  Expires =
                    values
                    |> List.tryFind (fun (key, _) -> key = "expires")
                    |> Option.map (snd >> DateTimeOffset.Parse)
                  Domain = values |> List.tryFind (fun (key, _) -> key = "domain") |> Option.map snd }

        result

    let connect username password =
        task {
            let httpClient = new HttpClient()
            let uri = Uri("https://www.tradingview.com/accounts/signin/")

            httpClient.DefaultRequestHeaders.Add("origin", "https://www.tradingview.com")
            httpClient.DefaultRequestHeaders.Add("referer", "https://www.tradingview.com")

            let form = new MultipartFormDataContent()
            form.Add(new StringContent(username), "username")
            form.Add(new StringContent(password), "password")
            form.Add(new StringContent("on"), "remember")

            let! response = httpClient.PostAsync(uri, form)

            let sessionId =
                response.Headers
                |> List.ofSeq
                |> List.tryFind (fun x -> x.Key = "Set-Cookie")
                |> Option.map (fun kvps -> List.ofSeq kvps.Value)
                |> Option.bind (fun values -> values |> List.tryFind (fun x -> x.Contains "sessionid"))
                |> Option.map getSessionIdData

            // let! body = response.Content.ReadAsStringAsync ()

            return sessionId
        }
