module ShadowMud.Characters

open System

open Microsoft.FSharp.Core.LanguagePrimitives

open ShadowMud.Data
open ShadowMud.Crypt

type Attributes =
    {
        Agility : int;
        Body : int;
        Reaction : int;
        Strength : int;
        Charisma : int;
        Intuition : int;
        Logic : int;
        Willpower : int;
        Magic : int;
        Resonance : int;
        Essence : int;
        Edge : int
    }

type State =
    {
        CurrentRoom : int;
        PhysicalMonitor : int;
        StunMonitor : int
    }

type Data =
    {
        Description : string;
        Gender : Gender;
        Height : int;
        Id : int;
        Awakened : Awakened;
        Metatype : Metatype;
        Name : string;
        Password : string;
        Title : string;
        Weight : int
    }

type Character =
    {
        Attributes : Attributes;
        Data : Data;
        Finances : Map<Currency, int>;
        State : State
    }

let newData : Data = { Description = String.Empty; Gender = Gender.Male; Height = 0; Id = 0; Awakened = Awakened.Unawakened; Metatype = Metatype.Human; Name = String.Empty; Password = String.Empty; Title = String.Empty; Weight = 0 }
let newState : State = { CurrentRoom = 1; PhysicalMonitor = 1; StunMonitor = 1 }
let newCharacter (attributes, data, nuyen, state) : Character = { Attributes = attributes; Data = data; Finances = Map.empty.Add(Currency.Nuyen, nuyen); State = state }

type private ClientCommands =
    | EnqueueInput of string
    | DequeueInput of AsyncReplyChannel<string>
    | GetInputPending of AsyncReplyChannel<bool>
    | GetCurrentRoom of AsyncReplyChannel<int>
    | SetCurrentRoom of (int * AsyncReplyChannel<unit>)
    | GetCharacter of AsyncReplyChannel<Character>
    | SetCharacter of (Character * AsyncReplyChannel<unit>)
    | GetSessionId of AsyncReplyChannel<Guid>
    | SetSessionId of (Guid * AsyncReplyChannel<unit>)

type Client (character : Character, sessionId : Guid) =
    let agent = Agent.Start (fun inbox ->
        let rec loop (character : Character, sessionId, inputQueue : StringQueue, outputList : string list) =
            async { let! msg = inbox.Receive()
                    match msg with
                    | EnqueueInput input ->
                        return! loop (character, sessionId, inputQueue.Enqueue(input), outputList)
                    | DequeueInput rc ->
                        if inputQueue.IsEmpty then
                            rc.Reply String.Empty
                            return! loop (character, sessionId, inputQueue, outputList)
                        else
                            let queue, input = inputQueue.Take ()
                            rc.Reply (input)
                            return! loop (character, sessionId, queue, outputList)
                    | GetInputPending rc ->
                        rc.Reply (not (inputQueue.IsEmpty))
                        return! loop (character, sessionId, inputQueue, outputList)
                    | GetCurrentRoom rc ->
                        rc.Reply character.State.CurrentRoom
                        return! loop (character, sessionId, inputQueue, outputList)
                    | SetCurrentRoom (roomId, rc) ->
                        let result = { character with State = { character.State with CurrentRoom = roomId } }
                        rc.Reply ()
                        return! loop (result, sessionId, inputQueue, outputList)
                    | GetCharacter rc ->
                        rc.Reply character
                        return! loop (character, sessionId, inputQueue, outputList)
                    | SetCharacter (newCharacter, rc) ->
                        rc.Reply ()
                        return! loop (newCharacter, sessionId, inputQueue, outputList)
                    | GetSessionId rc ->
                        rc.Reply sessionId
                        return! loop (character, sessionId, inputQueue, outputList)
                    | SetSessionId (sessionId, rc) ->
                        rc.Reply ()
                        return! loop (character, sessionId, inputQueue, outputList)
                  }
        loop (character, sessionId, StringQueue.Empty (), List.Empty))

    member p.InputPending with get () = agent.PostAndReply (fun rc -> GetInputPending rc)    
    member p.InputQueue with get () = agent.PostAndReply (fun rc -> DequeueInput rc) and set value = agent.Post (EnqueueInput value)
    member p.CurrentRoom with get () = agent.PostAndReply (fun rc -> GetCurrentRoom rc) and set value = agent.PostAndReply (fun rc -> SetCurrentRoom (value, rc))
    member p.Character with get () = agent.PostAndReply (fun rc -> GetCharacter rc) and set value = agent.PostAndReply (fun rc -> SetCharacter (value, rc))
    member p.SessionId with get () = agent.PostAndReply (fun rc -> GetSessionId rc) and set value = agent.PostAndReply (fun rc -> SetSessionId (value, rc))

let private edmConnectionString = "metadata=res://*/ShadowMud.csdl|res://*/ShadowMud.ssdl|res://*/ShadowMud.msl;provider=System.Data.SqlServerCe.4.0;provider connection string='data source=|DataDirectory|..\..\..\ShadowMudlib\EntityModel\ShadowMud.sdf;password=q0p1w9o2e8;persist security info=True;'"
let private context = new EntityModel.Entities(edmConnectionString)

let private getCharacterByName (name : string) =
    query { for i in context.Characters do
            where (i.Name.ToLower () = name.ToLower ())
            select i } |> Seq.head
            
let getCharacterRecord(character : EntityModel.Character, finances : List<EntityModel.Finance>, attributes : EntityModel.Attribute, state : EntityModel.State) =
    {   Data = { Description = character.Description; Gender = EnumOfValue character.Gender; Height = character.Height; Id = character.Id; Metatype = EnumOfValue character.Metatype
                 Awakened = EnumOfValue character.Awakened; Name = character.Name; Password = character.Password; Title = character.Title; Weight = character.Weight }

        Attributes = { Agility = attributes.Agility; Body = attributes.Body; Reaction = attributes.Reaction; Strength = attributes.Strength;
                       Charisma = attributes.Charisma; Intuition = attributes.Intuition; Logic = attributes.Logic; Willpower = attributes.Willpower;
                       Magic = attributes.Magic; Resonance = attributes.Resonance; Essence = attributes.Essence; Edge = attributes.Edge };
     
        Finances = finances |> List.fold (fun state finances -> state.Add (EnumOfValue finances.Currency, finances.Amount)) Map.empty;
       
        State = { CurrentRoom = state.CurrentRoom; PhysicalMonitor = state.PhysicalMonitor; StunMonitor = state.StunMonitor }
    }

let saveCharacterRecord (character : Character) =
    try
        let chardata = getCharacterByName character.Data.Name
        

        let attributes, state =
            query {    for i in context.Attributes do
                       join (for x in context.States -> chardata.Id = x.Id)
                       where (i.Id = chardata.Id)
                       select (i,x)
                  } |> Seq.head

        let finances = query { for i in context.Finances do
                               where (i.CharacterId = chardata.Id)
                               select i
                             } |> Seq.toList

        chardata.Description <- character.Data.Description
        chardata.Gender <- EnumToValue character.Data.Gender
        chardata.Height <- character.Data.Height
        chardata.Awakened <- EnumToValue character.Data.Awakened
        chardata.Metatype <- EnumToValue character.Data.Metatype
        chardata.Name <- character.Data.Name
        chardata.Password <-character.Data.Password
        chardata.Title <- character.Data.Title
        chardata.Weight <- character.Data.Weight

        attributes.Agility <- character.Attributes.Agility
        attributes.Body <- character.Attributes.Body
        attributes.Reaction <- character.Attributes.Reaction
        attributes.Strength <- character.Attributes.Strength
        attributes.Charisma <- character.Attributes.Charisma
        attributes.Intuition <- character.Attributes.Intuition
        attributes.Logic <- character.Attributes.Logic
        attributes.Willpower <- character.Attributes.Willpower
        attributes.Magic <- character.Attributes.Magic
        attributes.Resonance <- character.Attributes.Resonance
        attributes.Essence <- character.Attributes.Essence
        attributes.Edge <- character.Attributes.Edge

        state.StunMonitor <- character.State.StunMonitor
        state.PhysicalMonitor <- character.State.PhysicalMonitor
        state.CurrentRoom <- character.State.CurrentRoom

        //let currencyList = [ Currency.Dollars, Currency.Euro, Currency.Yen, Currency.Credits ]
        //for currency in currencyList then
          //  match finances |> Map.containsKey currency with
            //| true -> 

    with
        | exn ->
            Console.WriteLine "Save Error - Character does not exist"
            reraise ()


let private loadCharacterData name =
    try
        let character = getCharacterByName name
        

        let attributes, state =
            query {    for i in context.Attributes do
                       join (for x in context.States -> character.Id = x.Id)
                       select (i,x)
                  } |> Seq.head

        let finances = query { for i in context.Finances do
                               where (i.CharacterId = character.Id)
                               select i
                             } |> Seq.toList

        getCharacterRecord(character, finances, attributes, state)
        
    with
        | exn ->
            Console.WriteLine "Error - Character not found"
            reraise ()

let private characterExists (name : string)=
    not (query { for i in context.Characters do
                 where (i.Name.ToLower () = name.ToLower ())
                 select i
               } |> Seq.isEmpty)

let private testPassword (name, password) : bool =
    let characterPassword =
        try
             (getCharacterByName name).Password
        with
            | exn ->
                Console.WriteLine "Error - Character not found"
                reraise()

    BCrypt.CheckPassword (password, characterPassword)

type private CharacterCommands =
    | CharacterExists of (string * AsyncReplyChannel<bool>)
    | CheckPassword of (string * string * AsyncReplyChannel<bool>)
    | LoadCharacter of (string * AsyncReplyChannel<Character>)
    | Exit
    
let private agent = Agent.Start (fun inbox ->
    let rec loop () =
        async { let! msg = inbox.Receive ()
                match msg with
                | CharacterExists (name, rc) ->
                    rc.Reply (characterExists name)
                    return! loop ()
                | LoadCharacter (name, rc) ->
                    rc.Reply (loadCharacterData name)
                    return! loop ()
                | Exit ->
                    return ()
                | CheckPassword (name, password, rc) ->
                    rc.Reply (testPassword (name, password))
                    return! loop () }
    loop ())

let checkCharacter name = agent.PostAndReply (fun rc -> CharacterExists (name, rc))
let checkPassword (name, password) = agent.PostAndReply (fun rc -> CheckPassword (name, password, rc))
let loadCharacter client = agent.PostAndReply (fun rc -> LoadCharacter (client, rc))