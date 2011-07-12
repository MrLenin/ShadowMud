using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_statistics")]
    public class CharacterStatistics : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterStatistics()
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

        #region short ArmourClass

        private short _armourClass;

        [DebuggerNonUserCode]
        [Column(Storage = "_armourClass", Name = "armourclass", DbType = "smallint(16,0)", CanBeNull = false)]
        public short ArmourClass { get { return _armourClass; } set { SetField(ref _armourClass, value, "ArmourClass"); } }

        #endregion

        #region short DamageRoll

        private short _damageRoll;

        [DebuggerNonUserCode]
        [Column(Storage = "_damageRoll", Name = "damageroll", DbType = "smallint(16,0)", CanBeNull = false)]
        public short DamageRoll { get { return _damageRoll; } set { SetField(ref _damageRoll, value, "DamageRoll"); } }

        #endregion

        #region short HitRoll

        private short _hitRoll;

        [DebuggerNonUserCode]
        [Column(Storage = "_hitRoll", Name = "hitroll", DbType = "smallint(16,0)", CanBeNull = false)]
        public short HitRoll { get { return _hitRoll; } set { SetField(ref _hitRoll, value, "HitRoll"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region short MaxHitPoints

        private short _maxHitPoints;

        [DebuggerNonUserCode]
        [Column(Storage = "_maxHitPoints", Name = "maxhitpoints", DbType = "smallint(16,0)", CanBeNull = false)]
        public short MaxHitPoints { get { return _maxHitPoints; } set { SetField(ref _maxHitPoints, value, "MaxHitPoints"); } }

        #endregion

        #region short MaxMana

        private short _maxMana;

        [DebuggerNonUserCode]
        [Column(Storage = "_maxMana", Name = "maxmana", DbType = "smallint(16,0)", CanBeNull = false)]
        public short MaxMana { get { return _maxMana; } set { SetField(ref _maxMana, value, "MaxMana"); } }

        #endregion

        #region short MaxMovement

        private short _maxMovement;

        [DebuggerNonUserCode]
        [Column(Storage = "_maxMovement", Name = "maxmovement", DbType = "smallint(16,0)", CanBeNull = false)]
        public short MaxMovement { get { return _maxMovement; } set { SetField(ref _maxMovement, value, "MaxMovement"); } }

        #endregion

        #region short PasswordAttempts

        private short _passwordAttempts;

        [DebuggerNonUserCode]
        [Column(Storage = "_passwordAttempts", Name = "passwordattempts", DbType = "smallint(16,0)", CanBeNull = false)]
        public short PasswordAttempts { get { return _passwordAttempts; } set { SetField(ref _passwordAttempts, value, "PasswordAttempts"); } }

        #endregion

        #region short PracticePoints

        private short _practicePoints;

        [DebuggerNonUserCode]
        [Column(Storage = "_practicePoints", Name = "practicepoints", DbType = "smallint(16,0)", CanBeNull = false)]
        public short PracticePoints { get { return _practicePoints; } set { SetField(ref _practicePoints, value, "PracticePoints"); } }

        #endregion

        #region short TrainingPoints

        private short _trainingPoints;

        [DebuggerNonUserCode]
        [Column(Storage = "_trainingPoints", Name = "trainingpoints", DbType = "smallint(16,0)", CanBeNull = false)]
        public short TrainingPoints { get { return _trainingPoints; } set { SetField(ref _trainingPoints, value, "TrainingPoints"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_statistics_id_fkey", IsForeignKey = true
            )]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public CharacterStatistics Clone()
        {
            var character = MemberwiseClone() as CharacterStatistics;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}