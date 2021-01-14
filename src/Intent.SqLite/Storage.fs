namespace Intent.SqLite

open System
open System.Data
open System.Data.Common
open Microsoft.Data.Sqlite
open Dapper
open Intent.Core.Storage
open FSharp.Control.Tasks.V2

//abstract class SqliteTypeHandler<T> : SqlMapper.TypeHandler<T>
//{
//    // Parameters are converted by Microsoft.Data.Sqlite
//    public override void SetValue(IDbDataParameter parameter, T value)
//        => parameter.Value = value;
//}
//
//class DateTimeOffsetHandler : SqliteTypeHandler<DateTimeOffset>
//{
//    public override DateTimeOffset Parse(object value)
//        => DateTimeOffset.Parse((string)value);
//}
//
//class GuidHandler : SqliteTypeHandler<Guid>
//{
//    public override Guid Parse(object value)
//        => Guid.Parse((string)value);
//}
//
//class TimeSpanHandler : SqliteTypeHandler<TimeSpan>
//{
//    public override TimeSpan Parse(object value)
//        => TimeSpan.Parse((string)value);
//}

[<AbstractClass>]
type SqliteTypeHandler<'T>() =
    inherit SqlMapper.TypeHandler<'T>()
    override this.SetValue(parameter : IDbDataParameter, value : 'T) = parameter.Value <- value

type DateTimeOffsetHandler() =
    inherit SqliteTypeHandler<DateTimeOffset>()
    override this.Parse(value : obj) = DateTimeOffset.Parse(value :?> string)
    
type GuidHandler() =
    inherit SqliteTypeHandler<Guid>()
    override this.Parse(value : obj) = Guid.Parse(value :?> string)

module Storage =
    
    let connf file = new SqliteConnection(sprintf "Data Source=%s.sqlite;" file)
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
            
    let createTabel (conn : #DbConnection) =
        let sql = """
CREATE TABLE IF NOT EXISTS intents (
    id TEXT PRIMARY KEY,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    state_json TEXT NOT NULL,
    partition_number INTEGER DEFAULT 0,
    version INTEGER DEFAULT 0,
    message TEXT DEFAULT 'Pending',
    is_complete INTEGER DEFAULT 0,
    is_failed INTEGER DEFAULT 0
);
"""
        conn |> ensureOpen
        |> fun c -> c.Execute(sql) |> ignore; c

    let rowsForPartition (partition : int) (conn : #DbConnection) =
        SqlMapper.AddTypeHandler(DateTimeOffsetHandler())
        SqlMapper.AddTypeHandler(GuidHandler())
        let sql = """
SELECT
id AS Id,
rowid AS Number,
created_at AS CreatedAt,
updated_at AS UpdatedAt,
state_json AS Data,
partition_number AS Partition,
version AS Version,
message AS Message,
is_complete AS IsComplete,
is_failed AS IsFailed
FROM intents
WHERE is_complete = 0
AND partition_number = @Partition
"""
        conn |> ensureOpen
        |> fun c -> c.QueryAsync<ExistingRow>(sql, {| Partition = partition |})
        
    let createRow (newRow : NewRow) (conn : #DbConnection) = task {
        let dt = newRow.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss.zzz")
        let row =
            {| id = newRow.Id.ToString()
               created_at = dt
               updated_at = dt
               state_json = newRow.Data
               partition_number = newRow.Partition
               version = newRow.Version
               message = newRow.Message |}
        let sql = """
INSERT INTO intents (id, created_at, updated_at, state_json, partition_number, version, message)
VALUES(@id, @created_at, @updated_at, @state_json, @partition_number, @version, @message)
"""
        return! conn.ExecuteAsync(sql, row)
    }