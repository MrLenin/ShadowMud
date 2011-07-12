using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using ShadowMUD.MudObjects;

namespace ShadowMUD
{
    public class PlayerDescriptor
    {
        public Character Character;
// ReSharper disable UnaccessedField.Local
        private string[] _commandHistory;
// ReSharper restore UnaccessedField.Local
        public bool HasPrompt;
        public string Hostname;
        public readonly StringBuilder InputBuffer;
        public readonly Queue<string> InputQueue;
        public string IPAddress;
        public ushort PasswordAttempts;
        public Socket Socket;
        public PlayerState State;

        public PlayerDescriptor()
        {
            InputBuffer = new StringBuilder();
            InputQueue = new Queue<string>();
            _commandHistory = new string[6];
            HasPrompt = true;

            PasswordAttempts = 0;

            State = PlayerState.GetName;
        }

        public void Write(string text)
        {
            var msg = Encoding.ASCII.GetBytes(text);
            Socket.Send(msg, msg.Length, SocketFlags.None);
        }
    }
}