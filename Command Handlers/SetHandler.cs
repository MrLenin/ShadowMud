using System;
using System.Globalization;
using System.Text;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.CommandHandlers
{
    internal class SetHandler : ICommandHandler
    {
        private readonly string _commandString;
        private readonly RoomManager _roomManager;

        public SetHandler()
        {
            _roomManager = MudManagers.MudInstance.RoomManager;
            _commandString = "set";
        }

        #region ICommandHandler Members

        public void Handle(Character character, string[] arguments)
        {
            if (arguments.Length == 0)
            {
                character.Write("What do you want to change?\r\n");
                return;
            }
            switch (arguments[0])
            {
                case "room":

                    if (arguments.Length > 1)
                        ProcessChangeRoom(character, new ArraySegment<string>(arguments, 1, arguments.Length - 1));
                    else
                    {
                        character.Write("Change what about the room?\r\n");
                    }
                    break;

                case "save":

                    _roomManager.SaveRoom(character.Room);
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

        private void ProcessChangeRoom(Character character, ArraySegment<string> arguments)
        {
            if (arguments.Count == 0)
            {
                character.Write("You must enter a value to change that.\r\n");
                return;
            }

            if (arguments.Count < 2)
            {
                character.Write("You must provide a new value for this setting.\r\n");
                return;
            }

            var arg1 = arguments.Array[arguments.Offset].ToLower();

            var arr = new ArraySegment<string>
                (arguments.Array, arguments.Offset + 1,
                 arguments.Count - 1);

            switch (arg1)
            {
                case "name":

                    character.Room.Title = BuildArgumentString(arr);
                    break;

                case "description":

                    character.Room.Description = BuildArgumentString(arr);
                    break;

                case "exit":

                    if (arguments.Count > 1)
                        ParseExit(arr, character);
                    else
                    {
                        character.Write("You must provide a direction for the exit.\r\n");
                        break;
                    }

                    break;
            }
        }

        private void ParseExit(ArraySegment<string> arguments, Character character)
        {
            var argOffset = arguments.Array[arguments.Offset];

            if (!ExitDetail.Directions.ContainsKey(argOffset))
            {
                character.Write("You must provide a valid direction for the exit.\r\n");
                return;
            }

            if (character.Room[ExitDetail.Directions[argOffset]] == null)
            {
                character.Write("That direction does not have an exit.\r\n");
                return;
            }

            if (arguments.Count < 2)
            {
                character.Write("You must provide a setting for the exit.\r\n");
                return;
            }

            var arr = new ArraySegment<string>
                (arguments.Array, arguments.Offset + 2,
                 arguments.Count - 2);

            switch (arguments.Array[arguments.Offset + 1])
            {
                case "description":

                    if (arguments.Count > 2)
                        ProcessChangeExit(character.Room, ExitDetail.Directions[argOffset],
                                          BuildArgumentString(arr), 0);
                    else
                        character.Write("Not enough parameters to change exit keyid.\r\n");

                    break;

                case "keywords":

                    if (arguments.Count > 2)
                        ProcessChangeExit(character.Room, ExitDetail.Directions[argOffset],
                                          BuildArgumentString(arr), 1);
                    else
                        character.Write("Not enough parameters to change exit keyid.\r\n");

                    break;

                case "flags":

                    if (arguments.Count > 2)
                    {
                        if (IsNumeric(arguments.Array[arguments.Offset + 2]))
                            ProcessChangeExit(character.Room, ExitDetail.Directions[argOffset],
                                              arguments.Array[arguments.Offset + 2], 2);
                        else
                            character.Write("You must provide a numeric value for that setting.\r\n");
                    }
                    else
                        character.Write("Not enough parameters to change exit keyid.\r\n");

                    break;

                case "keyid":

                    if (arguments.Count > 2)
                    {
                        if (IsNumeric(arguments.Array[arguments.Offset + 2]))
                            ProcessChangeExit(character.Room, ExitDetail.Directions[argOffset],
                                              arguments.Array[arguments.Offset + 2], 3);
                        else
                            character.Write("You must provide a numeric value for that setting.\r\n");
                    }
                    else
                        character.Write("Not enough parameters to change exit keyid.\r\n");

                    break;

                case "target":

                    if (arguments.Count > 3)
                    {
                        if (IsNumeric(arguments.Array[arguments.Offset + 2]))
                            ProcessChangeExit(character.Room, ExitDetail.Directions[argOffset],
                                              arguments.Array[arguments.Offset + 2], 4);
                        else
                            character.Write("The target room id must be a number.\r\n");
                    }
                    else
                        character.Write("Not enough parameters to change exit target.\r\n");


                    break;
            }
        }

        internal static bool IsNumeric(object objectToTest)
        {
            if (objectToTest == null)
                return false;

            double outValue;

            return double.TryParse(objectToTest.ToString().Trim(),
                                   NumberStyles.Any,
                                   CultureInfo.CurrentCulture,
                                   out outValue);
        }

        private void ProcessChangeExit(Room room, Exit direction, string argument, int index)
        {
            switch (index)
            {
                case 0:

                    room[direction].Description = argument;
                    break;

                case 1:

                    foreach (var keyword in argument.Split(' '))
                        _roomManager.CreateExitKeyword(room[direction], keyword);
                    
                    break;

                case 2:

                    room[direction].Flags = Convert.ToInt32(argument);
                    break;

                case 3:

                    room[direction].KeyID = Convert.ToInt32(argument);
                    break;

                case 4:

                    room[direction].TargetRoom = Convert.ToInt32(argument);
                    break;
            }

            //MudManagers.MudInstance.RoomManager.SaveRoom();
        }

        private static string BuildArgumentString(ArraySegment<string> arguments)
        {
            var sb = new StringBuilder();

            for (var i = arguments.Offset; i < (arguments.Offset + arguments.Count); i++)
            {
                sb.Append(arguments.Array[i]);

                if ((i + 1) < (arguments.Offset + arguments.Count))
                    sb.Append(' ');
            }

            return sb.ToString();
        }
    }
}