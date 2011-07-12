using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.rooms")]
    public partial class Room : INotifyPropertyChanged
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
        [Column(Storage = "_description", Name = "description", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Description { get { return _description; } set { SetField(ref _description, value, "Description"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region string Title

        private string _title;

        [DebuggerNonUserCode]
        [Column(Storage = "_title", Name = "title", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Title { get { return _title; } set { SetField(ref _title, value, "Title"); } }

        #endregion

        #region Children

        [Association(Storage = null, OtherKey = "RoomID", Name = "rooms_zone_details_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ZoneDetail> ZoneDetails { get; set; }

        [Association(Storage = null, OtherKey = "RoomID", Name = "rooms_exit_details_roomid_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ExitDetail> RoomExitDetails { get; set; }

        [Association(Storage = null, OtherKey = "TargetRoom", Name = "rooms_exit_details_targetid_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ExitDetail> TargetExitDetails { get; set; }

        #endregion
    }
}