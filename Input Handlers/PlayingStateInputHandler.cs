using System;
using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.Properties;

namespace ShadowMUD.InputHandlers
{
    internal class PlayingStateInputHandler : IStateInputHandler
    {
        private ZoneManager _zoneManager;

        public PlayingStateInputHandler()
        {
            _zoneManager = MudManagers.MudInstance.ZoneManager;
        }

        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            Console.WriteLine(Resources.PlayingStateReachedString);
        }

        #endregion
    }
}