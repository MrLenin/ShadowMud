using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class CharacterManager
    {
        private const string CharacterAbilitesFormat = "SELECT * FROM character_abilities WHERE id = '{0}';";
        private const string CharacterFinancesFormat = "SELECT * FROM character_finances WHERE id = '{0}';";
        private const string CharacterFormat = "SELECT * FROM characters WHERE lower(name) = '{0}';";
        private const string CharacterQuestsFormat = "SELECT * FROM character_quests WHERE id = '{0}';";
        private const string CharacterSettingsFormat = "SELECT * FROM character_settings WHERE id = '{0}';";
        private const string CharacterStateFormat = "SELECT * FROM character_state WHERE id = '{0}';";
        private const string CharacterStatisticsFormat = "SELECT * FROM character_statistics WHERE id = '{0}';";

        private readonly Dictionary<string, Character> _characterList;
        private readonly ShadowDb _database;

        public CharacterManager(IDbConnection connection)
        {
            _characterList = new Dictionary<string, Character>();
            _database = new ShadowDb(connection);
        }

        public bool LoadCharacter(string name, out Character character)
        {
            if (_characterList.ContainsKey(name))
            {
                character = null;
                return false;
            }

            var query = string.Format(CharacterFormat, name.ToLower());

            var characters = _database.ExecuteQuery<Character>(query);

            if (characters.Count() == 0)
            {
                character = null;
                return false;
            }

            var temp = characters.First();

            query = string.Format(CharacterAbilitesFormat, temp.ID);
            var abilities = _database.ExecuteQuery<CharacterAbilities>(query);

            if (abilities.Count() == 0)
            {
                character = null;
                return false;
            }

            query = string.Format(CharacterFinancesFormat, temp.ID);
            var finances = _database.ExecuteQuery<CharacterFinances>(query);

            if (finances.Count() == 0)
            {
                character = null;
                return false;
            }

            query = string.Format(CharacterQuestsFormat, temp.ID);
            var quests = _database.ExecuteQuery<CharacterQuests>(query);

            if (quests.Count() == 0)
            {
                character = null;
                return false;
            }

            query = string.Format(CharacterSettingsFormat, temp.ID);
            var settings = _database.ExecuteQuery<CharacterSettings>(query);

            if (settings.Count() == 0)
            {
                character = null;
                return false;
            }

            query = string.Format(CharacterStateFormat, temp.ID);
            var state = _database.ExecuteQuery<CharacterState>(query);

            if (state.Count() == 0)
            {
                character = null;
                return false;
            }

            query = string.Format(CharacterStatisticsFormat, temp.ID);
            var statistics = _database.ExecuteQuery<CharacterStatistics>(query);

            if (statistics.Count() == 0)
            {
                character = null;
                return false;
            }

            temp.Abilities = abilities.First();
            temp.Finances = finances.First();
            temp.Quests = quests.First();
            temp.Settings = settings.First();
            temp.State = state.First();
            temp.Statistics = statistics.First();

            _characterList.Add(name, temp); //character
            character = _characterList[name].Clone();

            return true;
        }

        public void CreateCharacter(Character character)
        {
            _database.Characters.InsertOnSubmit(character);
            _database.SubmitChanges();

            character.Abilities.ID = character.ID;
            _database.CharacterAbilities.InsertOnSubmit(character.Abilities);

            character.Finances.ID = character.ID;
            _database.CharacterFinances.InsertOnSubmit(character.Finances);

            character.Quests.ID = character.ID;
            _database.CharacterQuests.InsertOnSubmit(character.Quests);

            character.Settings.ID = character.ID;
            _database.CharacterSettings.InsertOnSubmit(character.Settings);

            character.State.ID = character.ID;
            _database.CharacterState.InsertOnSubmit(character.State);

            character.Statistics.ID = character.ID;
            _database.CharacterStatistics.InsertOnSubmit(character.Statistics);

            _database.SubmitChanges();
        }

        public void SaveCharacter(Character character)
        {
            if (!_characterList.ContainsKey(character.Name))
                return;

            var original = _characterList[character.Name];

            foreach (var member in character.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original, new[] {getter.Invoke(character, null)});
            }

            foreach (var member in character.Abilities.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.Abilities, new[] {getter.Invoke(character.Abilities, null)});
            }

            foreach (var member in character.Finances.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.Finances, new[] {getter.Invoke(character.Finances, null)});
            }

            foreach (var member in character.Quests.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.Quests, new[] {getter.Invoke(character.Quests, null)});
            }

            foreach (var member in character.Settings.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.Settings, new[] {getter.Invoke(character.Settings, null)});
            }

            foreach (var member in character.State.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.State, new[] {getter.Invoke(character.State, null)});
            }

            foreach (var member in character.Statistics.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original.Statistics, new[] {getter.Invoke(character.Statistics, null)});
            }

            _database.SubmitChanges();

            _database.TableUpdate(TableID.Characters);
        }
    }
}