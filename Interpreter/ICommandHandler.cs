using ShadowMUD.MudObjects;

namespace ShadowMUD.Interpreter
{
    internal interface IStateInputHandler
    {
        void Handle(PlayerDescriptor descriptor, string command);
    }

    public interface ICommandHandler
    {
        string Command { get; }
        void Handle(Character character, string[] arguments);
    }
}