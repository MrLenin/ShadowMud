using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.object_details")]
    public class ObjectDetail : INotifyPropertyChanged
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

        #region string ActionDescription

        private string _actionDescription;

        [DebuggerNonUserCode]
        [Column(Storage = "_actionDescription", Name = "actiondescription", DbType = "text")]
        public string ActionDescription { get { return _actionDescription; } set { SetField(ref _actionDescription, value, "ActionDescription"); } }

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
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region string RoomDescription

        private string _roomDescription;

        [DebuggerNonUserCode]
        [Column(Storage = "_roomDescription", Name = "roomdescription", DbType = "text", CanBeNull = false)]
        public string RoomDescription { get { return _roomDescription; } set { SetField(ref _roomDescription, value, "RoomDescription"); } }

        #endregion

        #region string ShortDescription

        private string _shortDescription;

        [DebuggerNonUserCode]
        [Column(Storage = "_shortDescription", Name = "shortdescription", DbType = "text", CanBeNull = false)]
        public string ShortDescription { get { return _shortDescription; } set { SetField(ref _shortDescription, value, "ShortDescription"); } }

        #endregion

        #region Parents

        private EntityRef<Object> _objects;

        [Association(Storage = "_objects", ThisKey = "ID", Name = "objects_details_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Object Objects { get { return _objects.Entity; } set { _objects.Entity = value; } }

        #endregion
    }
}