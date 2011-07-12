using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.zones")]
    public class Zone : INotifyPropertyChanged
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

        #region string Description

        private string _description;

        [DebuggerNonUserCode]
        [Column(Storage = "_description", Name = "description", DbType = "text", CanBeNull = false)]
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
        [Column(Storage = "_title", Name = "title", DbType = "text", CanBeNull = false)]
        public string Title { get { return _title; } set { SetField(ref _title, value, "Title"); } }

        #endregion

        #region Children

        [Association(Storage = null, OtherKey = "ZoneID", Name = "zones_zone_details_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ZoneDetail> ZoneDetails { get; set; }

        #endregion
    }
}