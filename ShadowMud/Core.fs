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
    | LoginLoop
    | RetrieveInput of AsyncReplyChannel<unit>
    | RetrieveLogins of AsyncReplyChannel<unit>
    | IsClientsEmpty of AsyncReplyChannel<bool>
    | IsLoginsEmpty of AsyncReplyChannel<bool>
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

let private agent = Agent.Start (fun inbox ->
    let rec loop (clientList : List<Client>, loginMap : Map<LoginInfo, StringQueue>) =
        async { 
            let! msg = inbox.Receive ()
            match msg with
            | RetrieveLogins rc ->
                let result =
                    Server.RetrieveLogins ()
                    |> List.fold (fun (state : Map<LoginInfo, StringQueue>) loginInfo ->
                        state.Add(loginInfo, StringQueue.Empty ())
                    ) loginMap
                rc.Reply ()
                return! loop (clientList, result)

            | RetrieveInput rc ->
                let resultOption =
                    Server.RetrieveInput ()
                    |> Map.fold (fun (ret : Map<LoginInfo, StringQueue> option) key inputList ->
                        match clientList |> List.tryFind (fun c -> c.SessionId = key) with
                        | Some client ->
                            inputList |> List.iter (fun input -> client.InputQueue <- input)
                            None
                        | None ->
                            match loginMap |> Map.tryFindKey (fun loginInfo inputQueue -> loginInfo.SessionId = key) with
                            | Some loginInfo ->
                                let queue = inputList |> List.fold (fun (state : StringQueue) input -> state.Enqueue input) loginMap.[loginInfo]
                                Some (loginMap.Add (loginInfo, queue))
                            | None -> None
                    ) None
                rc.Reply ()
                match resultOption with
                | Some map ->
                    if map.IsEmpty then return! loop (clientList, loginMap)
                    else return! loop (clientList, map)
                | None -> return! loop (clientList, loginMap)

            | ClientLoop rc ->
                let! a = clientList
                            |> List.toSeq
                            |> Seq.map (fun client -> async { return! handleClient client })
                            |> Async.Parallel
                    
                rc.Reply ()
                return! loop (clientList, loginMap)
            | LoginLoop ->
                let! resultsArray =
                    loginMap
                    |> Map.fold (fun state key inputQueue ->
                        if inputQueue.IsEmpty then
                            state |> Map.add key (inputQueue, String.Empty)
                        else
                            state |> Map.add key (inputQueue.Take ())
                        ) Map.empty
                    |> Map.toSeq
                    |> Seq.map (fun (loginInfo, (newQueue, input)) ->
                        async { if input = String.Empty then
                                    return (loginInfo, newQueue)
                                else
                                    let! loginInfoState = Login.Handle (loginInfo, input)
                                    return (loginInfoState, newQueue)
                              } )
                    |> Async.Parallel
                return! loop (resultsArray
                             |> List.ofArray
                             |> List.fold (fun (clients, logins) (loginInfo, newQueue) ->
                                 match loginInfo.OutputMessage with
                                 | Some message -> Server.SendMessage (message, loginInfo.SessionId)
                                 | None -> ()
                                 match loginInfo.State with
                                 | Authenticated name ->
                                     match clientList |> List.tryFind (fun c -> c.Character.Data.Name = name) with
                                     | Some client -> 
                                         client.SessionId <- loginInfo.SessionId
                                         Server.SendMessage (Rooms.lookRoom (client, client.CurrentRoom, true), loginInfo.SessionId)
                                         (clients, logins)
                                     | None ->
                                         let character = Characters.loadCharacter name
                                         let client = new Client (character, loginInfo.SessionId)
                                         Rooms.addToRoom (client, client.CurrentRoom)
                                         Server.SendMessage (Rooms.lookRoom (client, client.CurrentRoom, true), loginInfo.SessionId)
                                         (client :: clients, logins)
                                 | _ -> (clients, logins |> Map.add { loginInfo with OutputMessage = None } newQueue)
                             ) (List.empty, Map.empty))
            | IsClientsEmpty rc ->
                rc.Reply (clientList.IsEmpty)
                return! loop (clientList, loginMap)
            | IsLoginsEmpty rc ->
                rc.Reply loginMap.IsEmpty
                return! loop (clientList, loginMap)
            | Exit -> return ()
            | ExitGame client ->
                Server.RemoveClient(client.SessionId)
                return! loop (clientList |> List.filter (fun c -> not (c = client)), loginMap)
        }
    loop (List.empty, Map.empty))

let private isLoginsEmpty () = agent.PostAndReply (fun rc -> IsLoginsEmpty rc)
let private isClientsEmpty () = agent.PostAndReply (fun rc -> IsClientsEmpty rc)
let private clientLoop () = agent.PostAndReply(fun rc -> ClientLoop rc)
let private loginLoop () = agent.Post(LoginLoop)
let private updateLogins () = agent.PostAndReply(fun rc -> RetrieveLogins rc)
let private updateInput () = agent.PostAndReply(fun rc -> RetrieveInput rc)

let ExitGame client = agent.Post (ExitGame client)

let rec private gameLoop () =
    async {
            updateLogins ()
            updateInput ()

            if not (isLoginsEmpty ()) then
                loginLoop ()

            if not (isClientsEmpty ()) then
                clientLoop ()
                    
            do! Async.Sleep 1
            return! gameLoop ()
          }

Async.Start (Server.Start ())
Async.RunSynchronously (gameLoop ())

