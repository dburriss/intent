namespace Intent.Core

type Serializer<'Intent> = 'Intent -> string
type Deserializer<'Intent> = string -> 'Intent
type Effect<'Intent> = 'Intent -> 'Intent

module Serialization =
    
    module Default = 
        open System.Text.Json
        open System.Text.Json.Serialization
        
        let private jsonOptions =
            let options = JsonSerializerOptions()
            options.Converters.Add(JsonFSharpConverter())
            options
            
        let serializer<'a> = fun x -> JsonSerializer.Serialize<'a>(x, jsonOptions)
            
        let deserializer<'a> = fun (s : string) -> JsonSerializer.Deserialize<'a>(s, jsonOptions)
        
    let serialize<'Intent> serializer (intent : 'Intent) : string = serializer intent
    let deserialize<'Intent> deserializer (data : string) : 'Intent = deserializer data

module Intent =
    let run<'Intent> (effect: Effect<'Intent>) (instance) =  instance |> effect
