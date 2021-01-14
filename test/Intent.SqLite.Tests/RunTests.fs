module RunTests

open System
open System.IO
open System.Data
open Xunit

open Intent.Core
open Intent.SqLite
open Intent.Example.Dice

open System.Data.Common
open Dapper

type A() =
    static member NewGame() = Game.start (Guid.NewGuid())
    static member NewWager by gameId player amount = Wager.make (fun () -> DateTimeOffset.UtcNow) by gameId player amount
    
let private ensureOpen (conn : #DbConnection) =
        match conn.State with
        | ConnectionState.Broken -> 
            conn.Close()
            conn.Open()
            conn
        | ConnectionState.Closed ->
            conn.Open()
            conn
        | _ -> conn        
let createWagerTable (conn : #DbConnection) =
    let sql = """
CREATE TABLE IF NOT EXISTS wagers (
    game_id TEXT NOT NULL,
    gambler TEXT NOT NULL,
    created_at TEXT NOT NULL,
    state_json TEXT NOT NULL,
    amount INTEGER DEFAULT 1,
    is_paid INTEGER DEFAULT 0
    PRIMARY KEY (game_id, gambler)
);
"""
    conn |> ensureOpen
    |> fun c -> c.Execute(sql) |> ignore; c
    
let cleanup dbName =
    let fileName = sprintf "%s.sqlite" dbName
    let path = Path.Combine(fileName)
    if File.Exists(path) then do File.Delete(path)

[<Fact>]
let ``A new game is in DB with scores 0 | 0`` () =
    let dbName = "test100"
    cleanup dbName
    use conn = Storage.connf dbName |> Storage.createTabel |> createWagerTable
    
    let gameState = A.NewGame()
    let game = gameState |> GameState.game
    let aWager = A.NewWager "Bob" (game |> Game.id) (game |> Game.player1) 1000
    
    
    
    Assert.Equal(conn.State, ConnectionState.Open)
    