using System;
using ShadowMUD.Crypt;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;
using ShadowMUD.Server;

namespace ShadowMUD.InputHandlers
{
    internal class ConfirmPasswordStateInputHandler : IStateInputHandler
    {
        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.ConfirmPasswordString);

            if (descriptor.Character.Password.Equals(BCrypt.HashPassword(command, BCrypt.GenerateSalt(10))))
            {
                descriptor.Write("Passwords don't match... start over.\r\nPassword: ");

                descriptor.State = PlayerState.CreatePassword;

                return;
            }

            MudServer.EnableLocalEcho(descriptor);

            if (descriptor.State == PlayerState.ConfirmPassword)
            {
                descriptor.Write("What is your sex; Male, Female or None? ");
                descriptor.State = PlayerState.ChooseSex;
            }
            else
            {
                var output = string.Format("\r\nDone.\r\n{0}", MudManagers.MudInstance.TextManager.LoadText("menu"));
                descriptor.Write(output);
                descriptor.State = PlayerState.MainMenu;
            }
        }

        #endregion
    }
}