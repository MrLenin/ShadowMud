using System.Collections.Generic;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.CommandHandlers
{
    internal class MoveHandler : ICommandHandler
    {


        private readonly string _commandString;

        public MoveHandler()
        {
            _commandString = "move";
        }

        #region ICommandHandler Members

        public void Handle(Character character, string[] arguments)
        {
            if (arguments.Length == 0)
            {
                character.Write("Which way do you want to move?\r\n");
                return;
            }

            var direction = arguments[0].ToLower();

            if (!Directions.ContainsKey(direction)) return;

            if (character.Room.ContainsExit(Directions[direction]))
            {
                var exitInfo = character.Room[Directions[direction]];

                MudManagers.MudInstance.RoomManager.AddCharacterTo(exitInfo.TargetRoom, character);
                MudManagers.MudInstance.RoomManager.RemoveCharacterFrom(character.Room.ID, character);

                character.ZoneID = exitInfo.ZoneID;
                character.RoomID = exitInfo.TargetRoom;

                character.Room.Look(character, false);
            }
            else
            {
                character.Write("Alas, you cannot go that way...\r\n");
                return;
            }
        }

        public string Command { get { return _commandString; } }

        #endregion
    }
}