module ShadowMud.DiceRoller

type private Byte = System.Byte
type private RNGCryptoServiceProvider = System.Security.Cryptography.RNGCryptoServiceProvider

let private numSides = 6uy

type DiceRoll = 
    {   Rolls : List<int>;
        HitRolls : List<int>
        NumHits : int;
        RuleOfOne : bool }

let private isFairRoll roll =
    roll < numSides * (Byte.MaxValue / numSides)

let rec private rollDie rngCsp =
    let rngCsp : RNGCryptoServiceProvider = rngCsp
    
    let mutable randomNumber = Array.init 1 (fun i -> 0uy)

    rngCsp.GetBytes(randomNumber)

    match isFairRoll randomNumber.[0] with
    | true -> int ((randomNumber.[0] % numSides) + 1uy)
    | false -> rollDie rngCsp

let rec private rollRuleOfSixDie (rngCsp, targetNum, prevResult) =
    let roll = rollDie rngCsp
    let result = roll + prevResult

    match roll = 6 && targetNum > result with
    | false -> result
    | true -> rollRuleOfSixDie (rngCsp, targetNum, result)


let RollDice(numberDice, targetNumber, ruleOfSix) =
    use rngCsp = new RNGCryptoServiceProvider ()
    
    let results = List.init numberDice (fun i ->
        match ruleOfSix with
        | false -> rollDie rngCsp
        | true -> rollRuleOfSixDie (rngCsp, targetNumber, 0))
    
    let hitResults = results
                   |> List.filter (fun i -> i >= targetNumber)

    { Rolls = results;
      HitRolls = hitResults;
      NumHits = hitResults.Length;
      RuleOfOne = results |> List.forall (fun i -> i = 1) }
