module ShadowMud.Data

open System
open System.Data.Entity

open Microsoft.FSharp.Data.TypeProviders

type Table =
    | Characters = 0
    | Rooms = 1
    | Objects = 2
    | Zones = 3
    | Texts = 4

type Gender =
    | None = 0
    | Male = 1
    | Female = 2

type Currency =
    | Dollars = 0
    | Euro = 1
    | Nuyen = 2
    | Credits = 3

type Direction =
    | North = 0
    | Northeast = 1
    | East = 2
    | Southeast = 3
    | South = 4
    | Southwest = 5
    | West = 6
    | Northwest = 7
    | Up = 8
    | Down = 9

type Metatype =
    | Human = 0
    | Elf = 1
    | Troll = 2
    | Ork = 3
    | Dwarf = 4

type Awakened =
    | Unawakened = 0
    | FullMagician = 1
    | Adept = 2
    | Aspected = 3

type Fifo<'a when 'a : equality> = 
    new () = { xs = []; rxs = [] }
    new (xs, rxs) = { xs = xs; rxs = rxs }

    val xs : 'a list;
    val rxs : 'a list;

    static member Empty () = new Fifo<'a> ()
    member q.IsEmpty = (q.xs = []) && (q.rxs = [])
    member q.Enqueue(x) = Fifo(q.xs,x::q.rxs)
    member q.Take() = 
        if q.IsEmpty then failwith "fifo.Take: empty queue"
        else match q.xs with
                | [] -> (Fifo(List.rev q.rxs,[])).Take()
                | y::ys -> (Fifo(ys, q.rxs)),y

type Agent<'T> = MailboxProcessor<'T>
type StringQueue = Fifo<string>

// EDMX-based - Seems to provide a richer interface, more fine grained control over the generated types
type internal ShadowMudEdmx = EdmxFile< @"..\ShadowMudlib\EntityModel\ShadowMud.edmx">
type EntityModel = ShadowMudEdmx.Model

let NullableToOption (n : System.Nullable<_>) = 
   if n.HasValue then Some n.Value else None

// Live database-based - interface closer to Linq-to-SQL, generated types are strictly controlled by the underlying database relational model/naming scheme
//[<Generate>]
//type EntityConnection = SqlEntityConnection<ConnectionString="data source=|DataDirectory|C:\Programming\ShadowMud.ctp\ShadowMud\ShadowMud\ShadowMud.sdf;password=q0p1w9o2e8;persist security info=True;", Provider="System.Data.SqlServerCe.4.0">
