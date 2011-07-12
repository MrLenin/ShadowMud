using System;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;
using ShadowMUD.Server;

namespace ShadowMUD.InputHandlers
{
    internal class GetNameStateInputHandler : IStateInputHandler
    {
        private readonly CharacterManager _characterManager;

        private readonly char[] _vowels =
            {
                'a', 'e', 'i', 'o', 'u', 'y',
                'A', 'E', 'I', 'O', 'U', 'Y'
            };

        public GetNameStateInputHandler()
        {
            _characterManager = MudManagers.MudInstance.CharacterManager;
        }

        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.NameEnteredString, command);

            if (!IsValidName(command))
            {
                descriptor.Write("Invalid name, please try another.\r\nName: ");

                return;
            }

            if (_characterManager.LoadCharacter(command, out descriptor.Character))
            {
                Console.WriteLine(Resources.PlayerExistsString);

                descriptor.Character.Descriptor = descriptor;

                descriptor.Write("Password: ");

                MudServer.DisableLocalEcho(descriptor);

                descriptor.State = PlayerState.EnterPassword;
            }
            else
            {
                Console.WriteLine(Resources.PlayerNotExistString);

                descriptor.Character = new Character();

                if (IsValidName(command))
                {
                    descriptor.Character.Abilities = new CharacterAbilities();
                    descriptor.Character.Finances = new CharacterFinances();
                    descriptor.Character.Quests = new CharacterQuests();
                    descriptor.Character.Settings = new CharacterSettings();
                    descriptor.Character.State = new CharacterState();
                    descriptor.Character.Statistics = new CharacterStatistics();

                    descriptor.Character.Name = command;

                    descriptor.Write(string.Format("Did I get that right, {0} (Y/N)? ", command));

                    descriptor.State = PlayerState.ConfirmName;
                }
                else
                {
                    descriptor.Write("Invalid name, please try another.\r\nName: ");

                    return;
                }
            }
        }

        #endregion

        private bool IsValidName(string command)
        {
            if (command.Length < 2)
            {
                Console.WriteLine(Resources.NameTooShortString);

                return false;
            }
            else if (command.Length > 12)
            {
                Console.WriteLine(Resources.NameTooLongString);

                return false;
            }
            else if (ContainsInvalidChars(command))
            {
                Console.WriteLine(Resources.InvalidNameString);

                return false;
            }

            // TODO: check if name is in use

            if (command.IndexOfAny(_vowels, 0) < 0)
            {
                Console.WriteLine(Resources.NoVowelsInNameString);

                return false;
            }

            // TODO: Check if name contains a string on the invalid list

            // TODO: check for reserved words

            Console.WriteLine(Resources.ValidNameString);

            return true;
        }

        private static bool ContainsInvalidChars(string command)
        {
            for (int i = 0; i < command.Length; i++)
                if (!char.IsLetter(command, i))
                    return true;

            return false;
        }
    }
}