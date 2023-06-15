[<AutoOpen>]
module Types

open System

type ExpirationType = ExpirationType of string

type SessionIdData =
    { SessionId: string option
      Expires: DateTimeOffset option
      Domain: string option }
