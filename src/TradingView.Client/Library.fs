module TradingViewClient

open System
open System.Net.Http

type SessionIdData =
    { SessionId: string option
      Expires: DateTimeOffset option
      Domain: string option }

let private (|SessionId|) =
    Array.tryFind (fun (key, _) -> key = "sessionid") >> Option.map snd

let private (|Expires|) =
    Array.tryFind (fun (key, _) -> key = "expires")
    >> Option.map (snd >> DateTimeOffset.Parse)

let private (|Domain|) =
    Array.tryFind (fun (key, _) -> key = "domain") >> Option.map snd

let private toLower (value: string) = value.Trim().ToLowerInvariant()

let private getSessionIdData (sessionIdData: string) =
    let result =
        sessionIdData.Split(";")
        |> Array.map (fun x -> x.Split("="))
        |> Array.choose (fun x ->
            match x with
            | x when x.Length = 2 -> Some(x[0] |> toLower, x[1] |> toLower)
            | _ -> None)
        |> fun (SessionId sessionId & Expires expireTime & Domain domain) ->
            { SessionId = sessionId
              Expires = expireTime
              Domain = domain }

    result

let connect username password =
    task {
        use httpClient = new HttpClient()
        let uri = Uri("https://www.tradingview.com/accounts/signin/")

        httpClient.DefaultRequestHeaders.Add("origin", "https://www.tradingview.com")
        httpClient.DefaultRequestHeaders.Add("referer", "https://www.tradingview.com")

        let form = new MultipartFormDataContent()
        form.Add(new StringContent(username), "username")
        form.Add(new StringContent(password), "password")
        form.Add(new StringContent("on"), "remember")

        let! response = httpClient.PostAsync(uri, form)

        let sessionIdData =
            response.Headers
            |> Seq.tryFind (fun kvp -> kvp.Key = "Set-Cookie")
            |> Option.map (fun kvp -> kvp.Value)
            |> Option.bind (Seq.tryFind (fun x -> x.StartsWith "sessionid"))
            |> Option.map getSessionIdData

        // let! body = response.Content.ReadAsStringAsync ()

        return sessionIdData
    }
