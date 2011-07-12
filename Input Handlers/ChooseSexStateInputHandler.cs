using System;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Properties;

namespace ShadowMUD.InputHandlers
{
    internal class ChooseSexStateInputHandler : IStateInputHandler
    {
        private readonly CharacterManager _characterManager;

        public ChooseSexStateInputHandler()
        {
            _characterManager = MudManagers.MudInstance.CharacterManager;
        }

        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.QueryGenderString);

            switch (command[0])
            {
                case 'm':
                case 'M':

                    descriptor.Character.Gender = (short) Genders.Male;
                    command = "male";

                    break;

                case 'f':
                case 'F':

                    descriptor.Character.Gender = (short) Genders.Female;
                    command = "female";

                    break;

                case 'n':
                case 'N':

                    descriptor.Character.Gender = (0);
                    command = "genderless";

                    break;

                default:

                    descriptor.Write("That is not a sex..\r\nWhat IS your sex? ");

                    return;
            }

            var output = string.Format("You are {0}, {1}cm tall and weigh {2}kg.\r\n",
                                          command, descriptor.Character.Height, descriptor.Character.Weight);

            descriptor.Write(output);

            descriptor.Character.Initialize();

            _characterManager.CreateCharacter(descriptor.Character);
            _characterManager.LoadCharacter(descriptor.Character.Name, out descriptor.Character);

            descriptor.Character.IsNew = true;

            descriptor.Character.Descriptor = descriptor;

            output = string.Format("{0}\r\n*** PRESS RETURN: ",
                                   MudManagers.MudInstance.TextManager.LoadText("motd"));

            descriptor.Write(output);

            descriptor.State = PlayerState.ReadMotd;
        }

        #endregion
    }
}