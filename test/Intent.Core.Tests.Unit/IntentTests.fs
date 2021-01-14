module Tests

open System
open Xunit
open Swensen.Unquote
open Intent.Core

[<CLIMutable>]
type TestRow = {
    Id: int
    At: DateTimeOffset
}

type IntentOf2 =
| Pending of TestRow
| Done of TestRow

let isDone = function | Done _ -> true | _ -> false

[<Fact>]
let ``run applies effect to input`` () =
    let data = { Id = 1; At = DateTimeOffset.UtcNow }
    let x = Pending data
    let effect = function | Pending x -> Done x | Done x -> Done x
    let intent = x |> Intent.run effect

    test <@ intent |> isDone @>
