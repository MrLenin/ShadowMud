using System;
using System.Collections.Generic;
using System.Reflection;
using ShadowMUD.Managers;
using ShadowMUD.Random;

namespace ShadowMUD.MudObjects
{
    public enum Genders : short
    {
        None = 0,
        Male,
        Female
    }

    public partial class Character : ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public CharacterAbilities Abilities;
        public PlayerDescriptor Descriptor;
        public CharacterFinances Finances;
        public uint IdleTime;
// ReSharper disable UnaccessedField.Local
        private bool _isCopy;
// ReSharper restore UnaccessedField.Local

        public bool IsNew;
        public CharacterQuests Quests;
        public CharacterSettings Settings;
        public CharacterState State;
        public CharacterStatistics Statistics;

        public uint WaitState;

        /* Need to add db table for these and associated entity mapping file
         * player_data_special_saved
        //int pref[PR_ARRAY_MAX];               //**< preference flags
        //struct txt_block *comm_hist[NUM_HIST];  //**< Communication history
        //qst_vnum *completed_quests;             //**< Quests completed              
        int    num_completed_quests;            //**< Number of completed quests
        //int    current_quest;                   //**< vnum of current quest         
        //struct alias_data *aliases;             //**< Command aliases			
        
        * char_special_data_saved IS SAVED
        int act[PM_ARRAY_MAX];                  //**< act flags for NPC's; player flag for PC's
        int affected_by[AF_ARRAY_MAX];          //**< Bitvector for spells/skills affected by
        sh_int apply_saving_throw[5];           //**< Saving throw (Bonuses)

        * player_special_data NOT SAVED
        long last_tell;                         //**< idnum of PC who last told this PC, used to reply 
        void *last_olc_targ;                    //**< ? Currently Unused ? 
        int last_olc_mode;                      //**< ? Currently Unused ? 
        char *host;                             //**< Resolved hostname, or ip, for player. 
        
        * char_special_data NOT SAVED
        struct char_data *fighting;             //**< Target of fight; else NULL
        struct char_data *hunting;              //**< Target of NPC hunt; else NULL
        struct obj_data *furniture;             //**< Object being sat on/in; else NULL
        struct char_data *next_in_furniture;    //**< Next person sitting, else NULL

        byte position;                          //**< Standing, fighting, sleeping, etc.

        int carry_weight;                       //**< Carried weight
        byte carry_items;                       //**< Number of items carried
        int timer;                              //**< Timer for update
        */

        public Character()
        {
            _modifiedList = new Dictionary<string, PropertyInfo>();

            Descriptor = null;
            WaitState = 0;
            IdleTime = 0;
            IsNew = false;

            ZoneID = 0;
            RoomID = 0;
        }

        public Room Room
        {
            get
            {
                if (ZoneID != null && RoomID != null)
                    return MudManagers.MudInstance.RoomManager[ZoneID.Value, RoomID.Value];
                return null;
            }
        }

        public IEnumerable<PropertyInfo> Modified
        {
            get { return _modifiedList.Values; }
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        public void Initialize()
        {
            SetTitle(null);

            Description = string.Empty;
            RoomID = 1;
            ZoneID = 1;

            Quests.QuestsCompleted = 0;

            // TODO: plr birth, logon, played times

            Statistics.ArmourClass = 100;

            var rng = new MersenneTwister();

            switch ((Genders) Gender)
            {
                case Genders.Male:

                    Height = rng.Next(155, 200);
                    Weight = rng.Next(62, 110);

                    break;

                case Genders.Female:

                    Height = rng.Next(143, 185);
                    Weight = rng.Next(45, 75);

                    break;

                case Genders.None:

                    Height = rng.Next(143, 200);
                    Weight = rng.Next(45, 110);

                    break;
            }

            Abilities.Intelligence = 25;
            Abilities.Wisdom = 25;
            Abilities.Dexterity = 25;
            Abilities.Strength = 25;
            Abilities.Constitution = 25;
            Abilities.Charisma = 25;

            Settings.EnterPoofMessage = string.Empty;
            Settings.ExitPoofMessage = string.Empty;
            Settings.ScreenWidth = 80;
            Settings.ScreenHeight = 22;
        }

        public void ResetCharacter()
        {
            WaitState = 0;
            IdleTime = 0;
        }

        public void Write(string text)
        {
            Descriptor.Write(text);
        }

        public void SetTitle(string title)
        {
            if (title == null)
            {
                if (Gender == (short) Genders.Female)
                    TitleFemale();
                else
                    TitleMale();
            }
            else
            {
                Title = " " + title + " ";
            }
        }

        public string TitleFemale()
        {
            return " ";
        }

        public string TitleMale()
        {
            return " ";
        }

        internal void ClearModified()
        {
            _modifiedList.Clear();
        }

        public Character Clone()
        {
            var character = MemberwiseClone() as Character;

            if (character == null)
                return null;

            character._isCopy = true;
            character.Abilities = character.Abilities.Clone();
            character.Abilities.IsCopy = true;
            character.Finances = character.Finances.Clone();
            character.Finances.IsCopy = true;
            character.Quests = character.Quests.Clone();
            character.Quests.IsCopy = true;
            character.Settings = character.Settings.Clone();
            character.Settings.IsCopy = true;
            character.State = character.State.Clone();
            character.State.IsCopy = true;
            character.Statistics = character.Statistics.Clone();
            character.Statistics.IsCopy = true;

            return character;
        }
    }
}