using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.object_affects")]
    public class ObjectAffect : INotifyPropertyChanged
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

        #region long ID

        private long _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "bigint(64,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public long ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int ObjectID

        private int _objectID;

        [DebuggerNonUserCode]
        [Column(Storage = "_objectID", Name = "objectid", DbType = "integer(32,0)", CanBeNull = false)]
        public int ObjectID { get { return _objectID; } set { SetField(ref _objectID, value, "ObjectID"); } }

        #endregion

        #region int Target

        private int _target;

        [DebuggerNonUserCode]
        [Column(Storage = "_target", Name = "target", DbType = "integer(32,0)", CanBeNull = false)]
        public int Target { get { return _target; } set { SetField(ref _target, value, "Target"); } }

        #endregion

        #region int Type

        private int _type;

        [DebuggerNonUserCode]
        [Column(Storage = "_type", Name = "type", DbType = "integer(32,0)", CanBeNull = false)]
        public int Type { get { return _type; } set { SetField(ref _type, value, "Type"); } }

        #endregion

        #region int Value

        private int _value;

        [DebuggerNonUserCode]
        [Column(Storage = "_value", Name = "value", DbType = "integer(32,0)", CanBeNull = false)]
        public int Value { get { return _value; } set { SetField(ref _value, value, "Value"); } }

        #endregion

        #region Parents

        private EntityRef<Object> _objects;

        [Association(Storage = "_objects", ThisKey = "ID", Name = "objects_affects_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Object Objects { get { return _objects.Entity; } set { _objects.Entity = value; } }

        #endregion
    }
}