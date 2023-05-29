namespace TradingView.Client

open System
open System.Net.Http
open System.Net.Http.Json
open System.Net.Http.Headers

module Client =
    let connect login password =
        task {
            let httpClient = new HttpClient()
            let uri = Uri("https://www.tradingview.com/accounts/signin/")

            httpClient.DefaultRequestHeaders.Add("origin", "https://www.tradingview.com")
            httpClient.DefaultRequestHeaders.Add("referer", "https://www.tradingview.com")
            // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36")


            let form = new MultipartFormDataContent()
            form.Add(new StringContent(login), "login")
            form.Add(new StringContent(password), "password")
            form.Add(new StringContent("on"), "remember")

            let! result = httpClient.PostAsync(uri, form)

            return result
        }
