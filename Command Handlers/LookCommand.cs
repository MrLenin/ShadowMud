using System.Collections.Generic;
using ShadowMUD.Interpreter;
using ShadowMUD.MudObjects;

namespace ShadowMUD.CommandHandlers
{
    internal class LookHandler : ICommandHandler
    {
        private readonly string _commandString;

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

        public LookHandler()
        {
            _commandString = "look";
        }

        #region ICommandHandler Members

        public void Handle(Character character, string[] arguments)
        {
            if (arguments.Length > 0)
            {
                if (Directions.ContainsKey(arguments[0]))
                {
                    character.Write("You look " + arguments[0] + " and what you can see is:\r\n");

                    var desc = character.Room[Directions[arguments[0]]].Description;

                    character.Write("\t" + desc + "\r\n");
                }
            }
            else
            {
                character.Room.Look(character, false);
            }
        }

        public string Command { get { return _commandString; } }

        #endregion
    }
}