using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_settings")]
    public class CharacterSettings : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public CharacterSettings()
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

        #region string EnterPoofMessage

        private string _enterPoofMessage;

        [DebuggerNonUserCode]
        [Column(Storage = "_enterPoofMessage", Name = "enterpoofmessage", DbType = "text")]
        public string EnterPoofMessage { get { return _enterPoofMessage; } set { SetField(ref _enterPoofMessage, value, "EnterPoofMessage"); } }

        #endregion

        #region string ExitPoofMessage

        private string _exitPoofMessage;

        [DebuggerNonUserCode]
        [Column(Storage = "_exitPoofMessage", Name = "exitpoofmessage", DbType = "text")]
        public string ExitPoofMessage { get { return _exitPoofMessage; } set { SetField(ref _exitPoofMessage, value, "ExitPoofMessage"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region short ScreenHeight

        private short _screenHeight;

        [DebuggerNonUserCode]
        [Column(Storage = "_screenHeight", Name = "screenheight", DbType = "smallint(16,0)", CanBeNull = false)]
        public short ScreenHeight { get { return _screenHeight; } set { SetField(ref _screenHeight, value, "ScreenHeight"); } }

        #endregion

        #region short ScreenWidth

        private short _screenWidth;

        [DebuggerNonUserCode]
        [Column(Storage = "_screenWidth", Name = "screenwidth", DbType = "smallint(16,0)", CanBeNull = false)]
        public short ScreenWidth { get { return _screenWidth; } set { SetField(ref _screenWidth, value, "ScreenWidth"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        [Association(Storage = "_characters", ThisKey = "ID", Name = "character_settings_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Character { get { return _characters.Entity; } set { _characters.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public CharacterSettings Clone()
        {
            var character = MemberwiseClone() as CharacterSettings;

            if (character != null)
                character.IsCopy = true;

            return character;
        }
    }
}