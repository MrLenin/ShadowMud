module ShadowMud.Core

open System

open Microsoft.FSharp.Core.LanguagePrimitives
open Microsoft.FSharp.Linq
open Microsoft.FSharp.Text.Lexing

open ShadowMud
open ShadowMud.Data

open ShadowMud.Input
open ShadowMud.Login

type private Client = Characters.Client

type private GameCommands =
    | ExitGame of Client
    | ClientLoop of AsyncReplyChannel<unit>
    | UpdateClients of AsyncReplyChannel<unit>
    | IsClientsEmpty of AsyncReplyChannel<bool>
    | Exit

let private handleClientInput (client : Client) =
    match client.InputPending with
    | false -> ()
    | true ->
        let lexbuf = Lexing.LexBuffer<char>.FromString (client.InputQueue.ToLower ())
        let command = InputParser.start InputLexer.tokenize lexbuf
        Server.SendMessage(Input.Handle (client, command), client.SessionId)

let private handleClient (client : Client) =
    async { handleClientInput client }

let private retrieveClients clientList =
    let clientList : Client list = clientList

    Login.RetrieveAuthenticated ()
    |> Map.fold (fun state name (sessionId, inputQueue) ->
        let mutable inputQueue = inputQueue
        match clientList |> List.tryFind (fun c -> c.Character.Data.Name = name) with
        | Some client -> 
            client.SessionId <- sessionId
            while not (inputQueue.IsEmpty) do
                let newQueue, input = inputQueue.Take ()
                client.InputQueue <- input
                inputQueue <- newQueue
            Server.SendMessage (Rooms.lookRoom (client, client.CurrentRoom, true), sessionId)
            state
        | None ->
            let character = Characters.loadCharacter name
            let client = new Client (character, sessionId)
            while not (inputQueue.IsEmpty) do
                let newQueue, input = inputQueue.Take ()
                client.InputQueue <- input
                inputQueue <- newQueue
            Rooms.addToRoom (client, client.CurrentRoom)
            Server.SendMessage (Rooms.lookRoom (client, client.CurrentRoom, true), sessionId)
            (client :: state)
    ) clientList

let private updateInput clientList =
    let clientList : Client list = clientList

    let sessions = clientList |> List.fold (fun state client -> client.SessionId :: state) List.empty
    Server.RetrieveInput sessions
    |> Map.iter (fun key inputList ->
        inputList |> List.iter (fun input ->
            let client = clientList |> List.find (fun client -> client.SessionId = key)
            client.InputQueue <- input)
        )

let private agent = Agent.Start (fun inbox ->
    let rec loop (clientList : Client list) = async { 
        let! msg = inbox.Receive ()
        match msg with
        | UpdateClients rc ->
            let clientList = retrieveClients clientList
            updateInput clientList
            rc.Reply ()
            return! loop clientList

        | ClientLoop rc ->
            do! clientList
                |> List.toSeq
                |> Seq.map (fun client -> async { return! handleClient client })
                |> Async.Parallel
                |> Async.Ignore
            rc.Reply ()
            return! loop clientList

        | IsClientsEmpty rc ->
            rc.Reply (clientList.IsEmpty)
            return! loop clientList

        | Exit -> return ()

        | ExitGame client ->
            Server.RemoveSession(client.SessionId)
            return! loop (clientList |> List.filter (fun c -> not (c = client)))
    }

    loop List.empty)

let private isClientsEmpty () = agent.PostAndReply (fun rc -> IsClientsEmpty rc)
let private clientLoop () = agent.PostAndReply(fun rc -> ClientLoop rc)
let private updateClients () = agent.PostAndReply(fun rc -> UpdateClients rc)

let ExitGame client = agent.Post (ExitGame client)

let rec private gameLoop () = async {
    updateClients ()
    if isClientsEmpty () then
        do! Async.Sleep 1     
    else clientLoop ()
    return! gameLoop ()
}

Async.Start (Server.Start ())
Async.Start (Login.Start ())
Async.RunSynchronously (gameLoop ())

