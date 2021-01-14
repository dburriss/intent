module StorageTests

open System
open System.IO
open System.Data
open FSharp.Control.Tasks.V2
open Xunit

open Intent.Core
open Intent.Core.Storage
open Intent.SqLite
open Intent.Example.Dice

type A() =
    static member NewRow partition intent =
        {   Id = Guid.NewGuid()
            CreatedAt = DateTimeOffset.UtcNow
            Version = 0
            Partition = partition
            Message = "Pending"
            Data = intent |> Serialization.serialize Serialization.Default.serializer
        }
        

let cleanup dbName =
    let fileName = sprintf "%s.sqlite" dbName
    let path = Path.Combine(fileName)
    if File.Exists(path) then do File.Delete(path)

[<Fact>]
let ``Interacting opens connection`` () =
    use conn = Storage.connf "test" |> Storage.createTabel
    Assert.Equal(conn.State, ConnectionState.Open)
    
[<Fact>]
let ``Fetch intents for default partition when no intents`` () = task {
    let dbName = "test1"
    cleanup dbName
    
    use conn = Storage.connf dbName |> Storage.createTabel
    let! result = conn |> Storage.rowsForPartition 0
    Assert.Empty(result)    
}

[<Fact>]
let ``Add intent adds 1 row`` () = task {
    let dbName = "test2"
    cleanup dbName
    
    use conn = Storage.connf dbName |> Storage.createTabel
    let intent = Game.start()
    let newRow = A.NewRow 0 intent 
    let! result = conn |> Storage.createRow newRow
    Assert.Equal(1, result) 
}

[<Fact>]
let ``Add 2 intent with different partitions and fetch one`` () = task {
    let dbName = "test2"
    cleanup dbName
    
    use conn = Storage.connf dbName |> Storage.createTabel
    let intent = Game.start()
    let! _ = conn |> Storage.createRow (A.NewRow 0 intent)
    let! _ = conn |> Storage.createRow (A.NewRow 1 intent)
    let! rows = conn |> Storage.rowsForPartition 1
    let partition = rows |> Seq.head |> fun x -> x.Partition
    Assert.Equal(1, partition) 
}

