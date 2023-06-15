[<AutoOpen>]
module TypeExtensions

type OptionBuilder() =
    member _.Bind(x, f) =
        match x with
        | None -> None
        | Some a -> f a

    member _.Return x = Some x

let option = OptionBuilder()
