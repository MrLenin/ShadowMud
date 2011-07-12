using ShadowMUD.Interpreter;
using ShadowMUD.Managers;
using ShadowMUD.MudObjects;
using ShadowMUD.Server;

namespace ShadowMUD.InputHandlers
{
    internal class MainMenuStateInputHandler : IStateInputHandler
    {
        private readonly GameManager _gameManager;
        private readonly TextManager _textManager;

        public MainMenuStateInputHandler()
        {
            _gameManager = MudManagers.MudInstance.GameManager;
            _textManager = MudManagers.MudInstance.TextManager;
        }

        #region IStateInputHandler Members

        public void Handle(PlayerDescriptor descriptor, string command)
        {
            if (command.Length < 1)
            {
                descriptor.Write("\r\nPlease enter a menu choice!\r\n" + _textManager.LoadText("menu"));
                return;
            }

            switch (command[0])
            {
                case '0':

                    descriptor.Write("Goodbye.\r\n");
                    //add_llog_entry(d->character, LAST_QUIT);

                    descriptor.State = PlayerState.ConnectionClosed;

                    break;

                case '1':

                    //load_result = enter_player_game(d);
                    _gameManager.EnterGame(descriptor.Character);
                    descriptor.Write(_textManager.LoadText("welcomemsg"));

                    // TODO: Figure out why you would want this?
                    /* Clear their load room if it's not persistant. */
                    //				if (!PLR_FLAGGED(d->character, PLR_LOADROOM))
                    //					GET_LOADROOM(d->character) = NOWHERE;

                    // TODO: handles room entry triggers for mobs
                    //greet_mtrigger(d->character, -1);
                    //greet_memory_mtrigger(d->character);

                    // TODO: tell room we entered the game/room
                    //act("$n has entered the game.", TRUE, d->character, 0, 0, TO_ROOM);

                    descriptor.State = PlayerState.Playing;

                    // new character, finish initializing
                    if (descriptor.Character.IsNew)
                    {
                        _gameManager.Start(descriptor.Character);

                        descriptor.Write(MudManagers.MudInstance.TextManager.LoadText("startmsg"));
                        descriptor.Character.IsNew = false;
                    }

                    MudManagers.MudInstance.CharacterManager.SaveCharacter(descriptor.Character);

                    descriptor.Character.Room.Look(descriptor.Character, false);

                    // TODO: (LOW PRIORITY) Check MudMail
                    //if (has_mail(GET_IDNUM(d->character)))
                    //    send_to_char(d->character, "You have mail waiting.\r\n");

                    // This is from the rent system in TBA/Circle, I personally hate the idea
                    //if (load_result == 2)
                    //{
                    //    /// rented items lost
                    //    send_to_char(d->character, "\r\n\007You could not afford your rent!\r\n"
                    //        "Your possesions have been donated to the Salvation Army!\r\n");
                    //}

                    descriptor.HasPrompt = false;

                    break;

                case '2':

                    if (descriptor.Character.Description.Length > 0)
                    {
                        descriptor.Write(string.Format("Current description:\r\n{0}", descriptor.Character.Description));

                        /* Don't free this now... so that the old description gets loaded as the 
     					* current buffer in the editor.  Do setup the ABORT buffer here, however. */
                        //d->backstr = strdup(d->character->player.description);
                    }

                    descriptor.Write("Enter the new text you'd like others to see when they look at you.\r\n");

                    // TODO: make an editor
                    //send_editor_help(d);

                    //d->str = &d->character->player.description;
                    //d->max_str = PLR_DESC_LENGTH;
                    //STATE(d) = CON_PLR_DESC;

                    break;

                case '3':

                    //TODO: Use a paging function
                    descriptor.Write(MudManagers.MudInstance.TextManager.LoadText("background"));
                    //page_string(d, background, 0);
                    descriptor.State = PlayerState.ReadMotd;

                    break;

                case '4':

                    descriptor.Write("\r\nEnter your old password: ");
                    MudServer.DisableLocalEcho(descriptor);
                    descriptor.State = PlayerState.GetOldPassword;

                    break;

                case '5':

                    descriptor.Write("\r\nEnter your password for verification: ");
                    MudServer.DisableLocalEcho(descriptor);
                    descriptor.State = PlayerState.DeleteCharacterConfirm1;

                    break;

                default:

                    descriptor.Write("\r\nThat's not a menu choice!\r\n" + _textManager.LoadText("menu"));

                    break;
            }
        }

        #endregion
    }
}