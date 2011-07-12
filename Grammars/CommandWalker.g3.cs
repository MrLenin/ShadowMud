using System;
using System.Collections.Generic;
using System.Text;
using Antlr.Runtime;
using Antlr.Runtime.Tree;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Grammars
{

    partial class CommandWalker
    {
        private readonly RoomManager _roomManager;
        private readonly ObjectManager _objectManager;

        private readonly Character _character;

        private static readonly Dictionary<string, Exit> Directions = new Dictionary<string, Exit>(10)
        {
            {"north", Exit.North},
            {"northeast", Exit.NorthEast},
            {"east", Exit.East},
            {"southeast", Exit.SouthEast},
            {"south", Exit.South},
            {"southwest", Exit.SouthWest},
            {"west", Exit.West},
            {"northwest", Exit.NorthWest},
            {"up", Exit.Up},
            {"down", Exit.Down}
        };

        public CommandWalker(ITreeNodeStream input, Character character)
            : this(input)
        {
            _character = character;
            _roomManager = MudManagers.MudInstance.RoomManager;
            _objectManager = MudManagers.MudInstance.ObjectManager;
        }

        #region Set Handlers
        
        public void SetRoom(string command, string args)
        {
            if (args == null)
            {
                _character.Write("You must provide a new value for this attribute.\r\n");
                return;
            }

            switch (command)
            {
                case "name":

                    _character.Room.Title = args;
                    _character.Write("Done.\r\n");

                    break;

                case "description":

                    _character.Room.Description = args;
                    _character.Write("Done.\r\n");

                    break;

                case "badattribute":

                    _character.Write("You must enter a valid attribute to change.\r\n");
                    break;
            }
        }

        public void SetExit(string direction, string command, string args)
        {

        }

        public void SetUnknown()
        {
            _character.Write("What exactly are you trying to set?\r\n");
        }

        #endregion

        #region Move Handler(s)

        public void Move(string direction)
        {
            if (direction.Length == 0)
            {
                _character.Write("Which way do you want to move?\r\n");
                return;
            }

            if (_character.Room.ContainsExit(Directions[direction]))
            {
                var exitInfo = _character.Room[Directions[direction]];

                _roomManager.AddCharacterTo(exitInfo.TargetRoom, _character);
                _roomManager.RemoveCharacterFrom(_character.Room.ID, _character);

                _character.ZoneID = exitInfo.ZoneID;
                _character.RoomID = exitInfo.TargetRoom;

                _character.Room.Look(_character, false);
            }
            else
            {
                _character.Write("Alas, you cannot go that way...\r\n");
                return;
            }
        } 

        #endregion

        #region Look Handler(s)

        public void Look(string direction)
        {
            if (direction.Length != 0)
            {
                if (Directions.ContainsKey(direction))
                {
                    _character.Write("You look " + direction + " and what you can see is:\r\n");

                    var desc = _character.Room[Directions[direction]].Description;

                    _character.Write("\t" + desc + "\r\n");
                }
            }
            else
            {
                _character.Room.Look(_character, false);
            }
        }

        #endregion
    }
}
