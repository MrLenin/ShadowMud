module ShadowMud.Server

type private Agent<'T> = MailboxProcessor<'T>
type private Encoding = System.Text.Encoding
type private Environment = System.Environment
type private Guid = System.Guid
type private IPAddress = System.Net.IPAddress
type private NetworkStream = System.Net.Sockets.NetworkStream
type private SelectMode = System.Net.Sockets.SelectMode
type private Socket = System.Net.Sockets.Socket
type private SocketOptionLevel = System.Net.Sockets.SocketOptionLevel
type private SocketOptionName = System.Net.Sockets.SocketOptionName
type private String = System.String
type private TcpClient = System.Net.Sockets.TcpClient
type private TcpListener = System.Net.Sockets.TcpListener


type private System.Net.Sockets.TcpListener with
    member tl.AsyncAcceptTcpClient () =
        Async.FromBeginEnd (tl.BeginAcceptTcpClient, tl.EndAcceptTcpClient)

type private System.Net.Sockets.Socket with
    static member IsConnected (socket : Socket) =
        try
          not (socket.Poll (1, SelectMode.SelectRead) && socket.Available = 0)
        with _ -> false

type private ClientTableCommands =
    | AddSession of (Guid * NetworkStream)
    | RemoveSession of (Guid * AsyncReplyChannel<unit>)
    | StoreLogin of (Guid * string)
    | RetrieveLogins of AsyncReplyChannel<Map<Guid, string>>
    | SendMessage of (string * Guid)
    | StoreInput of (Guid * string)
    | RetrieveInput of (Guid list * AsyncReplyChannel<Map<Guid, string list>>)

let private listener = new TcpListener (IPAddress.Loopback, 4242)
let private sendBufferSize = 24*1024

let private agent = Agent.Start (fun inbox ->
    let rec loop (sessionMap, inputMap, loginMap) = async {
        let sessionMap : Map<Guid, NetworkStream> = sessionMap
        let inputMap : Map<Guid, string list> = inputMap
        let loginMap : Map<Guid, string> = loginMap

        let! msg = inbox.Receive ()
        match msg with
        | SendMessage (msg, clientId) ->
            match sessionMap.TryFind clientId with
            | Some stream ->
                if stream.CanWrite then
                    let buffer = Encoding.ASCII.GetBytes (msg.ToCharArray ())
                    do! stream.AsyncWrite buffer
                    return! loop (sessionMap, inputMap, loginMap)
                else
                    return! loop (sessionMap, inputMap, loginMap)
            | None ->  return! loop (sessionMap, inputMap, loginMap)

        | StoreInput (sessionId, input) ->
            match inputMap |> Map.tryFind sessionId with
            | Some inputList ->
                return! loop (sessionMap, inputMap.Add (sessionId, input :: inputList), loginMap)
            | None ->
                return! loop (sessionMap, inputMap.Add (sessionId, input :: List.Empty), loginMap)

        | RetrieveInput (sessions, rc) ->
            let result, inputMap =
                inputMap |> Map.partition (fun key value ->
                    sessions |> List.exists (fun session -> session = key) )
            rc.Reply result
            return! loop (sessionMap, inputMap, loginMap)

        | AddSession (sessionId, stream) ->
            return! loop (sessionMap |> Map.add sessionId stream, inputMap, loginMap)

        | StoreLogin loginInfo ->
            return! loop (sessionMap, inputMap, loginMap.Add (loginInfo))

        | RetrieveLogins rc ->
            rc.Reply loginMap
            return! loop (sessionMap, inputMap, Map.empty)

        | RemoveSession (sessionId, rc) ->
            match sessionMap |> Map.tryFindKey (fun key value -> key = sessionId) with
            | Some sessionId -> 
                rc.Reply ()
                return! loop (sessionMap |> Map.remove sessionId, inputMap, loginMap)
            | None ->
                rc.Reply ()
                return! loop (sessionMap, inputMap, loginMap)
    }
    loop (Map.empty, Map.empty, Map.empty))

let private storeLogin loginInfo = agent.Post (StoreLogin loginInfo)
let private storeInput (sessionId, msg) = agent.Post (StoreInput (sessionId, msg))
let private addSession (loginInfo, stream) = agent.Post (AddSession (loginInfo, stream))

let RemoveSession sessionId = agent.PostAndReply (fun rc -> RemoveSession (sessionId, rc))
let RetrieveInput sessions = agent.PostAndReply (fun rc -> RetrieveInput (sessions, rc))
let RetrieveLogins () = agent.PostAndReply (fun rc -> RetrieveLogins rc)
let SendMessage (dest, msg) = agent.Post (SendMessage (dest, msg))

let private handleClient (connection : TcpClient) = async {
    let ipAddress = connection.Client.RemoteEndPoint.ToString ()
    let hostname = String.Empty//try (Dns.GetHostEntry ipAddress).HostName // this shit slows down connecting
                    //with | exn -> String.Empty
    let sessionId = Guid.NewGuid ()
    let networkStream = connection.GetStream ()

    addSession (sessionId, networkStream)
    storeLogin (sessionId, ipAddress)

    let closeConnection (stream : NetworkStream) =
        RemoveSession sessionId
        stream.Close ()
        stream.Dispose ()
        connection.Close ()

    let rec asyncReadStream (stream : NetworkStream) = async {
        if not (Socket.IsConnected connection.Client) then
            return closeConnection stream
        else
            let buffer = Array.create 1024 0uy
            let! read = stream.AsyncRead (buffer, 0, 1024)
            let allText = Encoding.ASCII.GetString (buffer, 0, read)

            match allText with
            | "\0" -> return closeConnection stream
            | "\r\n" -> return! asyncReadStream stream
            | "\n" -> return! asyncReadStream stream
            | _ -> storeInput (sessionId, allText.Replace (Environment.NewLine, ""))
                   return! asyncReadStream stream }

    return! asyncReadStream (networkStream) }

let rec private handleConnections () = async {
    match listener.Pending () with
    | true ->
        let! connection = listener.AsyncAcceptTcpClient ()
        do! handleClient connection
        return! handleConnections ()
    | false ->
        do! Async.Sleep 1
        return! handleConnections () }

let Start () = async {
    listener.Server.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)
    listener.Server.LingerState.Enabled <- false
    listener.Server.LingerState.LingerTime <- 0
    listener.Server.SendBufferSize <- sendBufferSize
        
    listener.Start ()
    return! handleConnections () }

