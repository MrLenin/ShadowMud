using System;
using System.Collections.Generic;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.CommandHandlers
{
    internal class LoadHandler : ICommandHandler
    {
        private readonly string _commandString;

        public LoadHandler()
        {
            _commandString = "load";
        }

        #region ICommandHandler Members

        public void Handle(Character character, string[] arguments)
        {
            if (arguments.Length < 2)
                return;

            switch (arguments[0])
            {
                case "object":

                    var id = Convert.ToInt32(arguments[1]);
                    character.Room.AddObject(MudManagers.MudInstance.ObjectManager[id]);

                    break;
            }
        }

        public string Command
        {
            get { return _commandString; }
        }

        #endregion
    }
}