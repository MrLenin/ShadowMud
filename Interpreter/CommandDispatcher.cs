using System.Collections.Generic;
using ShadowMUD.CommandHandlers;
using ShadowMUD.InputHandlers;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Interpreter
{
    public class CommandDispatcher
    {
        private readonly Dictionary<PlayerState, IStateInputHandler> _stateHandlers;

        // State based Input Handlers
        private ChooseSexStateInputHandler _chooseSexHandler;
        private ConfirmNameStateInputHandler _confirmNameHandler;
        private ConfirmPasswordStateInputHandler _confirmPasswordHandler;
        private CreatePasswordStateInputHandler _createPasswordHandler;
        private EnterPasswordStateInputHandler _enterPasswordHandler;
        private GetNameStateInputHandler _getNameHandler;
        private IgnoreInputHandler _ignoreInputHandler;
        private MainMenuStateInputHandler _mainMenuHandler;
        private PlayingStateInputHandler _playingHandler;

        public CommandDispatcher()
        {
            _stateHandlers = new Dictionary<PlayerState, IStateInputHandler>();
        }

        public void InitializeStateHandlers()
        {
            // init handlers
            _getNameHandler = new GetNameStateInputHandler();
            _enterPasswordHandler = new EnterPasswordStateInputHandler();
            _confirmNameHandler = new ConfirmNameStateInputHandler();
            _createPasswordHandler = new CreatePasswordStateInputHandler();
            _confirmPasswordHandler = new ConfirmPasswordStateInputHandler();
            _chooseSexHandler = new ChooseSexStateInputHandler();
            _playingHandler = new PlayingStateInputHandler();
            _ignoreInputHandler = new IgnoreInputHandler();
            _mainMenuHandler = new MainMenuStateInputHandler();
        }

        public void RegisterStateHandlers()
        {
            // input handling
            AddHandler(PlayerState.GetName, _getNameHandler);
            AddHandler(PlayerState.EnterPassword, _enterPasswordHandler);
            AddHandler(PlayerState.ConfirmName, _confirmNameHandler);
            AddHandler(PlayerState.CreatePassword, _createPasswordHandler);
            AddHandler(PlayerState.ConfirmPassword, _confirmPasswordHandler);
            AddHandler(PlayerState.ChooseSex, _chooseSexHandler);
            AddHandler(PlayerState.Playing, _playingHandler);
            AddHandler(PlayerState.MainMenu, _mainMenuHandler);

            // ignore input
            AddHandler(PlayerState.ReadMotd, _ignoreInputHandler);
        }

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            if (_stateHandlers.ContainsKey(descriptor.State))
                _stateHandlers[descriptor.State].Handle(descriptor, command);
        }

        private void AddHandler(PlayerState state, IStateInputHandler handler)
        {
            _stateHandlers.Add(state, handler);
        }
    }
}