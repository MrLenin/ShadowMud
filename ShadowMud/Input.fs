module ShadowMud.Input

type private Direction = Data.Direction
type private Client = Characters.Client

type InputCommand =
    | Go of Direction option * string option
    | Look of Direction option * string option
    | Save of string option
    | Say of string option

let private lookRoom (client : Client, roomId : int) =
    let room = Rooms.getRoom client.CurrentRoom
    Rooms.lookRoom (client, roomId, true)

let private lookDirection direction =
    ()

let private lookItem keyword =
    ()

let private go (client : Client, direction) =
    let room = Rooms.getRoom client.CurrentRoom
    let destinationRoom = room.ExitData.[direction].DestinationRoomId
    Rooms.addToRoom (client, destinationRoom)
    lookRoom (client, destinationRoom)

let private saveCharacter client =
    ()

let private saveItem keyword =
    ()

let private saveRoom roomId =
    ()

let private say text =
    ()

let Handle (client, inputCommand) =
    try
        match inputCommand with
        | Go (direction, keyword) ->
            match direction with
            | Some value -> go (client, value)
            | None ->
                match keyword with
                | Some word -> ""
                | None -> ""
        | Look (direction, keyword) ->
            match direction with
            | Some value -> ""
            | None ->
                match keyword with
                | Some value -> ""
                | None -> lookRoom (client, client.CurrentRoom)
        | Save keyword -> ""
        | Say text -> ""
    with
    | _ -> "I don't understand.\r\n"