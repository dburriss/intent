namespace Intent.Example.Dice

open System

type Player = {
    Score : int
    Rolls : int
}
type GameId = Guid
type Contestants = Player * Player
type Game = GameId * Contestants

type GameState =
    | Playing of Game
    | DoneWithWinner of (Player * Game)
    | Draw of Game

module Player =
    let init = { Score = 0; Rolls = 0 }

module Game =
    let start gameId =
        let player1 = Player.init
        let player2 = Player.init
        Playing (gameId, (player1, player2))
    
    let id (game : Game) = game |> fst
    let player1 (game : Game) = game |> snd |> fst
    let player2 (game : Game) = game |> snd |> snd

module GameState =
    let game = function
        | Playing g -> g
        | DoneWithWinner (_,g) -> g
        | Draw g -> g
        

type Cents = int
type Gambler = string
type Wager = {
    By : Gambler
    Game : GameId
    BetOn : Player
    Amount : Cents
    At : DateTimeOffset
}

module Wager =
    
    let make dt by game player cents =
        {
            By = by
            Game = game
            BetOn = player
            Amount = cents
            At = dt()
        }