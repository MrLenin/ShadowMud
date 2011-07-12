using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.object_keywords")]
    public partial class ObjectKeyword : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public ObjectKeyword()
        {
            _modifiedList = new Dictionary<string, PropertyInfo>();
        }

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

        #region string  Keyword

        private string _keyword;

        [DebuggerNonUserCode]
        [Column(Storage = "_keyword", Name = "keyword", DbType = "varchar(50)", CanBeNull = false)]
        public string Keyword { get { return _keyword; } set { SetField(ref _keyword, value, "Keyword"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int ObjectID

        private int _objectID;

        [DebuggerNonUserCode]
        [Column(Storage = "_objectID", Name = "objectid", DbType = "integer(32,0)", CanBeNull = false)]
        public int ObjectID { get { return _objectID; } set { SetField(ref _objectID, value, "ObjectID"); } }

        #endregion

        #region Parents

        private EntityRef<Object> _object;

        [Association(Storage = "_object", ThisKey = "ObjectID", Name = "objects_keywords_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Object Source { get { return _object.Entity; } set { _object.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public ObjectKeyword Clone()
        {
            var objectKeyword = MemberwiseClone() as ObjectKeyword;

            if (objectKeyword != null)
                objectKeyword.IsCopy = true;

            return objectKeyword;
        }
    }
}