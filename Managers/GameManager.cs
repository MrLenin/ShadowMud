using System.Collections.Generic;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class GameManager
    {
        private readonly Dictionary<int, Character> _characterCollection;

        private RoomManager _roomManager;

        public GameManager()
        {
            _characterCollection = new Dictionary<int, Character>(20);
        }

        public void EnterGame(Character character)
        {
            _roomManager = MudManagers.MudInstance.RoomManager;

            if (character.RoomID != null)
                if (!_roomManager.AddCharacterTo(character.RoomID.Value, character))
                {
                    character.ZoneID = 1;
                    character.RoomID = 1;

                    _roomManager.AddCharacterTo(character.RoomID.Value, character);
                }

            // TODO: Load characters inventory

            _characterCollection.Add(character.ID, character);
        }

        /* Some initializations for characters, including initial skills */

        public void Start(Character character)
        {
            //GET_LEVEL(ch) = 1;
            //GET_EXP(ch) = 1;

            character.SetTitle(null);
            //roll_real_abils(ch);

            character.Statistics.MaxHitPoints = 10;
            character.Statistics.MaxMana = 100;
            character.Statistics.MaxMovement = 82;

            //case CLASS_THIEF:
            //SET_SKILL(ch, SKILL_SNEAK, 10);
            //SET_SKILL(ch, SKILL_HIDE, 5);
            //SET_SKILL(ch, SKILL_STEAL, 15);
            //SET_SKILL(ch, SKILL_BACKSTAB, 10);
            //SET_SKILL(ch, SKILL_PICK_LOCK, 10);
            //SET_SKILL(ch, SKILL_TRACK, 10);
            //break;

            //advance_level(ch);

            character.State.HitPoints = character.Statistics.MaxHitPoints;
            character.State.Mana = character.Statistics.MaxMana;
            character.State.Movement = character.Statistics.MaxMovement;

            character.State.ThirstLevel = 20;
            character.State.HungerLevel = 15;
            character.State.DrunkLevel = 0;

            //SET_BIT_AR(PRF_FLAGS(ch), PRF_AUTOEXIT);  

            //if (CONFIG_SITEOK_ALL)
            //SET_BIT_AR(PLR_FLAGS(ch), PLR_SITEOK);
        }
    }
}