module TradingViewClient

open System
open System.Net.Http

let private (|SessionId|) =
    Array.tryFind (fun (key, _) -> key = "sessionid") >> Option.map snd

let private (|Expires|) =
    Array.tryFind (fun (key, _) -> key = "expires")
    >> Option.map (snd >> DateTimeOffset.Parse)

let private (|Domain|) =
    Array.tryFind (fun (key, _) -> key = "domain") >> Option.map snd

let private toLower (value: string) = value.Trim().ToLowerInvariant()

let private extractSessionIdData (sessionIdData: string) =
    // Example of sessionId data returned by TradingView:
    //
    // sessionid=12345; Domain=.tradingview.com; expires=Wed, 30 Aug 2023 22:14:50 GMT; HttpOnly; Max-Age=8035200; Path=/; SameSite=Lax; Secure
    //
    // We choose below to just extract: sessionid, Domain and expires
    //
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
            |> Option.map extractSessionIdData

        // let! body = response.Content.ReadAsStringAsync ()

        return sessionIdData
    }

let removeAccess pineId userName (sessionId: string) =
    task {
        use httpClient = new HttpClient()
        let uri = Uri("https://www.tradingview.com/pine_perm/remove/")

        httpClient.DefaultRequestHeaders.Add("origin", "https://www.tradingview.com")
        httpClient.DefaultRequestHeaders.Add("cookie", $"sessionid={sessionId}")

        let form = new MultipartFormDataContent()
        form.Add(new StringContent(userName), "pine_id")
        form.Add(new StringContent(pineId), "username_recip")

        let! response = httpClient.PostAsync(uri, form)

        return response
    }

let addAccess pineId userName (sessionId: string) (ExpirationType expirationType) =
    task {
        use httpClient = new HttpClient()
        let uri = Uri("https://www.tradingview.com/pine_perm/add/")

        httpClient.DefaultRequestHeaders.Add("origin", "https://www.tradingview.com")
        httpClient.DefaultRequestHeaders.Add("cookie", $"sessionid={sessionId}")

        let form = new MultipartFormDataContent()
        form.Add(new StringContent(userName), "pine_id")
        form.Add(new StringContent(pineId), "username_recip")
        form.Add(new StringContent(expirationType), "expiration")

        let! response = httpClient.PostAsync(uri, form)

        return response
    }
