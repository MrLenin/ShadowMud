using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.room_exit_details")]
    public partial class ExitDetail : INotifyPropertyChanged, ICloneable
    {
        private readonly List<ExitKeyword> _exitKeywords;
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public ExitDetail()
        {
            _exitKeywords = new List<MudObjects.ExitKeyword>();
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

        private bool SetField<T>(ref T field, T value, string propertyName)
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
        [Column(Storage = "_description", Name = "description", DbType = "text", CanBeNull = false)]
        public string Description { get { return _description; } set { SetField(ref _description, value, "Description"); } }

        #endregion

        #region short ExitDirection

        private short _exitDirection;

        [DebuggerNonUserCode]
        [Column(Storage = "_exitDirection", Name = "exitdirection", DbType = "smallint(16,0)", CanBeNull = false)]
        public short ExitDirection { get { return _exitDirection; } set { SetField(ref _exitDirection, value, "ExitDirection"); } }

        #endregion

        #region int Flags

        private int _flags;

        [DebuggerNonUserCode]
        [Column(Storage = "_flags", Name = "flags", DbType = "integer(32,0)", CanBeNull = false)]
        public int Flags { get { return _flags; } set { SetField(ref _flags, value, "Flags"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int KeyID

        private int _keyID;

        [DebuggerNonUserCode]
        [Column(Storage = "_keyID", Name = "keyid", DbType = "integer(32,0)", CanBeNull = false)]
        public int KeyID { get { return _keyID; } set { SetField(ref _keyID, value, "KeyID"); } }

        #endregion

        #region int RoomID

        private int _roomID;

        [DebuggerNonUserCode]
        [Column(Storage = "_roomID", Name = "roomid", DbType = "integer(32,0)", CanBeNull = false)]
        public int RoomID { get { return _roomID; } set { SetField(ref _roomID, value, "RoomID"); } }

        #endregion

        #region int TargetRoom

        private int _targetRoom;

        [DebuggerNonUserCode]
        [Column(Storage = "_targetRoom", Name = "targetroom", DbType = "integer(32,0)", CanBeNull = false)]
        public int TargetRoom { get { return _targetRoom; } set { SetField(ref _targetRoom, value, "TargetRoom"); } }

        #endregion

        #region Parents

        private EntityRef<Room> _rooms;
        private EntityRef<Room> _targetRooms;

        [Association(Storage = "_rooms", ThisKey = "RoomID", Name = "rooms_exit_details_roomid_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Room Source { get { return _rooms.Entity; } set { _rooms.Entity = value; } }

        [Association(Storage = "_targetRooms", ThisKey = "TargetRoom", Name = "rooms_exit_details_targetid_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Room Target { get { return _targetRooms.Entity; } set { _targetRooms.Entity = value; } }

        #endregion

        #region Children

        [Association(Storage = null, OtherKey = "ExitDetailID", Name = "exit_details_keywords_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ExitKeyword> ExitKeyword { get; set; }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion

        public ExitDetail Clone()
        {
            var exitDetail = MemberwiseClone() as ExitDetail;

            if (exitDetail != null)
                exitDetail.IsCopy = true;

            return exitDetail;
        }
    }
}