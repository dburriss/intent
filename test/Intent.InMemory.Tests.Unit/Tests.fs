module Tests

open System
open Xunit
open System.Collections.Generic
open Swensen.Unquote

[<CLIMutable>]
type TestRow = {
    Id: int
    At: DateTimeOffset
}

type IntentOf2 =
| Pending of TestRow
| Done of TestRow

type TestRowData() =
    let mutable intents = List.empty<IntentOf2>
    member this.Data = Dictionary<int,TestRow>()
    member this.Intents = intents
    member this.Save v = 
        do this.Data.Add(v.Id, v)
        do intents <- this.Intents @ [Pending v]

    member this.ProcessIntents effect =
        
        do intents <- intents |> List.map effect    


[<Fact>]
let ``Saved intent is saved in Pending`` () =
    let memory = TestRowData()
    let now = DateTimeOffset.UtcNow
    let row = {Id = 1; At = now}
    do memory.Save({Id = 1; At = now}) |> ignore
    test <@ memory.Intents = [Pending row] @>

[<Fact>]
let ``Saved intent is processed with data`` () =
    let memory = TestRowData()
    let now = DateTimeOffset.UtcNow
    let row = {Id = 1; At = now}
    do memory.Save({Id = 1; At = now}) |> ignore
    let effect = function | Pending x -> Done x | Done x -> Done x
    memory.ProcessIntents effect
    test <@ memory.Intents = [Done row] @>