module ShadowMud.Rooms

open System
open System.Data.Linq
open System.Text

open Microsoft.FSharp.Core.LanguagePrimitives

open ShadowMud.Data
open ShadowMud.Cache

type private Client = Characters.Client

type Room =
    {   Description : string;
        Id : int;
        Title : string }

type Exit =
    {   Description : string;
        DestinationRoomId : int;
        Direction : Direction;
        Id : int;
        KeyId : Nullable<int>;
        SourceRoomId : int }

type ExitKeyword =
    {   Keyword : string;
        ExitId : int }

type ExitMap = Map<Direction, Exit>
type ExitKeywordsMap = Map<Direction, seq<ExitKeyword>>

type private RoomStateCommands =
    | AddClient of (Client * AsyncReplyChannel<unit>)
    | ContainsClient of (Client * AsyncReplyChannel<bool>)
    | RemoveClient of (Client * AsyncReplyChannel<unit>)
    | GetClients of AsyncReplyChannel<List<Client>>
    | GetExitData of AsyncReplyChannel<ExitMap>
    | SetExitData of (ExitMap * AsyncReplyChannel<unit>)
    | GetExitKeywords of AsyncReplyChannel<ExitKeywordsMap>
    | SetExitKeywords of (ExitKeywordsMap * AsyncReplyChannel<unit>)
    | GetRoom of AsyncReplyChannel<Room>
    | SetRoom of (Room * AsyncReplyChannel<unit>)

type RoomsState (room : Room, exitData : ExitMap, exitKeywords : ExitKeywordsMap) =
    let agent = Agent.Start (fun inbox ->
        let rec loop (clients, room : Room, exitData : ExitMap, exitKeywords : ExitKeywordsMap) =
            async { let! msg = inbox.Receive()
                    match msg with
                    | AddClient (client, rc) ->
                        client.CurrentRoom <- room.Id
                        rc.Reply ()
                        return! loop (clients @ [ client ], room, exitData, exitKeywords)
                    | ContainsClient (client, rc) ->
                        match clients |> List.tryFind (fun c -> c = client) with
                        | Some value ->
                            rc.Reply true
                            return! loop (clients, room, exitData, exitKeywords)
                        | None ->
                            rc.Reply false
                            return! loop (clients, room, exitData, exitKeywords)
                    | RemoveClient (client, rc) ->
                        rc.Reply ()
                        let newClients = clients |> List.filter(fun c -> not (c.Character.Data.Id = client.Character.Data.Id))
                        return! loop (newClients, room, exitData, exitKeywords)
                    | GetClients rc ->
                        rc.Reply clients
                        return! loop (clients, room, exitData, exitKeywords)
                    | GetRoom rc ->
                        rc.Reply room
                        return! loop (clients, room, exitData, exitKeywords)
                    | SetRoom (newRoom, rc) ->
                        rc.Reply ()
                        return! loop (clients, newRoom, exitData, exitKeywords)
                    | GetExitData rc ->
                        rc.Reply exitData
                        return! loop (clients, room, exitData, exitKeywords)
                    | SetExitData (newExitData, rc) ->
                        rc.Reply ()
                        return! loop (clients, room, newExitData, exitKeywords)
                    | GetExitKeywords rc ->
                        rc.Reply exitKeywords
                        return! loop (clients, room, exitData, exitKeywords)
                    | SetExitKeywords (newExitKeywords, rc) ->
                        rc.Reply ()
                        return! loop (clients, room, exitData, newExitKeywords) }
        loop (List.Empty, room, exitData, exitKeywords))

    member rd.AddClient client = agent.PostAndReply (fun rc -> AddClient (client, rc))
    member rd.ContainsClient client = agent.PostAndReply (fun rc -> ContainsClient (client, rc))
    member rd.RemoveClient client = agent.PostAndReply (fun rc -> RemoveClient (client, rc))

    member rd.Clients with get () = agent.PostAndReply (fun rc -> GetClients rc)

    member rd.Room with get () = agent.PostAndReply (fun rc -> GetRoom rc) and set value = agent.PostAndReply (fun rc -> SetRoom (value, rc))
    member rd.ExitData with get () = agent.PostAndReply (fun rc -> GetExitData rc) and set value = agent.PostAndReply (fun rc -> SetExitData (value, rc))
    member rd.ExitKeywords with get () = agent.PostAndReply (fun rc -> GetExitKeywords rc) and set value = agent.PostAndReply (fun rc -> SetExitKeywords (value, rc))

let private edmConnectionString = "metadata=res://*/ShadowMud.csdl|res://*/ShadowMud.ssdl|res://*/ShadowMud.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='data source=|DataDirectory|..\..\..\ShadowMudlib\EntityModel\ShadowMud.sdf;password=q0p1w9o2e8;persist security info=True;'"
let private context = new EntityModel.Entities(edmConnectionString)

let private doesRoomExist id =
    not (query { for i in context.Rooms do
                 where (i.Id = id)
                 select i
               } |> Seq.isEmpty)

type RoomCache (isValid) as rc =
    inherit LRUCache<RoomsState> (20, TimeSpan.FromMinutes 1.0, TimeSpan.FromMinutes 5.0, isValid)
    
    let _findByRoomId = rc.AddIndex ("RoomId", (fun room -> room.Room.Id), new LRUCache<RoomsState>.LoadItemFunc<int> (rc.LoadRoomData))

    do
        rc.IsValid.Invoke() |> ignore

    member rc.FindByRoomId roomId : RoomsState =
        _findByRoomId.[roomId]
        
    member rc.LoadRoomData id =
        try
            let getExitKeywords (exitId) =
                query { for i in context.ExitKeywords do
                        where (i.ExitId = exitId)
                        select { Keyword = i.Keyword; ExitId = i.ExitId }
                      }

            let getExits (id) =
                query { for i in context.Exits do
                        where (i.SourceRoomId = id)
                        select i
                      } |> Seq.fold (fun (state : Map<Direction, Exit>) exit -> 
                                          let direction = EnumOfValue exit.Direction
                                          state.Add (direction, { Description = exit.Description; DestinationRoomId = exit.DestinationRoomId;
                                                     Direction = direction; Id = exit.Id; KeyId = exit.KeyId; SourceRoomId = exit.SourceRoomId })) Map.empty
              
            let edMap = getExits id
            let ekMap = edMap |> Map.fold(fun (state : ExitKeywordsMap) key value -> state.Add (key, getExitKeywords value.Id)) Map.empty

            let room = query { for i in context.Rooms do
                               where (i.Id = id)
                               select ({ Description = i.Description; Id = i.Id; Title = i.Title } : Room)
                             } |> Seq.head

            new RoomsState (room, edMap, ekMap)
        
        with
            | exn ->
                Console.WriteLine "Error - Character not found"
                reraise ()

let mutable private tableVersion = 0

let private isDataValid () =
    let oldVersion = tableVersion;
    
    //tableVersion <- dc.TableUpdates.Single (fun updates -> updates.Id = (TableId.Rooms :> int)).Updates;

    //(oldVersion = tableVersion)
    true

type private RoomCommands =
    | RoomExists of (int * AsyncReplyChannel<bool>)
    | GetRoom of (int * AsyncReplyChannel<RoomsState>)
    | AddToRoom of (Client * int * AsyncReplyChannel<unit>)
    | LookRoom of (Client * bool * int * AsyncReplyChannel<string>)
    | Exit

let lookExits (roomExitData : ExitMap, short) =
    if short then
        let text = new StringBuilder ()
        if roomExitData.Count > 0 then
            for exit in roomExitData do
                text.AppendFormat ("{0} ", exit.Value.Direction) |> ignore
            (text.Append ("\r\n")).ToString ()
        else (text.Append ("None\r\n")).ToString ()
    else
        let text = new StringBuilder ("\r\n")
        if roomExitData.Count > 0 then
            for exit in roomExitData do
                text.AppendFormat ("  {0} - {1}\r\n", exit.Value.Direction, exit.Value.Description) |> ignore
            (text.Append ("\r\n")).ToString ()
        else (text.Append ("None\r\n")).ToString ()

let lookClients (clients : List<Client>, lookClient) =
    if clients.Length > 0 then
        let text = new StringBuilder ()
        for client in clients do
            //if not (client = lookClient) then
            let character = client.Character
            text.AppendFormat (" {0} is standing here.\r\n", character.Data.Name) |> ignore
            //else ()
        (text.Append "\r\n").ToString ()
    else "\r\n"

let private agent = Agent.Start (fun inbox ->
    let rec loop (roomCache : RoomCache) =
        async { let! msg = inbox.Receive ()
                match msg with
                | RoomExists (id, rc) ->
                    rc.Reply (doesRoomExist id)
                    return! loop roomCache
                | LookRoom (client, short, roomId, rc) ->
                    let roomState = roomCache.FindByRoomId roomId
                    let clients = lookClients (roomState.Clients, client)
                    let exits = lookExits (roomState.ExitData, short)
                    rc.Reply (String.Format("[{0}]  {1}\r\n {2}\r\n\r\n Exits: {3}\r\n{4}", roomState.Room.Id, roomState.Room.Title, roomState.Room.Description, exits, clients))
                    return! loop (roomCache)
                | AddToRoom (client, roomId, rc) ->
                    let character = client.Character
                    let room = roomCache.FindByRoomId roomId
                    if client.CurrentRoom = roomId then
                        if not (room.ContainsClient client) then
                            room.AddClient client
                            rc.Reply ()
                            return! loop roomCache
                    else
                        let curRoom = roomCache.FindByRoomId client.CurrentRoom
                        curRoom.RemoveClient client
                        room.AddClient client
                        rc.Reply ()
                        return! loop roomCache
                | Exit ->
                    return ()
                | GetRoom (id, rc) ->
                    rc.Reply (roomCache.FindByRoomId id)
                    return! loop roomCache }
    loop (new RoomCache (new LRUCache<RoomsState>.IsCacheValid (isDataValid))))

let checkRoom id = agent.PostAndReply (fun rc -> RoomExists (id, rc))
let getRoom id = agent.PostAndReply (fun rc -> GetRoom (id, rc))
let addToRoom (client, roomId) = agent.PostAndReply(fun rc -> AddToRoom (client, roomId, rc))

let lookRoom (client, roomId, short) =
    agent.PostAndReply (fun rc -> LookRoom (client, short, roomId, rc))