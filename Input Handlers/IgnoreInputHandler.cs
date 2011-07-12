using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.InputHandlers
{
    internal class IgnoreInputHandler : IStateInputHandler
    {
        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            switch (descriptor.State)
            {
                case PlayerState.ReadMotd:
                    descriptor.Write(MudManagers.MudInstance.TextManager.LoadText("menu"));
                    descriptor.State = PlayerState.MainMenu;

                    break;
            }

            return;
        }

        #endregion
    }
}