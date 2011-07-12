using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.zone_details")]
    public class ZoneDetail : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged handling

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

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
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int RoomID

        private int _roomID;

        [DebuggerNonUserCode]
        [Column(Storage = "_roomID", Name = "roomid", DbType = "integer(32,0)", CanBeNull = false)]
        public int RoomID { get { return _roomID; } set { SetField(ref _roomID, value, "RoomID"); } }

        #endregion

        #region int ZoneID

        private int _zoneID;

        [DebuggerNonUserCode]
        [Column(Storage = "_zoneID", Name = "zoneid", DbType = "integer(32,0)", CanBeNull = false)]
        public int ZoneID { get { return _zoneID; } set { SetField(ref _zoneID, value, "ZoneID"); } }

        #endregion

        #region Parents

        private EntityRef<Room> _rooms;

        private EntityRef<Zone> _zones;

        [Association(Storage = "_rooms", ThisKey = "RoomID", Name = "rooms_zone_details_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Room Rooms { get { return _rooms.Entity; } set { _rooms.Entity = value; } }

        [Association(Storage = "_zones", ThisKey = "ZoneID", Name = "zones_zone_details_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Zone Zones { get { return _zones.Entity; } set { _zones.Entity = value; } }

        #endregion
    }
}