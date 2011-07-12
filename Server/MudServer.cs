using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ShadowMUD.Properties;

namespace ShadowMUD.Server
{
    internal class MudServer : TcpListener
    {
        // Constant members
        private const byte IAC = 255; // Telnet Interpret As Command
        private const byte OptionEcho = 1; // Telnet ECHO Option Command
        private const byte WILL = 251; // Telnet WILL command
        private const byte WONT = 252; // Telnet WONT command


        // Non static members

        private readonly DescriptorCollection _descriptorList;

        public MudServer(DescriptorCollection descriptors, string address, Int32 port)
            : base(IPAddress.Parse(address), port)
        {
            _descriptorList = descriptors;

            Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            var option = new LingerOption(false, 0);

            Server.LingerState = option;
            Server.SendBufferSize = 24*1024;
            Server.Blocking = false;
        }

        public static void DisableLocalEcho(PlayerDescriptor descriptor)
        {
            byte[] msg = {IAC, WILL, OptionEcho};
            descriptor.Socket.Send(msg, msg.Length, SocketFlags.None);
        }

        // Turn on echoing (specific to telnet client)
        public static void EnableLocalEcho(PlayerDescriptor descriptor)
        {
            byte[] msg = {IAC, WONT, OptionEcho};
            descriptor.Socket.Send(msg, msg.Length, SocketFlags.None);
        }

        public Socket AcceptConnection()
        {
            var client = AcceptSocket();

            client.Blocking = false;
            client.SendBufferSize = 24*1024;

            return client;
        }

        public void CheckDataAvailable()
        {
            if (_descriptorList.Count > 0)
            {
                // Buffer for reading data
                var bytes = new Byte[256];

                //string data = null;

                var readList = new List<Socket>();
                var errorList = new List<Socket>();

                var deadDescriptors = new List<PlayerDescriptor>();

                foreach (var descriptor in _descriptorList)
                {
                    readList.Add(descriptor.Socket);
                    errorList.Add(descriptor.Socket);
                }

                Socket.Select(readList, null, errorList, 1000);

                foreach (var descriptor in _descriptorList)
                {
                    if (errorList.Contains(descriptor.Socket))
                    {
                        deadDescriptors.Add(descriptor);

                        continue;
                    }

                    if (!readList.Contains(descriptor.Socket))
                        continue;

                    var socket = descriptor.Socket;

                    bytes.Initialize();

                    try
                    {
                        var bytesRead = socket.Receive(bytes, socket.Available, SocketFlags.None);

                        if (bytesRead <= 0)
                        {
                            deadDescriptors.Add(descriptor);

                            continue;
                        }

                        descriptor.InputBuffer.Append(FilterTelnetCommands(bytes));
                    }
                    catch (Exception)
                    {
                        Debugger.Break();
                    }
                }

                foreach (var descriptor in deadDescriptors)
                {
                    Console.WriteLine(Resources.ConnectionClosedString, descriptor.Hostname, descriptor.IPAddress);

                    descriptor.Socket.Close();
                    _descriptorList.Remove(descriptor);
                }
            }
        }

        // Checks the received bytes for Telnet commands before we convert the bytes to a string since the ASCII
        // decoder clobbers the Interpret As Command value since it is greater than 0x7F
        private static string FilterTelnetCommands(IList<byte> bytes)
        {
            var filteredBytes = new List<byte>(50);
            var i = 0;

            // loop until we reach a null byte
            while (bytes[i] > 0)
            {
                // test for the telnet code for IAC (255)
                if (bytes[i] != 255)
                {
                    // this byte is not an IAC, so we add it to
                    // the list of filtered bytes and move on
                    filteredBytes.Add(bytes[i]);

                    i++;
                }
                else
                    // we found an IAC code so we filter out
                    // the entire command which is three bytes
                    i += 3;
            }

            // decode the filtered bytes to ASCII and return the string
            return Encoding.ASCII.GetString(filteredBytes.ToArray(), 0, filteredBytes.Count);
        }
    }
}