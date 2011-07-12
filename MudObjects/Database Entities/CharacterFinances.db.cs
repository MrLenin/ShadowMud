using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_finances")]
    public class CharacterFinances : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterFinances()
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

        #region int BankedCreds

        private int _bankedCreds;

        [DebuggerNonUserCode]
        [Column(Storage = "_bankedCreds", Name = "bankedcreds", DbType = "integer(32,0)", CanBeNull = false)]
        public int BankedCreds { get { return _bankedCreds; } set { SetField(ref _bankedCreds, value, "BankedCreds"); } }

        #endregion

        #region int BankedDollars

        private int _bankedDollars;

        [DebuggerNonUserCode]
        [Column(Storage = "_bankedDollars", Name = "bankeddollars", DbType = "integer(32,0)", CanBeNull = false)]
        public int BankedDollars { get { return _bankedDollars; } set { SetField(ref _bankedDollars, value, "BankedDollars"); } }

        #endregion

        #region int BankedEuro

        private int _bankedEuro;

        [DebuggerNonUserCode]
        [Column(Storage = "_bankedEuro", Name = "bankedeuro", DbType = "integer(32,0)", CanBeNull = false)]
        public int BankedEuro { get { return _bankedEuro; } set { SetField(ref _bankedEuro, value, "BankedEuro"); } }

        #endregion

        #region int BankedYen

        private int _bankedYen;

        [DebuggerNonUserCode]
        [Column(Storage = "_bankedYen", Name = "bankedyen", DbType = "integer(32,0)", CanBeNull = false)]
        public int BankedYen { get { return _bankedYen; } set { SetField(ref _bankedYen, value, "BankedYen"); } }

        #endregion

        #region int Creds

        private int _creds;

        [DebuggerNonUserCode]
        [Column(Storage = "_creds", Name = "creds", DbType = "integer(32,0)", CanBeNull = false)]
        public int Creds { get { return _creds; } set { SetField(ref _creds, value, "Creds"); } }

        #endregion

        #region int Dollars

        private int _dollars;

        [DebuggerNonUserCode]
        [Column(Storage = "_dollars", Name = "dollars", DbType = "integer(32,0)", CanBeNull = false)]
        public int Dollars { get { return _dollars; } set { SetField(ref _dollars, value, "Dollars"); } }

        #endregion

        #region int Euro

        private int _euro;

        [DebuggerNonUserCode]
        [Column(Storage = "_euro", Name = "euro", DbType = "integer(32,0)", CanBeNull = false)]
        public int Euro { get { return _euro; } set { SetField(ref _euro, value, "Euro"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int Yen

        private int _yen;

        [DebuggerNonUserCode]
        [Column(Storage = "_yen", Name = "yen", DbType = "integer(32,0)", CanBeNull = false)]
        public int Yen { get { return _yen; } set { SetField(ref _yen, value, "Yen"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_finances_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } } 

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        public CharacterFinances Clone()
        {
            var character = MemberwiseClone() as CharacterFinances;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}