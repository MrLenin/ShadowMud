module ShadowMud.Characters

open System
open System.Linq

open Microsoft.FSharp.Core.LanguagePrimitives
//open Microsoft.FSharp.Linq

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
        Magic : int option;
        Resonance : int option;
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

let newAttributes = { Agility = 1; Body = 1; Reaction = 1; Strength = 1; Charisma = 1; Intuition = 1; Logic = 1; Willpower = 1; Magic = None; Resonance = None; Essence = 6; Edge = 1 }
let newData : Data = { Description = String.Empty; Gender = Gender.Male; Height = 0; Id = 0; Awakened = Awakened.Unawakened; Metatype = Metatype.Human; Name = String.Empty; Password = String.Empty; Title = String.Empty; Weight = 0 }
let newState : State = { CurrentRoom = 1; PhysicalMonitor = 1; StunMonitor = 1 }

let currencyList =
    [ Currency.Credits; Currency.Dollars; Currency.Euro; Currency.Nuyen ]

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
        let rec loop (character : Character, sessionId, inputQueue : StringQueue, outputList : string list) = async {
            let! msg = inbox.Receive()
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

            | SetCharacter (character, rc) ->
                rc.Reply ()
                return! loop (character, sessionId, inputQueue, outputList)

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
context.Connection.Open ()

type private RefreshMode = System.Data.Objects.RefreshMode
type private UpdateException = System.Data.UpdateException
type private OptimisticConcurrencyException = System.Data.OptimisticConcurrencyException

type private System.Data.Objects.ObjectContext with
    member oc.SaferSaveChanges () =
        try oc.SaveChanges () |> ignore
        with
        | :? OptimisticConcurrencyException ->
            oc.Refresh (RefreshMode.ClientWins, context.Characters)
            oc.SaveChanges ()
            |> ignore
            Console.WriteLine ("OptimisticConcurrencyException " +
                "handled and changes saved")

//exception MalformedCharacter of string
//exception CharacterNotFound of string

let private getCharacterEntities (name : string) =
    try query {
        for i in context.Characters.Include("Attributes").Include("Finances").Include ("State") do
            where (i.Name.ToLower () = name.ToLower ())
            exactlyOne }
    with
    | exn ->
        Console.WriteLine "Error - Character not found in database"
        reraise()
            
let private getCharacterRecord (character : EntityModel.Character) =
    let attributes = character.Attributes
    let finances = character.Finances
    let state = character.State

    {   Data =
            { Description = character.Description; Gender = EnumOfValue character.Gender; Height = character.Height; Id = character.Id; Metatype = EnumOfValue character.Metatype
              Awakened = EnumOfValue character.Awakened; Name = character.Name; Password = character.Password; Title = character.Title; Weight = character.Weight }
        Attributes =
            { Agility = attributes.Agility; Body = attributes.Body; Reaction = attributes.Reaction; Strength = attributes.Strength;
              Charisma = attributes.Charisma; Intuition = attributes.Intuition; Logic = attributes.Logic; Willpower = attributes.Willpower;
              Magic = NullableToOption attributes.Magic; Resonance = NullableToOption attributes.Resonance; Essence = attributes.Essence; Edge = attributes.Edge }
        Finances =
            finances.ToArray ()
            |> List.ofArray
            |> List.fold (fun state finances -> state.Add (EnumOfValue finances.Currency, finances.Amount)) Map.empty
        State =
            { CurrentRoom = state.CurrentRoom; PhysicalMonitor = state.PhysicalMonitor; StunMonitor = state.StunMonitor }
    }

let private transferToCharacterEntity (data, characterEntity : EntityModel.Character) =
    characterEntity.Description <- data.Description
    characterEntity.Gender <- EnumToValue data.Gender
    characterEntity.Height <- data.Height
    characterEntity.Awakened <- EnumToValue data.Awakened
    characterEntity.Metatype <- EnumToValue data.Metatype
    characterEntity.Name <- data.Name
    characterEntity.Password <- data.Password
    characterEntity.Title <- data.Title
    characterEntity.Weight <- data.Weight

let private transferToAttributesEntity (attributes, attributesEntity : EntityModel.Attributes) =
    attributesEntity.Agility <- attributes.Agility
    attributesEntity.Body <- attributes.Body
    attributesEntity.Reaction <- attributes.Reaction
    attributesEntity.Strength <- attributes.Strength
    attributesEntity.Charisma <- attributes.Charisma
    attributesEntity.Intuition <- attributes.Intuition
    attributesEntity.Logic <- attributes.Logic
    attributesEntity.Willpower <- attributes.Willpower
    attributesEntity.Essence <- attributes.Essence
    attributesEntity.Edge <- attributes.Edge
    attributesEntity.Magic <-
        match attributes.Magic with
        | Some value -> new Nullable<int> (value)
        | None -> new Nullable<int> ()
    attributesEntity.Resonance <-
        match attributes.Resonance with
        | Some value -> new Nullable<int> (value)
        | None -> new Nullable<int> ()

let private transferToStateEntity (state, stateEntity : EntityModel.State) =
    stateEntity.StunMonitor <- state.StunMonitor
    stateEntity.PhysicalMonitor <- state.PhysicalMonitor
    stateEntity.CurrentRoom <- state.CurrentRoom

let private transferToFinancesEntities (finances, financeEntities : EntityModel.Finances list) =
    finances |> Map.iter (fun currency amount ->
        let entity = financeEntities |> List.find (fun elem -> elem.Currency = EnumToValue currency)
        entity.Amount <- amount)

let private updateCharacterTable (character, characterEntity) =
    let characterEntity : EntityModel.Character = characterEntity
    let attributesEntity = characterEntity.Attributes
    let stateEntity = characterEntity.State
    let financeEntities = characterEntity.Finances.ToArray () |> List.ofArray

    transferToAttributesEntity (character.Attributes, attributesEntity)
    transferToCharacterEntity (character.Data, characterEntity)
    transferToFinancesEntities (character.Finances, financeEntities)
    transferToStateEntity (character.State, stateEntity)
    context.SaferSaveChanges ()

let private createCharacterEntity character =
    let characterEntity = new EntityModel.Character ()
    transferToCharacterEntity (character.Data, characterEntity)
    context.Characters.AddObject characterEntity
    context.SaferSaveChanges ()
    character, characterEntity.Id

let private createSupportingEntities (character, characterId) =
    let attributesEntity = new EntityModel.Attributes ()
    let stateEntity = new EntityModel.State ()
    
    attributesEntity.Id <- characterId
    stateEntity.Id <- characterId

    let financeEntities =
        List.rev currencyList
        |> List.fold (fun (state : EntityModel.Finances list) currency ->
            let entity = new EntityModel.Finances ()
            entity.Amount <- 0
            entity.CharacterId <- characterId
            entity.Currency <- EnumToValue currency
            entity :: state
        ) List.empty

    context.Attributes.AddObject attributesEntity
    context.States.AddObject stateEntity
    financeEntities |> List.iter (fun financeEntity -> context.Finances.AddObject financeEntity)

    transferToAttributesEntity (character.Attributes, attributesEntity)
    transferToFinancesEntities (character.Finances, financeEntities)
    transferToStateEntity (character.State, stateEntity)
    context.SaferSaveChanges ()

let newCharacter character =
    try
    createSupportingEntities (createCharacterEntity character)

    with
    | :? UpdateException ->
        Console.WriteLine "Save Error - Failed to add character to database"
        reraise ()

let saveCharacterRecord character =
    updateCharacterTable (character, (getCharacterEntities character.Data.Name))

let private loadCharacterData name =
    getCharacterRecord (getCharacterEntities name)

let private characterExists (name : string)=
    query { for i in context.Characters do
            select (i.Name.ToLower())
            contains (name.ToLower()) }

let private testPassword (name, password) =
    BCrypt.CheckPassword (password, (getCharacterEntities name).Password)

type private CharacterCommands =
    | CharacterExists of (string * AsyncReplyChannel<bool>)
    | CheckPassword of (string * string * AsyncReplyChannel<bool>)
    | LoadCharacter of (string * AsyncReplyChannel<Character>)
    | Exit
    
let private agent = Agent.Start (fun inbox ->
    let rec loop () = async {
        let! msg = inbox.Receive ()
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
            return! loop ()
    }

    loop ())

let checkCharacter name = agent.PostAndReply (fun rc -> CharacterExists (name, rc))
let checkPassword (name, password) = agent.PostAndReply (fun rc -> CheckPassword (name, password, rc))
let loadCharacter name = agent.PostAndReply (fun rc -> LoadCharacter (name, rc))