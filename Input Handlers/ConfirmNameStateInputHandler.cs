using System;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;
using ShadowMUD.Server;

namespace ShadowMUD.InputHandlers
{
    internal class ConfirmNameStateInputHandler : IStateInputHandler
    {
        private CharacterManager _characterManager;

        public ConfirmNameStateInputHandler()
        {
            _characterManager = MudManagers.MudInstance.CharacterManager;
        }

        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.ConfirmNameString);

            if (command.Length == 0)
            {
                descriptor.Write("Please type Yes or No: ");

                return;
            }

            if (command.ToLower()[0] == 'y')
            {
                // TODO: check if banned

                // TODO: check if mud is wizlocked

                descriptor.Write(string.Format("Give me a password for {0}: ", descriptor.Character.Name));

                MudServer.DisableLocalEcho(descriptor);

                descriptor.State = PlayerState.CreatePassword;
            }
            else if (command.ToLower()[0] == 'n')
            {
                descriptor.Write("Okay, what IS it, then? ");

                descriptor.Character.Name = null;
                descriptor.State = PlayerState.GetName;
            }
            else
                descriptor.Write("Please type Yes or No: ");
        }

        #endregion
    }
}