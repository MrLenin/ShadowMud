using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.characters")]
    public partial class Character : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged handling

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (_isCopy)
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

        #region string Description

        private string _description;

        [DebuggerNonUserCode]
        [Column(Storage = "_description", Name = "description", DbType = "varchar(MAX)")]
        public string Description { get { return _description; } set { SetField(ref _description, value, "Description"); } }

        #endregion

        #region int Gender

        private int _gender;

        [DebuggerNonUserCode]
        [Column(Storage = "_gender", Name = "gender", DbType = "integer(32,0)", CanBeNull = false)]
        public int Gender { get { return _gender; } set { SetField(ref _gender, value, "Gender"); } }

        #endregion

        #region int Height

        private int _height;

        [DebuggerNonUserCode]
        [Column(Storage = "_height", Name = "height", DbType = "integer(32,0)", CanBeNull = false)]
        public int Height { get { return _height; } set { SetField(ref _height, value, "Height"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsDbGenerated = true, CanBeNull = false, IsPrimaryKey = true)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region string Name

        private string _name;

        [DebuggerNonUserCode]
        [Column(Storage = "_name", Name = "name", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Name { get { return _name; } set { SetField(ref _name, value, "Name"); } }

        #endregion

        #region string Password

        private string _password;

        [DebuggerNonUserCode]
        [Column(Storage = "_password", Name = "password", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Password { get { return _password; } set { SetField(ref _password, value, "Password"); } }

        #endregion

        #region int? RoomID

        private int? _roomID;

        [DebuggerNonUserCode]
        [Column(Storage = "_roomID", Name = "roomid", DbType = "integer(32,0)")]
        public int? RoomID { get { return _roomID; } set { SetField(ref _roomID, value, "RoomID"); } }

        #endregion

        #region string Title

        private string _title;

        [DebuggerNonUserCode]
        [Column(Storage = "_title", Name = "title", DbType = "varchar(MAX)")]
        public string Title { get { return _title; } set { SetField(ref _title, value, "Title"); } }

        #endregion

        #region int Weight

        private int _weight;

        [DebuggerNonUserCode]
        [Column(Storage = "_weight", Name = "weight", DbType = "integer(32,0)", CanBeNull = false)]
        public int Weight { get { return _weight; } set { SetField(ref _weight, value, "Weight"); } }

        #endregion

        #region int? ZoneID

        private int? _zoneID;

        [DebuggerNonUserCode]
        [Column(Storage = "_zoneID", Name = "zoneid", DbType = "integer(32,0)")]
        public int? ZoneID { get { return _zoneID; } set { SetField(ref _zoneID, value, "ZoneID"); } }

        #endregion

        #region Children

        [Association(Storage = null, OtherKey = "ID", Name = "character_abilities_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterAbilities> CharacterAbilities { get; set; }

        [Association(Storage = null, OtherKey = "ID", Name = "character_statistics_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterStatistics> CharacterStatistics { get; set; }

        [Association(Storage = null, OtherKey = "ID", Name = "character_state_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterState> CharacterState { get; set; }

        [Association(Storage = null, OtherKey = "ID", Name = "character_settings_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterSettings> CharacterSettings { get; set; }

        [Association(Storage = null, OtherKey = "ID", Name = "character_quests_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterQuests> CharacterQuests { get; set; }

        [Association(Storage = null, OtherKey = "ID", Name = "character_finances_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterFinances> CharacterFinances { get; set; }

        #endregion
    }
}