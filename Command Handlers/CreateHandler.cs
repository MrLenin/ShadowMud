using System;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.CommandHandlers
{
    internal class CreateHandler : ICommandHandler
    {
        private readonly string _commandString;
        private readonly RoomManager _roomManager;

        public CreateHandler()
        {
            _roomManager = MudManagers.MudInstance.RoomManager;
            _commandString = "create";
        }

        #region ICommandHandler Members

        public void Handle(Character character, string[] arguments)
        {
            if (arguments.Length == 0)
            {
                character.Write("What do you want to create?\r\n");
                return;
            }
            switch (arguments[0])
            {
                case "room":

                    if (arguments.Length > 1)
                        ProcessCreateRoom(character, new ArraySegment<string>(arguments, 1, arguments.Length - 1));
                    else
                    {
                        character.Write("You must enter a value to create a room.\r\n");
                    }
                    break;

                default:

                    character.Write("That is not something you can change.\r\n");
                    break;
            }
        }

        public string Command
        {
            get { return _commandString; }
        }

        #endregion

        private void ProcessCreateRoom(Character character, ArraySegment<string> arguments)
        {
            if (arguments.Count == 0)
            {
                character.Write("You must enter a value to change that.\r\n");
                return;
            }

            string offsetArg = arguments.Array[arguments.Offset].ToLower();

            if (!ExitDetail.Directions.ContainsKey(offsetArg))
            {
                var arr = new ArraySegment<string>
                    (arguments.Array, arguments.Offset + 1,
                     arguments.Count - 1);

                switch (offsetArg)
                {
                    default:

                        character.Write("You must provide a valid command or direction to create a room.\r\n");
                        break;
                }
            }
            else
            {
                if (character.Room[ExitDetail.Directions[offsetArg]] != null)
                {
                    character.Write("There is already an exit in that direction.\r\n");
                    return;
                }

                var opposite = (short) ExitDetail.Opposites[ExitDetail.Directions[offsetArg]];

                var room = new Room
                {
                    Title = "New room",
                    Description = "New room description\r\n\r\n",
                    ZoneID = character.Room.ZoneID
                };

                room = _roomManager.CreateRoom(room);

                var newRoomExit = new ExitDetail
                {
                    RoomID = room.ID,
                    ExitDirection = opposite,
                    Description = string.Empty,
                    Flags = 0,
                    KeyID = 0,
                    TargetRoom = character.Room.ID
                };

                _roomManager.CreateExit(room, newRoomExit);

                var exit = (short)ExitDetail.Directions[offsetArg];

                var currentRoomExit = new ExitDetail
                {
                    RoomID = character.Room.ID,
                    ExitDirection = exit,
                    Description = string.Empty,
                    Flags = 0,
                    KeyID = 0,
                    TargetRoom = room.ID
                };

                _roomManager.CreateExit(character.Room, currentRoomExit);

                character.Room.Look(character, true);
            }
        }
    }
}