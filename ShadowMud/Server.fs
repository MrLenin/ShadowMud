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

type private LoginInfo = Login.LoginInfo
type private LoginState = Login.LoginState


type private System.Net.Sockets.TcpListener with
    member tl.AsyncAcceptTcpClient () =
        Async.FromBeginEnd (tl.BeginAcceptTcpClient, tl.EndAcceptTcpClient)

type private System.Net.Sockets.Socket with
    static member IsConnected (socket : Socket) =
        try
          not (socket.Poll (1, SelectMode.SelectRead) && socket.Available = 0)
        with _ -> false

type private ClientTableCommands =
    | Add of (Guid * NetworkStream)
    | Remove of (Guid * AsyncReplyChannel<unit>)
    | AddLogin of LoginInfo
    | RetrieveLogins of AsyncReplyChannel<LoginInfo list>
    | SendMessage of (string * Guid)
    | AddInput of (Guid * string)
    | RetrieveInput of AsyncReplyChannel<Map<Guid, string list>>

let private listener = new TcpListener (IPAddress.Loopback, 4242)
let private sendBufferSize = 24*1024

let private agent = Agent.Start (fun inbox ->
    let rec loop (clientMap : Map<Guid, NetworkStream>, inputMap : Map<Guid, string list>, logins : LoginInfo list) =
        async { let! msg = inbox.Receive ()
                match msg with
                | SendMessage (msg, clientId) ->
                    match clientMap.TryFind clientId with
                    | Some stream ->
                        if stream.CanWrite then
                            let buffer = Encoding.ASCII.GetBytes (msg.ToCharArray ())
                            do! stream.AsyncWrite buffer
                            return! loop (clientMap, inputMap, logins)
                        else
                            return! loop (clientMap, inputMap, logins)

                    | None ->  return! loop (clientMap, inputMap, logins)

                | AddInput (sessionId, input) ->
                    match inputMap |> Map.tryFind sessionId with
                    | Some inputList -> return! loop (clientMap, inputMap.Add (sessionId, input :: inputList), logins)
                    | None -> return! loop (clientMap, inputMap.Add (sessionId, input :: List.Empty), logins)

                | RetrieveInput rc ->
                    if inputMap.IsEmpty then
                        rc.Reply inputMap
                        return! loop (clientMap, inputMap, logins)
                    else
                        rc.Reply inputMap
                        return! loop (clientMap, Map.empty, logins)

                | Add (sessionId, stream) ->
                    return! loop (clientMap |> Map.add sessionId stream, inputMap, logins)
                | AddLogin loginInfo ->
                    let newlogins = loginInfo :: logins
                    return! loop (clientMap, inputMap, newlogins)
                | RetrieveLogins rc ->
                    rc.Reply logins
                    return! loop (clientMap, inputMap, List.Empty)
                | Remove (sessionId, rc) ->
                    match clientMap |> Map.tryFindKey (fun key value -> key = sessionId) with
                    | Some sessionId -> 
                        rc.Reply ()
                        return! loop (clientMap |> Map.remove sessionId, inputMap, logins)
                    | None ->
                        rc.Reply ()
                        return! loop (clientMap, inputMap, logins) }
    loop (Map.empty, Map.empty, List.Empty))

let private addLogin loginInfo = agent.Post (AddLogin loginInfo)
let private addInput (sessionId, msg) = agent.Post (AddInput (sessionId, msg))

let AddClient (loginInfo, stream) = agent.Post (Add (loginInfo, stream))
let RemoveClient sessionId = agent.PostAndReply (fun rc -> Remove (sessionId, rc))
let RetrieveInput () = agent.PostAndReply (fun rc -> RetrieveInput rc)
let RetrieveLogins () = agent.PostAndReply (fun rc -> RetrieveLogins rc)
let SendMessage (dest, msg) = agent.Post (SendMessage (dest, msg))

let private handleClient (connection : TcpClient) =
    async { let ipAddress = connection.Client.RemoteEndPoint.ToString ()
            let hostname = String.Empty//try (Dns.GetHostEntry ipAddress).HostName // this shit slows down connecting
                            //with | exn -> String.Empty

            let loginInfo = { Login.newLoginInfo with IpAddress = ipAddress; Hostname = hostname }
            let networkStream = connection.GetStream ()

            AddClient (loginInfo.SessionId, networkStream)
            addLogin loginInfo

            let rec closeConnection (stream : NetworkStream) =
                async { RemoveClient loginInfo.SessionId
                        stream.Close ()
                        stream.Dispose ()
                        connection.Close ()
                        return () }

            let rec asyncReadStream (stream : NetworkStream) =
                async { match Socket.IsConnected connection.Client with
                        | false -> return! closeConnection stream
                        | true -> let buffer = Array.create 1024 0uy
                                  let! read = stream.AsyncRead (buffer, 0, 1024)
                                  let allText = Encoding.ASCII.GetString (buffer, 0, read)

                                  match allText with
                                  | "\0" -> return! closeConnection stream
                                  | "\r\n" -> return! asyncReadStream stream
                                  | _ -> addInput (loginInfo.SessionId, allText.Replace (Environment.NewLine, ""))
                                         return! asyncReadStream stream }

            SendMessage ("What is your name?\r\n", loginInfo.SessionId)
            return! asyncReadStream (networkStream) }

let rec private handleConnections () =
    async { match listener.Pending () with
            | true ->
                let! connection = listener.AsyncAcceptTcpClient ()
                do! handleClient connection
                return! handleConnections ()
            | false ->
                do! Async.Sleep 1
                return! handleConnections () }

let Start () =
    async { listener.Server.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true)
            listener.Server.LingerState.Enabled <- false
            listener.Server.LingerState.LingerTime <- 0
            listener.Server.SendBufferSize <- sendBufferSize
        
            listener.Start ()
            return! handleConnections () }

