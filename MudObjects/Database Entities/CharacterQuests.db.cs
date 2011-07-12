using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_quests")]
    public class CharacterQuests : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterQuests()
        {
            _modifiedList = new Dictionary<string, PropertyInfo>();
        }

        #region INotifyPropertyChanged handling

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (IsCopy)
            {
                if (!_modifiedList.ContainsKey(propertyName))
                {
                    var propertyInfo = GetType().GetProperty(propertyName);
                    _modifiedList.Add(propertyName, propertyInfo);
                }

                return;
            }

            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;

            field = value;

            OnPropertyChanged(propertyName);

            return true;
        }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region short QuestPoints

        private short _questPoints;

        [DebuggerNonUserCode]
        [Column(Storage = "_questPoints", Name = "questpoints", DbType = "smallint(16,0)", CanBeNull = false)]
        public short QuestPoints { get { return _questPoints; } set { SetField(ref _questPoints, value, "QuestPoints"); } }

        #endregion

        #region short QuestsCompleted

        private short _questsCompleted;

        [DebuggerNonUserCode]
        [Column(Storage = "_questsCompleted", Name = "questscompleted", DbType = "smallint(16,0)", CanBeNull = false)]
        public short QuestsCompleted { get { return _questsCompleted; } set { SetField(ref _questsCompleted, value, "QuestsCompleted"); } }

        #endregion

        #region int TimeRemaining

        private int _timeRemaining;

        [DebuggerNonUserCode]
        [Column(Storage = "_timeRemaining", Name = "timeremaining", DbType = "integer(32,0)", CanBeNull = false)]
        public int TimeRemaining { get { return _timeRemaining; } set { SetField(ref _timeRemaining, value, "TimeRemaining"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_quests_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public CharacterQuests Clone()
        {
            var character = MemberwiseClone() as CharacterQuests;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}