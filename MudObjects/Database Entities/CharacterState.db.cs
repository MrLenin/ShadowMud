using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_state")]
    public class CharacterState : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterState()
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

        #region short Alignment

        private short _alignment;

        [DebuggerNonUserCode]
        [Column(Storage = "_alignment", Name = "alignment", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Alignment { get { return _alignment; } set { SetField(ref _alignment, value, "Alignment"); } }

        #endregion

        #region short DrunkLevel

        private short _drunkLevel;

        [DebuggerNonUserCode]
        [Column(Storage = "_drunkLevel", Name = "drunklevel", DbType = "smallint(16,0)", CanBeNull = false)]
        public short DrunkLevel { get { return _drunkLevel; } set { SetField(ref _drunkLevel, value, "DrunkLevel"); } }

        #endregion

        #region short FreezeLevel

        private short _freezeLevel;

        [DebuggerNonUserCode]
        [Column(Storage = "_freezeLevel", Name = "freezelevel", DbType = "smallint(16,0)", CanBeNull = false)]
        public short FreezeLevel { get { return _freezeLevel; } set { SetField(ref _freezeLevel, value, "FreezeLevel"); } }

        #endregion

        #region short HitPoints

        private short _hitPoints;

        [DebuggerNonUserCode]
        [Column(Storage = "_hitPoints", Name = "hitpoints", DbType = "smallint(16,0)", CanBeNull = false)]
        public short HitPoints { get { return _hitPoints; } set { SetField(ref _hitPoints, value, "HitPoints"); } }

        #endregion

        #region short HungerLevel

        private short _hungerLevel;

        [DebuggerNonUserCode]
        [Column(Storage = "_hungerLevel", Name = "hungerlevel", DbType = "smallint(16,0)", CanBeNull = false)]
        public short HungerLevel { get { return _hungerLevel; } set { SetField(ref _hungerLevel, value, "HungerLevel"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region short InvisLevel

        private short _invisLevel;

        [DebuggerNonUserCode]
        [Column(Storage = "_invisLevel", Name = "invislevel", DbType = "smallint(16,0)", CanBeNull = false)]
        public short InvisLevel { get { return _invisLevel; } set { SetField(ref _invisLevel, value, "InvisLevel"); } }

        #endregion

        #region short Mana

        private short _mana;

        [DebuggerNonUserCode]
        [Column(Storage = "_mana", Name = "mana", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Mana { get { return _mana; } set { SetField(ref _mana, value, "Mana"); } }

        #endregion

        #region short Movement

        private short _movement;

        [DebuggerNonUserCode]
        [Column(Storage = "_movement", Name = "movement", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Movement { get { return _movement; } set { SetField(ref _movement, value, "Movement"); } }

        #endregion

        #region short ThirstLevel

        private short _thirstLevel;

        [DebuggerNonUserCode]
        [Column(Storage = "_thirstLevel", Name = "thirstlevel", DbType = "smallint(16,0)", CanBeNull = false)]
        public short ThirstLevel { get { return _thirstLevel; } set { SetField(ref _thirstLevel, value, "ThirstLevel"); } }

        #endregion

        #region short Wimpy

        private short _wimpy;

        [DebuggerNonUserCode]
        [Column(Storage = "_wimpy", Name = "wimpy", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Wimpy { get { return _wimpy; } set { SetField(ref _wimpy, value, "Wimpy"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_state_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public CharacterState Clone()
        {
            var character = MemberwiseClone() as CharacterState;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}