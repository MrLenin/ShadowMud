using System;
using ShadowMUD.Crypt;
using ShadowMUD.Interpreter;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;

namespace ShadowMUD.InputHandlers
{
    internal class CreatePasswordStateInputHandler : IStateInputHandler
    {
        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.GetPasswordString);

            if (command.Length == 0 || command.Length < 5 || command.Equals(descriptor.Character.Name))
            {
                descriptor.Write("Illegal password.\r\nPassword: ");

                return;
            }

            descriptor.Character.Password = BCrypt.HashPassword(command, BCrypt.GenerateSalt(10));

            descriptor.Write("Please retype password: ");

            descriptor.State = (descriptor.State != PlayerState.GetNewPassword)
                                   ? PlayerState.ConfirmPassword
                                   : PlayerState.VerifyNewPassword;
        }

        #endregion
    }
}