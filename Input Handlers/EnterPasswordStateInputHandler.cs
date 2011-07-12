using System;
using ShadowMUD.Crypt;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;
using ShadowMUD.Server;

namespace ShadowMUD.InputHandlers
{
    internal class EnterPasswordStateInputHandler : IStateInputHandler
    {
        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.GetExistingPasswordString);

            MudServer.EnableLocalEcho(descriptor);

            if (command.Length == 0)
            {
                descriptor.State = PlayerState.ConnectionClosed;
            }

            if (!BCrypt.CheckPassword(command, descriptor.Character.Password))
            {
                if (++descriptor.PasswordAttempts == 3)
                {
                    descriptor.Write("Wrong password... disconnecting.\r\n");

                    descriptor.State = PlayerState.ConnectionClosed;
                }
                else
                {
                    descriptor.Write("Wrong password.\r\nPassword: ");

                    MudServer.DisableLocalEcho(descriptor);
                }

                return;
            }

            // TODO: check bans

            // TODO: check for wizlock

            var output = string.Format("{0}\r\n*** PRESS RETURN: ",
                                          MudManagers.MudInstance.TextManager.LoadText("motd"));

            descriptor.Write(output);

            descriptor.State = PlayerState.ReadMotd;
        }

        #endregion
    }
}