namespace Intent.Core

open System

module Storage =

    [<CLIMutable>]
    type NewRow = {
        Id : Guid
        CreatedAt : DateTimeOffset
        Data : string
        Partition : int
        Version : int
        Message : string
    }    
        
    [<CLIMutable>]
    type ExistingRow = {
        Id : Guid
        Number : int64
        CreatedAt : DateTimeOffset
        UpdatedAt : DateTimeOffset
        Data : string
        Partition : int
        Version : int
        Message : string
        IsComplete : bool
        IsFailed : bool
    }

