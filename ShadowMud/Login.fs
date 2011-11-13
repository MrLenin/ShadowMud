module ShadowMud.Login

open System
open System.Linq

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives

open ShadowMud.Data

type private Attributes = Characters.Attributes
type private CharacterData = Characters.Data
type private CharacterState = Characters.State
type private Character = Characters.Character

type private BCrypt = Crypt.BCrypt

type Priority =
    | Attributes
    | Skills
    | Resources

type PriorityLevel =
    | PriorityA
    | PriorityB
    | PriorityC
    | PriorityD
    | PriorityE

type Attribute =
    | Agility = 0
    | Body = 1
    | Reaction = 2
    | Strength = 3
    | Charisma = 4
    | Intuition = 5
    | Logic = 6
    | Willpower = 7
    | Magic = 8
    | Resonance = 9
    | Edge = 10

type PrioritiesMap = Map<PriorityLevel, Priority option>
type AttributesMap = Map<Attribute, int>

let PriorityList = [ PriorityA; PriorityB; PriorityC; PriorityD; PriorityE ]
let attributesList =
    [ Attribute.Agility; Attribute.Body; Attribute.Reaction; Attribute.Strength; Attribute.Charisma; Attribute.Intuition;
      Attribute.Logic; Attribute.Willpower; Attribute.Magic; Attribute.Resonance; Attribute.Edge ]

let AttributesPriorityMap =
    [ PriorityA, 30; PriorityB, 27; PriorityC, 24; PriorityD, 21; PriorityE, 18 ]
    |> Map.ofList

let SkillsPriorityMap =
    [ PriorityA, 50; PriorityB, 40; PriorityC, 34; PriorityD, 30; PriorityE, 27 ]
    |> Map.ofList

let ResourcesPriorityMap =
    [ PriorityA, 1000000; PriorityB, 400000; PriorityC, 90000; PriorityD, 20000; PriorityE, 5000 ]
    |> Map.ofList

let AttributeStringMap =
    [ Attribute.Agility, "Agility"; Attribute.Body, "Body"; Attribute.Reaction, "Reaction"; Attribute.Strength, "Strength";
      Attribute.Charisma, "Charisma"; Attribute.Intuition, "Intuition"; Attribute.Logic, "Logic"; Attribute.Willpower, "Willpower";
      Attribute.Magic, "Magic"; Attribute.Resonance, "Resonance"; Attribute.Edge, "Edge" ]
    |> Map.ofList

type PrioritiesTable =
    {
        PrioritiesMap : PrioritiesMap;
        PriorityList : PriorityLevel list
    }

type AttributesTable =
    {
        AttributesMap : AttributesMap;
        AttributesList : Attribute list;
        SetAttributesList : Attribute list
    }

type CreateData =
    {
        Priorities : PrioritiesTable;
        Attributes : AttributesTable;
        AttributePoints : int;
        Nuyen : int;
        SkillPoints : int
    }

type CreateDataState =
    | VerifyName of string
    | GetPassword of (string * string option)
    | GetGender of CharacterData
    | ChooseMetatype of CharacterData
    | ChooseAwakened of (CreateData * CharacterData)
    | ChoosePriority of (CreateData * CharacterData * Priority option)

type CreateAttributesState =
    | ChooseAttribute of (CreateData * CharacterData)
    | SetAttribute of (CreateData * CharacterData * Attribute)

type CreateCharacterState =
    | CreateData of CreateDataState
    | CreateAttributes of CreateAttributesState
    //| CreateSkills of int

type LoginState =
    | TestName
    | TestPassword of string
    | CreateCharacter of CreateCharacterState

type LoginInfo = 
    {   SessionId : Guid;
        Hostname : string;
        IpAddress : string;
        State : LoginState
    }

let newPrioritiesTable = { PrioritiesMap = Map.empty; PriorityList = PriorityList }
let newAttributesTable = { AttributesMap = Map.empty; AttributesList = attributesList; SetAttributesList = List.empty }
let newCreateData = { Priorities = newPrioritiesTable; Attributes = newAttributesTable; AttributePoints = 0; Nuyen = 0; SkillPoints = 0 }
let newAttributes : Attributes = { Agility = 1; Body = 1; Reaction = 1; Strength = 1; Charisma = 1; Intuition = 1; Logic = 1; Willpower = 1; Magic = 1; Resonance = 1; Essence = 1; Edge = 2 }
let newData : CharacterData = { Description = String.Empty; Gender = Gender.Male; Height = 0; Id = 0; Awakened = Awakened.Unawakened; Metatype = Metatype.Human; Name = String.Empty; Password = String.Empty; Title = String.Empty; Weight = 0 }
let newState : CharacterState = { CurrentRoom = 1; PhysicalMonitor = 1; StunMonitor = 1 }
let newCharacter (attributes, data, nuyen, state) : Character = { Attributes = attributes; Data = data; Finances = Map.empty.Add(Currency.Nuyen, nuyen); State = state }
let newLoginInfo : LoginInfo = { SessionId = Guid.NewGuid (); Hostname = String.Empty; IpAddress = String.Empty; State = TestName }

let addDummyPriority (table, level) =
    let newLevels = table.PriorityList |> List.filter(fun priorityLevel -> not (priorityLevel = level))
    { PrioritiesMap = table.PrioritiesMap.Add(level, None); PriorityList = newLevels }
            
let addPriority (table, priority) =
    let level = List.min(table.PriorityList)
    let newLevels = table.PriorityList |> List.filter(fun priorityLevel -> not (priorityLevel = level))
    { PrioritiesMap = table.PrioritiesMap.Add(level, Some priority); PriorityList = newLevels }

let currentAttributeLevel (atributesTable, attribute) =
    match atributesTable.AttributesMap.TryFind (attribute) with
    | Some value -> value
    | None -> 1

let chooseAttributeMessage (createData : CreateData) =
    let choiceFormat = "\r\n\t{0}) {1} [{2}]"
    let messageFormat = "You have {0} points remaining.{1}\r\n\r\nChoose an attribute: "
    let choices =
        createData.Attributes.AttributesList
        |> List.fold (fun (state : string) attribute ->
            state + String.Format (choiceFormat, EnumToValue attribute, AttributeStringMap.[attribute], currentAttributeLevel (createData.Attributes, attribute))
        ) String.Empty
    String.Format (messageFormat, createData.AttributePoints, choices)

let getPriorityValue (level, state, priority) =
    let attributes, skills, resources = state
    match priority with
    | Some priority ->
        match priority with
        | Attributes -> (AttributesPriorityMap.[level], skills, resources)
        | Skills -> (attributes, SkillsPriorityMap.[level], resources)
        | Resources -> (attributes, skills, ResourcesPriorityMap.[level])
    | None -> state

let HandleCreateData (dataState, input, loginInfo) =
    let input : string = input

    match dataState with
    | VerifyName name ->
        if input.ToLower () = "yes" || input.ToLower () = "y" then
            let state = CreateCharacter (CreateData (GetPassword (name, None)))
            let loginInfo = { loginInfo with LoginInfo.State = state }
            let message = "What do you want your passphrase to be?.\r\n"
            (loginInfo, message)
        else
            let loginInfo = { loginInfo with State = TestName }
            let message = "So who are you then?\r\n"
            (loginInfo, message)

    | GetPassword (name, fstPassword) ->
        match fstPassword with
        | Some password ->
            if input = password then
                let data = { newData with Password = BCrypt.HashPassword(input, BCrypt.GenerateSalt(10)) }
                let state = CreateCharacter (CreateData (GetGender data))
                let loginInfo = { loginInfo with State = state }
                let message = "Passphrase verified.\r\nWhat is your gender?\r\n\t0) Asexual\r\n\t1) Male\r\n\t2) Female\r\n"
                (loginInfo, message)
            else
                let state = CreateCharacter (CreateData (GetPassword (name, None)))
                let loginInfo = { loginInfo with State = state }
                let message = "That doesn't match what you gave me, are you trying to pull something? Let's try that again:\r\n"
                (loginInfo, message)
        | None ->
            let state = CreateCharacter (CreateData (GetPassword (name, Some input)))
            let loginInfo = { loginInfo with State = state }
            let message = "Write it down a second time for your records:\r\n"
            (loginInfo, message)

    | GetGender data ->
        let data = { data with Gender = EnumOfValue (int (input)) }
        let state = CreateCharacter (CreateData (ChooseMetatype data))
        let loginInfo = { loginInfo with State = state }
        let message = "Which metatype are you?\r\n\t0) Human\r\n\t1) Elf\r\n\t2) Troll\r\n\t3) Troll\r\n\t4) Ork\r\n\t5) Dwarf\r\n"
        (loginInfo, message)

    | ChooseMetatype data ->
        let metatype = EnumOfValue (int (input))
        let priorities = match metatype with
                         | Metatype.Elf | Metatype.Troll -> addDummyPriority(newPrioritiesTable, PriorityC)
                         | Metatype.Dwarf | Metatype.Ork -> addDummyPriority(newPrioritiesTable, PriorityD)
                         | Metatype.Human -> addDummyPriority(newPrioritiesTable, PriorityE)
                         | _ -> newPrioritiesTable

        if priorities = newPrioritiesTable then
            let state =  CreateCharacter (CreateData (ChooseMetatype data))
            let loginInfo = { loginInfo with State = state }                 
            let message = "That's not a valid option, try again.\r\n"
            (loginInfo, message)
        else
            let data = { data with Metatype = metatype }
            let createData = { newCreateData with Priorities = priorities }
            let state = CreateCharacter (CreateData (ChooseAwakened (createData, data)))
            let loginInfo = { loginInfo with State = state }
            let message = "What best describes your magical abilities?\r\n\t0) Unawakened\r\n\t1) Full Magician\r\n" +
                          "\t2) Adept\r\n\t3) Aspected Magician\r\n"
            (loginInfo, message)

    | ChooseAwakened (createData, data) ->
        let awakened = EnumOfValue (int (input))
        let priorities = match awakened with
                         | Awakened.Adept | Awakened.Aspected -> addDummyPriority(createData.Priorities, PriorityB)
                         | Awakened.FullMagician -> addDummyPriority(createData.Priorities, PriorityA)
                         | Awakened.Unawakened -> 
                            if createData.Priorities.PrioritiesMap.ContainsKey(PriorityE) then
                                addDummyPriority(createData.Priorities, PriorityD)
                            else
                                addDummyPriority(createData.Priorities, PriorityE)
                         | _ -> createData.Priorities

        if priorities = createData.Priorities then
            let state = CreateCharacter (CreateData (ChooseAwakened (createData, data)))
            let loginInfo = { loginInfo with State = state }
            let message = "That's not a valid option, try again.\r\n"
            (loginInfo, message)
        else
            let data = { data with Awakened = awakened }
            let state = CreateCharacter (CreateData (ChoosePriority (createData, data, None)))
            let loginInfo = { loginInfo with State = state }
            let message = "Which of following statements best describes you?\r\n\t0) Naturally gifted, either physically or mentally.\r\n" +
                          "\t1) Highly skilled in many fields.\r\n\t2) Independently wealthy or from a wealthy family.\r\n"
            (loginInfo, message)

    | ChoosePriority (createData, data, priority) ->
        let prioritiesFormat = "Which of these following statements best describes you?\r\n\t0) {0}\r\n\t1) {1}\r\n"
        let skills = "Highly skilled in many fields."
        let attributes = "Naturally gifted, either physically or mentally."
        let resources = "Independently wealthy or from a wealthy family."

        let choosePriority (createData, priority) =
            let priorities = addPriority (createData.Priorities, priority)
            let createData = { createData with Priorities = priorities }
            let state = CreateCharacter (CreateData (ChoosePriority (createData, data, Some priority)))
            { loginInfo with State = state }

        let choosePriorities (createData, fstPriority, sndPriority) =
            let priorities = addPriority (addPriority (createData.Priorities, fstPriority), sndPriority)
            let attributes, skills, resources =
                priorities.PrioritiesMap
                |> Map.fold (fun state level priority ->
                    getPriorityValue (level, state, priority)
                ) (0, 0, 0)
            let createData = { createData with Priorities = priorities; AttributePoints = attributes; Nuyen = resources; SkillPoints = skills }
            let state = CreateCharacter (CreateAttributes (ChooseAttribute (createData, data)))
            ({ loginInfo with State = state }, chooseAttributeMessage createData)

        match priority with
        | Some priority ->
            match int (input) with
            | 0 ->
                match priority with
                | Attributes -> choosePriorities(createData, Skills, Resources)
                | Skills -> choosePriorities(createData, Attributes, Resources)
                | Resources -> choosePriorities(createData, Attributes, Skills)
            | 1 ->
                match priority with
                | Attributes -> choosePriorities(createData, Resources, Skills)
                | Skills -> choosePriorities(createData, Resources, Attributes)
                | Resources -> choosePriorities(createData, Skills, Attributes)
            | _ -> let failState = CreateCharacter (CreateData (ChoosePriority (createData, data, Some priority)))
                   let loginInfo = { loginInfo with State = failState }
                   let message = "That's not a valid option, try again.\r\n"
                   (loginInfo, message)
        | None ->
            match int (input) with
            | 0 -> let message = String.Format(prioritiesFormat, skills, resources)
                   (choosePriority (createData, Attributes), message)
            | 1 -> let message = String.Format(prioritiesFormat, attributes, resources)
                   (choosePriority (createData, Skills), message)
            | 2 -> let message = String.Format(prioritiesFormat, attributes, skills)
                   (choosePriority (createData, Resources), message)
            | _ -> let failState = CreateCharacter (CreateData (ChoosePriority (createData, data, None)))
                   let loginInfo = { loginInfo with State = failState }
                   let message = "That's not a valid option, try again.\r\n"
                   (loginInfo, message)

let HandleCreateAttributes (attributeState, input, loginInfo) =
    let attributeState : CreateAttributesState = attributeState
    let input : string = input

    match attributeState with
    | ChooseAttribute (createData, characterData) ->
        let attribute : Attribute = EnumOfValue (int (input))
        let setFormat = "{0} is currently set to {1}, what do you wish to set it to? "

        match attribute with
        | Attribute.Agility | Attribute.Body | Attribute.Reaction | Attribute.Strength | Attribute.Charisma
        | Attribute.Logic | Attribute.Willpower | Attribute.Magic | Attribute.Resonance | Attribute.Edge ->
            let state = CreateCharacter (CreateAttributes (SetAttribute (createData, characterData, attribute)))
            let message = String.Format (setFormat, AttributeStringMap.[attribute], currentAttributeLevel (createData.Attributes, attribute))
            ({ loginInfo with LoginInfo.State = state }, message)
        | _ ->
            let state = CreateCharacter (CreateAttributes (ChooseAttribute (createData, characterData)))
            ({ loginInfo with LoginInfo.State = state }, chooseAttributeMessage createData)
        
    | SetAttribute (createData, characterData, attribute) ->
        let value = int (input)
        if value > createData.AttributePoints then
            let message = "You do not have enough points remaining for that selection. "
            (loginInfo, message)
        else if value < 1 then
            let message = "You must enter a value of at least one. "
            (loginInfo, message)
        else
            let attributesTable = createData.Attributes
            let attributesTable = { attributesTable with AttributesMap = attributesTable.AttributesMap.Add(attribute, value) }
            let createData = { createData with Attributes = attributesTable }
            let state = CreateCharacter (CreateAttributes (ChooseAttribute (createData, characterData)))
            ({ loginInfo with State = state }, chooseAttributeMessage createData)