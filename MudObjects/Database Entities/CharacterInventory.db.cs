using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.character_inventories")]
    public class CharacterInventory : INotifyPropertyChanged
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

        #region int? ParentID

        private int? _parentID;

        [DebuggerNonUserCode]
        [Column(Storage = "_parentID", Name = "parentid", DbType = "integer(32,0)")]
        public int? ParentID { get { return _parentID; } set { SetField(ref _parentID, value, "ParentID"); } }

        #endregion

        #region int PlayerID

        private int _playerID;

        [DebuggerNonUserCode]
        [Column(Storage = "_playerID", Name = "playerid", DbType = "integer(32,0)", CanBeNull = false)]
        public int PlayerID { get { return _playerID; } set { SetField(ref _playerID, value, "PlayerID"); } }

        #endregion

        #region Parents

        private EntityRef<Character> _characters;

        private EntityRef<Object> _objects;

        private EntityRef<Object> _parentObject;

        [Association(Storage = "_characters", ThisKey = "PlayerID", Name = "fk_char_id", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Character Characters { get { return _characters.Entity; } set { _characters.Entity = value; } }

        [Association(Storage = "_objects", ThisKey = "ParentID", Name = "fk_obj_id", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Object ParentObject { get { return _objects.Entity; } set { _objects.Entity = value; } }

        [Association(Storage = "_parentObject", ThisKey = "ObjectID", Name = "fk_inv_obj_id", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Object Object { get { return _parentObject.Entity; } set { _parentObject.Entity = value; } }

        #endregion
    }
}