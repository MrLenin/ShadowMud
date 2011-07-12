using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_abilities")]
    public class CharacterAbilities : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterAbilities()
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

        #region short Charisma

        private short _charisma;

        [DebuggerNonUserCode]
        [Column(Storage = "_charisma", Name = "charisma", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Charisma { get { return _charisma; } set { SetField(ref _charisma, value, "Charisma"); } }

        #endregion

        #region short Constitution

        private short _constitution;

        [DebuggerNonUserCode]
        [Column(Storage = "_constitution", Name = "constitution", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Constitution { get { return _constitution; } set { SetField(ref _constitution, value, "Constitution"); } }

        #endregion

        #region short Dexterity

        private short _dexterity;

        [DebuggerNonUserCode]
        [Column(Storage = "_dexterity", Name = "dexterity", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Dexterity { get { return _dexterity; } set { SetField(ref _dexterity, value, "Dexterity"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region short Intelligence

        private short _intelligence;

        [DebuggerNonUserCode]
        [Column(Storage = "_intelligence", Name = "intelligence", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Intelligence { get { return _intelligence; } set { SetField(ref _intelligence, value, "Intelligence"); } }

        #endregion

        #region short Strength

        private short _strength;

        [DebuggerNonUserCode]
        [Column(Storage = "_strength", Name = "strength", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Strength { get { return _strength; } set { SetField(ref _strength, value, "Strength"); } }

        #endregion

        #region short Wisdom

        private short _wisdom;

        [DebuggerNonUserCode]
        [Column(Storage = "_wisdom", Name = "wisdom", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Wisdom { get { return _wisdom; } set { SetField(ref _wisdom, value, "Wisdom"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_abilities_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } } 

        #endregion

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

        public CharacterAbilities Clone()
        {
            var character = MemberwiseClone() as CharacterAbilities;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}