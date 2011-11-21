module ShadowMud.Login

open System
open System.Linq

open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.LanguagePrimitives

open ShadowMud
open ShadowMud.Data

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
    | Agility
    | Body
    | Reaction
    | Strength
    | Charisma
    | Intuition
    | Logic
    | Willpower
    | Edge
    | Magic
    | Resonance

type PrioritiesMap = Map<PriorityLevel, Priority option>
type AttributesMap = Map<Attribute, int>

let PriorityList = [ PriorityA; PriorityB; PriorityC; PriorityD; PriorityE ]
let AttributesList = [ Agility; Body; Reaction; Strength; Charisma; Intuition; Logic; Willpower; Edge ]

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
    [ Agility, "Agility"; Body, "Body"; Reaction, "Reaction"; Strength, "Strength";
      Charisma, "Charisma"; Intuition, "Intuition"; Logic, "Logic"; Willpower, "Willpower";
      Magic, "Magic"; Resonance, "Resonance"; Edge, "Edge" ]
    |> Map.ofList

type PrioritiesTable =
    {
        PrioritiesMap : PrioritiesMap;
        PriorityList : PriorityLevel list
    }

type AttributesTable =
    {
        AttributesMap : AttributesMap;
        Attributes : Attribute list;
        Points : int;
    }

type CreateData =
    {
        Name : string;
        Password : string;
        Gender : Gender;
        Metatype : Metatype;
        Awakened : Awakened;
        Priorities : PrioritiesTable;
        AttributesTable : AttributesTable;
        Nuyen : int;
        SkillPoints : int
    }

type CreateDataState =
    | VerifyName of string
    | GetPassword of (string * string option)
    | GetGender of CreateData
    | ChooseMetatype of CreateData
    | ChooseAwakened of CreateData
    | ChoosePriority of (CreateData * Priority option)

type CreateAttributesState =
    | ChooseAttribute of CreateData
    | SetAttribute of (CreateData * Attribute)

type LoginState =
    | TestName
    | TestPassword of string
    | CreateData of CreateDataState
    | CreateAttributes of CreateAttributesState
    | Authenticated of string

type LoginInfo = 
    {   SessionId : Guid;
        Hostname : string;
        IpAddress : string;
        State : LoginState
        OutputMessage : string option
    }

let newAttributesMap attrList =
    attrList
    |> List.fold (fun state attr ->
        let state : Map<Attribute, int> = state
        state.Add (attr, 1)
    ) Map.empty

let newPrioritiesTable = { PrioritiesMap = Map.empty; PriorityList = PriorityList }
let newAttributesTable = { AttributesMap = Map.empty; Attributes = AttributesList; Points = 0 }
let newCreateData = { Name = String.Empty; Password = String.Empty; Gender = Gender.None; Metatype = Metatype.Human; Awakened = Awakened.Unawakened; Priorities = newPrioritiesTable; AttributesTable = newAttributesTable; Nuyen = 0; SkillPoints = 0 }
let newLoginInfo : LoginInfo = { SessionId = Guid.NewGuid (); Hostname = String.Empty; IpAddress = String.Empty; State = TestName; OutputMessage = None }

let addDummyPriority (table, level) =
    let newLevels = table.PriorityList |> List.filter(fun priorityLevel -> not (priorityLevel = level))
    { PrioritiesMap = table.PrioritiesMap.Add(level, None); PriorityList = newLevels }
            
let addPriority (table, priority) =
    let level = List.min(table.PriorityList)
    let newLevels = table.PriorityList |> List.filter(fun priorityLevel -> not (priorityLevel = level))
    { PrioritiesMap = table.PrioritiesMap.Add(level, Some priority); PriorityList = newLevels }

let attributePoints (attrTable, attribute) =
    attrTable.AttributesMap.[attribute]

let usedPoints attrTable =
    attrTable.Attributes
    |> List.fold ( fun state attr ->
        state + attrTable.AttributesMap.[attr] ) 0

let remainingPoints (attrTable : AttributesTable) =
    attrTable.Points - usedPoints attrTable

let chooseAttributeMessage attrTable =
    let choiceFormat = "\r\n\t{0}) {1} [{2}]"
    let pointsFormat = "You recieve {0} points to spend on attributs to begin with.\r\n{1} points have been allocated so far and you have {2} points remaining."
    let messageFormat = "{0}{1}\r\n\r\nChoose an attribute: "
    let pointsMsg = String.Format (pointsFormat, attrTable.Points, usedPoints attrTable, remainingPoints attrTable)
    let _, choicesMsg =
        attrTable.Attributes
        |> List.fold (fun ( (count, msg) : (int * string) ) attribute ->
            let count = count + 1
            let msg = msg + String.Format (choiceFormat, count, AttributeStringMap.[attribute], attributePoints (attrTable, attribute))
            (count, msg)) (0, String.Empty)
    String.Format(messageFormat, pointsMsg, choicesMsg)

let getPriorityValue (level, state, priority) =
    let attributes, skills, resources = state
    match priority with
    | Some priority ->
        match priority with
        | Attributes -> (AttributesPriorityMap.[level], skills, resources)
        | Skills -> (attributes, SkillsPriorityMap.[level], resources)
        | Resources -> (attributes, skills, ResourcesPriorityMap.[level])
    | None -> state

let private handleCreateData (dataState, input, loginInfo) =
    let input : string = input

    match dataState with
    | VerifyName name ->
        if input.ToLower () = "yes" || input.ToLower () = "y" then
            { loginInfo with LoginInfo.State = CreateData (GetPassword (name, None)); OutputMessage = Some "What do you want your passphrase to be?.\r\n" }
        else
            { loginInfo with State = TestName; OutputMessage = Some "So who are you then?\r\n" }

    | GetPassword (name, fstPassword) ->
        match fstPassword with
        | Some password ->
            if input = password then
                let createData = { newCreateData with Name = name; Password = BCrypt.HashPassword(input, BCrypt.GenerateSalt(10)) }
                let message = "Passphrase verified.\r\nWhat is your gender?\r\n\t0) Asexual\r\n\t1) Male\r\n\t2) Female\r\n"
                { loginInfo with State = CreateData (GetGender createData); OutputMessage = Some message }
            else
                let message = "That doesn't match what you gave me, are you trying to pull something? Let's try that again:\r\n"
                { loginInfo with State = CreateData (GetPassword (name, None)); OutputMessage = Some message }
        | None ->
            let message = "Write it down a second time for your records:\r\n"
            { loginInfo with State = CreateData (GetPassword (name, Some input)); OutputMessage = Some message }

    | GetGender createData ->
        let createData = { createData with Gender = EnumOfValue (int (input)) }
        let message = "Which metatype are you?\r\n\t0) Human\r\n\t1) Elf\r\n\t2) Troll\r\n\t3) Troll\r\n\t4) Ork\r\n\t5) Dwarf\r\n"
        { loginInfo with State = CreateData (ChooseMetatype createData); OutputMessage = Some message }

    | ChooseMetatype createData ->
        let metatype = EnumOfValue (int (input))
        let priorities =
            match metatype with
            | Metatype.Elf | Metatype.Troll -> addDummyPriority(newPrioritiesTable, PriorityC)
            | Metatype.Dwarf | Metatype.Ork -> addDummyPriority(newPrioritiesTable, PriorityD)
            | Metatype.Human -> addDummyPriority(newPrioritiesTable, PriorityE)
            | _ -> newPrioritiesTable

        if priorities = newPrioritiesTable then
            { loginInfo with State = CreateData (ChooseMetatype createData); OutputMessage = Some "That's not a valid option, try again.\r\n" }
        else
            let createData = { newCreateData with Metatype = metatype; Priorities = priorities }
            let message = "What best describes your magical abilities?\r\n\t0) Unawakened\r\n\t1) Full Magician\r\n" +
                          "\t2) Adept\r\n\t3) Aspected Magician\r\n"
            { loginInfo with State = CreateData (ChooseAwakened createData); OutputMessage = Some message }

    | ChooseAwakened createData ->
        let awakened = EnumOfValue (int (input))
        let priorities =
            match awakened with
            | Awakened.Adept | Awakened.Aspected -> addDummyPriority(createData.Priorities, PriorityB)
            | Awakened.FullMagician -> addDummyPriority(createData.Priorities, PriorityA)
            | Awakened.Unawakened -> 
                if createData.Priorities.PrioritiesMap.ContainsKey(PriorityE) then
                    addDummyPriority(createData.Priorities, PriorityD)
                else
                    addDummyPriority(createData.Priorities, PriorityE)
            | _ -> createData.Priorities

        if priorities = createData.Priorities then
            { loginInfo with State = CreateData (ChooseAwakened createData); OutputMessage = Some "That's not a valid option, try again.\r\n" }
        else
            let attrList =
                if awakened = Awakened.Unawakened then
                    createData.AttributesTable.Attributes
                else createData.AttributesTable.Attributes @ [ Magic ] 

            let attrTable = { createData.AttributesTable with AttributesMap = newAttributesMap attrList; Attributes = attrList }
            let createData = { createData with Awakened = awakened; AttributesTable = attrTable }
            let message = "Which of following statements best describes you?\r\n\t0) Naturally gifted, either physically or mentally.\r\n" +
                          "\t1) Highly skilled in many fields.\r\n\t2) Independently wealthy or from a wealthy family.\r\n"
            { loginInfo with State = CreateData (ChoosePriority (createData, None)); OutputMessage = Some message }

    | ChoosePriority (createData, priority) ->
        let prioritiesFormat = "Which of these following statements best describes you?\r\n\t0) {0}\r\n\t1) {1}\r\n"
        let skills = "Highly skilled in many fields."
        let attributes = "Naturally gifted, either physically or mentally."
        let resources = "Independently wealthy or from a wealthy family."

        let choosePriority (createData, priority, message) =
            let priorities = addPriority (createData.Priorities, priority)
            let createData = { createData with Priorities = priorities }
            { loginInfo with State = CreateData (ChoosePriority (createData, Some priority)); OutputMessage = Some message }

        let choosePriorities (createData, fstPriority, sndPriority) =
            let priorities = addPriority (addPriority (createData.Priorities, fstPriority), sndPriority)
            let attributes, skills, resources =
                priorities.PrioritiesMap
                |> Map.fold (fun state level priority ->
                    getPriorityValue (level, state, priority)
                ) (0, 0, 0)
            let attrTable = { createData.AttributesTable with Points = attributes }
            let createData = { createData with AttributesTable = attrTable; Priorities = priorities; Nuyen = resources; SkillPoints = skills }
            { loginInfo with State = CreateAttributes (ChooseAttribute createData); OutputMessage = Some (chooseAttributeMessage attrTable) }

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
            | _ -> { loginInfo with State = CreateData (ChoosePriority (createData, Some priority)); OutputMessage = Some "That's not a valid option, try again.\r\n" }

        | None ->
            match int (input) with
            | 0 -> let message = String.Format(prioritiesFormat, skills, resources)
                   choosePriority (createData, Attributes, message)
            | 1 -> let message = String.Format(prioritiesFormat, attributes, resources)
                   choosePriority (createData, Skills, message)
            | 2 -> let message = String.Format(prioritiesFormat, attributes, skills)
                   choosePriority (createData, Resources, message)
            | _ -> { loginInfo with State = CreateData (ChoosePriority (createData, None)); OutputMessage = Some "That's not a valid option, try again.\r\n" }

let private handleCreateAttributes (attributeState, input, loginInfo) =
    let attributeState : CreateAttributesState = attributeState
    let input : string = input

    match attributeState with
    | ChooseAttribute createData ->
        let selection = int (input)
        let attrList = createData.AttributesTable.Attributes
        if selection > 0 && selection <= attrList.Length then
            let attribute = attrList.[selection - 1]
            let setFormat = "{0} is currently set to {1}, what do you wish to set it to? "
            let message = String.Format (setFormat, AttributeStringMap.[attribute], attributePoints (createData.AttributesTable, attribute))
            { loginInfo with LoginInfo.State = CreateAttributes (SetAttribute (createData, attribute)); OutputMessage = Some message }
        else
            let message = chooseAttributeMessage createData.AttributesTable
            { loginInfo with LoginInfo.State = CreateAttributes (ChooseAttribute createData); OutputMessage = Some message }
        
    | SetAttribute (createData, attribute) ->
        let value = int (input)
        if value > remainingPoints createData.AttributesTable then
            { loginInfo with OutputMessage = Some "That exceeds your remaining points. " }
        else if value < 1 then
            { loginInfo with OutputMessage = Some "You must enter a value of at least one. " }
        else
            let attrTable = { createData.AttributesTable with AttributesMap = createData.AttributesTable.AttributesMap.Add(attribute, value) }
            let createData = { createData with AttributesTable = attrTable }
            let message = chooseAttributeMessage attrTable
            { loginInfo with State = CreateAttributes (ChooseAttribute createData); OutputMessage = Some message }

let private handleTestName (input, loginInfo) =
    if Characters.checkCharacter input then
        let message = "If you are who you say you are, you should know the\r\npassword: "
        { loginInfo with State = TestPassword input; OutputMessage = Some message }
    else
        let message = String.Format("Is that so, you say your name is {0}?.\r\n", input)
        { loginInfo with State =  CreateData (VerifyName input); OutputMessage = Some message }

let private handleTestPassword (name, input, loginInfo) =
    if Characters.checkPassword (name, input) then
        let message = String.Format("Oh, why didn't you say so sooner. Welcome, {0}.\r\n", name)
        { loginInfo with State = Authenticated name; OutputMessage = Some message }
    else
        let message = "I don't think so buddy, who are you really?\r\n"
        { loginInfo with State = TestName; OutputMessage = Some message }

let private handleLoginState (loginInfo : LoginInfo, input : string) =
    match loginInfo.State with
    | Authenticated name -> loginInfo
    | CreateAttributes attributeState -> handleCreateAttributes(attributeState, input, loginInfo)
    | CreateData dataState -> handleCreateData(dataState, input, loginInfo)
    | TestName -> handleTestName (input, loginInfo)
    | TestPassword name -> handleTestPassword (name, input, loginInfo)

let private handleLogins loginMap =
    let needHandling, noHandling =
        loginMap |> Map.partition (fun key (value : StringQueue) ->
            not (value.IsEmpty) )
    
    Async.RunSynchronously (
        needHandling
        |> Map.fold (fun state key (inputQueue : StringQueue) ->
            state |> Map.add key (inputQueue.Take ())
        ) Map.empty
        |> Map.toSeq
        |> Seq.map (fun (loginInfo, (newQueue, input)) ->
            async { return (handleLoginState (loginInfo, input), newQueue) } )
        |> Async.Parallel
    )
    |> Seq.ofArray
    |> Seq.append (noHandling |> Map.toSeq)
    |> Map.ofSeq
    |> Map.fold (fun (state : Map<LoginInfo, StringQueue>) loginInfo newQueue ->
        match loginInfo.OutputMessage with
        | Some message ->
            Server.SendMessage (message, loginInfo.SessionId)
            state.Add ({ loginInfo with OutputMessage = None }, newQueue)
        | None ->
            state.Add (loginInfo, newQueue)
    ) Map.empty

let private retrieveLogins loginMap =
    Server.RetrieveLogins ()
    |> Map.fold (fun (state : Map<LoginInfo, StringQueue>) sessionId ipAddress ->
        state.Add (
            { newLoginInfo with
                SessionId = sessionId
                IpAddress = ipAddress
                OutputMessage = Some "What is your name? "
            }, StringQueue.Empty ())
    ) loginMap

let private updateInput loginMap =
    let sessions = loginMap |> Map.fold (fun state key value -> key.SessionId :: state) List.empty
    Server.RetrieveInput sessions
    |> Map.fold (fun (state : Map<LoginInfo, StringQueue>) key inputList ->
        let loginInfo, queue =
            loginMap |> Map.pick (fun loginInfo queue ->
                if loginInfo.SessionId = key then Some (loginInfo, queue) else None)
        let updatedQueue =
            inputList |> List.fold (fun (state : StringQueue) input ->
                state.Enqueue input) queue
        state.Add(loginInfo, updatedQueue)
    ) loginMap

let private retrieveAuthenticated loginMap =
    loginMap
    |> Map.fold (fun (result, ret) key value ->
        let result : Map<string, Guid * StringQueue> = result
        let ret : Map<LoginInfo, StringQueue> = ret
        match key.State with
        | Authenticated name -> (result.Add (name, (key.SessionId, value)), ret)
        | _ -> (result, ret.Add (key, value))
    ) (Map.empty, Map.empty)

type private LoginCommands =
    | UpdateLogins of AsyncReplyChannel<unit>
    | IsLoginsEmpty of AsyncReplyChannel<bool>
    | ProcessLogins
    | RetrieveAuthenticated of AsyncReplyChannel<Map<string, Guid * StringQueue>>
    
let private agent = Agent.Start (fun inbox ->
    let rec loop (loginMap : Map<LoginInfo, StringQueue>) = async {
        let! msg = inbox.Receive ()
        match msg with
        | UpdateLogins rc ->
            let loginMap = updateInput (retrieveLogins loginMap)
            rc.Reply ()
            return! loop loginMap

        | ProcessLogins ->
            return! loop (handleLogins loginMap)

        | IsLoginsEmpty rc ->
            rc.Reply loginMap.IsEmpty
            return! loop loginMap

        | RetrieveAuthenticated rc ->
            let authenticated, loginMap =
                retrieveAuthenticated loginMap
            rc.Reply authenticated
            return! loop loginMap
        }
    loop Map.empty )

let private isLoginsEmpty () = agent.PostAndReply (fun rc -> IsLoginsEmpty rc)
let private updateLogins () = agent.PostAndReply (fun rc -> UpdateLogins rc)
let private processLogins () = agent.Post ProcessLogins

let RetrieveAuthenticated () = agent.PostAndReply (fun rc -> RetrieveAuthenticated rc)

let rec private loginLoop () = async {
    updateLogins ()
    if isLoginsEmpty () then
        do! Async.Sleep 1
    else processLogins ()
    return! loginLoop ()
    }

let Start () = async { return! loginLoop () }