using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using ShadowMUD.Grammars;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;
using ShadowMUD.Server;

namespace ShadowMUD
{
    public class ShadowMud
    {
        private readonly CommandDispatcher _interpreter;
        private readonly MudServer _mudServer;
        private readonly DescriptorCollection _playerDescriptors;

        private bool _isRunning;

        public ShadowMud()
        {
            _interpreter = new CommandDispatcher();
            _playerDescriptors = new DescriptorCollection();

            _mudServer = new MudServer(_playerDescriptors, "0.0.0.0", 4003);
        }

        public void Initialize()
        {
            try
            {
                // First use of MudManager singleton, need to catch constructor exceptions
                MudManagers.MudInstance.ZoneManager.LoadZones();
            }
            catch (TypeInitializationException e)
            {
                throw new Exception("Failed to create MUD Instance.", e.InnerException);
            }

            _interpreter.InitializeStateHandlers();
            _interpreter.RegisterStateHandlers();

            _isRunning = true;
        }

        public void MainLoop()
        {
            try
            {
                // Start listening for client requests.
                _mudServer.Start();

                Console.WriteLine(Resources.ListenString);

                // Enter the listening loop.
                while (_isRunning)
                {
                    if (_mudServer.Pending())
                    {
                        CreateDescriptor();
                    }

                    if (_playerDescriptors.Count > 0)
                    {
                        // TODO: heartbeat time stuff

                        _mudServer.CheckDataAvailable();

                        ProcessInputBuffers();

                        ProcessCommands();

                        // ProcessOutput();

                        ProcessPrompts();

                        // mudServer.CloseConnections();

                        // TODO: Heartbeat stuff

                        Thread.Sleep(10);
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(Resources.SocketExceptionString, e);
            }
            finally
            {
                // Stop listening for new clients.
                _mudServer.Stop();
            }
        }

        private void ProcessPrompts()
        {
            const string prompt = "\r\n<{0}/{1}hp {2}/{3}ma {4}/{5}mv> ";

            foreach (var descriptor in _playerDescriptors)
            {
                if (!descriptor.HasPrompt)
                {
                    descriptor.Write(string.Format(prompt,
                                                   descriptor.Character.State.HitPoints,
                                                   descriptor.Character.Statistics.MaxHitPoints,
                                                   descriptor.Character.State.Mana,
                                                   descriptor.Character.Statistics.MaxMana,
                                                   descriptor.Character.State.Movement,
                                                   descriptor.Character.Statistics.MaxMovement));

                    descriptor.HasPrompt = true;
                }
            }
        }

        private void ProcessCommands()
        {
            foreach (var descriptor in _playerDescriptors)
            {
                if (descriptor.Character != null)
                {
                    descriptor.Character.WaitState -= Convert.ToUInt32((descriptor.Character.WaitState > 0));

                    if (descriptor.Character.WaitState > 0)
                        continue;
                }

                if (descriptor.InputQueue.Count == 0)
                    continue;

                if (descriptor.Character != null)
                {
                    descriptor.Character.IdleTime = 0;

                    // TODO: handle return from void

                    descriptor.Character.WaitState = 1;
                }

                string command = StripExcessWhitespace(descriptor.InputQueue.Dequeue());

                if (descriptor.State != PlayerState.Playing)
                {
                    _interpreter.Handle(descriptor, command);
                }
                else
                {
                    descriptor.HasPrompt = false;

                    var lexer = new MudInputLexer(new ANTLRStringStream(command));
                    var tokenStream = new CommonTokenStream(lexer);
                    var inputParser = new MudInputParser(tokenStream);

                    var parseResult = inputParser.commandUnit();

                    Console.WriteLine(parseResult.Tree.ToStringTree());
                    
                    var nodeStream = new CommonTreeNodeStream(parseResult.Tree) { TokenStream = tokenStream };

                    var commandWalker = new CommandWalker(nodeStream, descriptor.Character);

                    var walkResult = commandWalker.commandUnit();

                    Console.WriteLine(walkResult.Start.ToStringTree());
                }
            }
        }

        private static string StripExcessWhitespace(string command)
        {
            var sb = new StringBuilder(command.Length);

            if (command.Length == 0)
                return command;

            var i = 0;
            var lastWs = false;

            while ((i < command.Length) && char.IsWhiteSpace(command[i])) i++;
            var firstIndex = i;

            do
            {
                if (lastWs)
                {
                    lastWs = false;
                    i++;
                }

                while ((i < command.Length) && !char.IsWhiteSpace(command[i])) i++;

                var secondIndex = i;

                if (firstIndex == secondIndex)
                    return string.Empty;

                sb.Append(command.Substring(firstIndex, secondIndex - firstIndex));

                while ((i < command.Length) && char.IsWhiteSpace(command[i]))
                {
                    i++;

                    if ((i < command.Length) && !char.IsWhiteSpace(command[i]))
                    {
                        lastWs = true;
                        i--;
                        break;
                    }
                }

                firstIndex = i;
            } while (i < command.Length);

            return sb.ToString();
        }

        private void CreateDescriptor()
        {
            var client = _mudServer.AcceptConnection();

            string address = client.RemoteEndPoint.ToString();

            address = address.Substring(0, address.IndexOf(':'));

            var descriptor = new PlayerDescriptor
            {
                Socket = client,
                IPAddress = address
            };

            try
            {
                descriptor.Hostname = Dns.GetHostEntry(address).HostName;
            }
            catch (SocketException)
            {
                descriptor.Hostname = string.Empty;
            }

            _playerDescriptors.Add(descriptor);

            Console.WriteLine(Resources.ConnectionAcceptedString, descriptor.Hostname, descriptor.IPAddress);

            descriptor.Write(MudManagers.MudInstance.TextManager.LoadText("greetings"));
        }

        // props to zahlman for fixing a \b handling issue
        private void ProcessInputBuffers()
        {
            StringBuilder line;
            String buffer;

            foreach (var descriptor in _playerDescriptors)
            {
                 buffer = descriptor.InputBuffer.ToString();

                while (buffer.Contains('\r') || buffer.Contains('\n'))
                {
                    line = ExtractFullLine(buffer, descriptor);

                    for (var i = 0; i < line.Length; i++)
                    {
                        if (line[i] != '\b' && line[i] != (char) 127) continue;
                        
                        if (i > 0)
                            line.Remove(--i, 2);
                        else
                            line.Remove(i, 1);

                        --i;
                    }

                    // TODO: Don't do this here
                    if (line.ToString().Contains("quit"))
                        _isRunning = false;

                    descriptor.InputQueue.Enqueue(line.ToString());

                    descriptor.Write("\r\n");

                    buffer = descriptor.InputBuffer.ToString();
                }
            }
        }

        private static StringBuilder ExtractFullLine(string text, PlayerDescriptor descriptor)
        {
            var fullLine = new StringBuilder();

            var indexCR = text.IndexOf('\r');
            var indexLF = text.IndexOf('\n');

            int index = -1, lengthModifier = 1;

            if (indexLF >= 0 && indexCR >= 0)
            {
                index = Math.Min(indexCR, indexLF);
                lengthModifier = 2;
            }
            else if (indexCR >= 0)
            {
                index = indexCR;
            }
            else if (indexLF >= 0)
            {
                index = indexLF;
            }

            if (index != -1)
            {
                fullLine.Append(text.Substring(0, index));
                descriptor.InputBuffer.Remove(0, fullLine.Length + lengthModifier);
            }

            // this now seems like it is useless since outside of debug since we will
            // not usually be echoing input back to the client
            //fullLine.Append("\r\n");

            return fullLine;
        }

// ReSharper disable UnusedMember.Global
        internal void Shutdown(string reason)
// ReSharper restore UnusedMember.Global
        {
            _isRunning = false;

            MudManagers.ShutdownDatabase();
        }
    }
}