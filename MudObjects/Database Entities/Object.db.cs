using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.objects")]
    public partial class Object : INotifyPropertyChanged
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

        #region int Cost

        private int _cost;

        [DebuggerNonUserCode]
        [Column(Storage = "_cost", Name = "cost", DbType = "integer(32,0)", CanBeNull = false)]
        public int Cost { get { return _cost; } set { SetField(ref _cost, value, "Cost"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region bool IsContainer

        private bool _isContainer;

        [DebuggerNonUserCode]
        [Column(Storage = "_isContainer", Name = "iscontainer", DbType = "boolean", CanBeNull = false)]
        public bool IsContainer { get { return _isContainer; } set { SetField(ref _isContainer, value, "IsContainer"); } }

        #endregion

        #region string Title

        private string _title;

        [DebuggerNonUserCode]
        [Column(Storage = "_title", Name = "title", DbType = "text", CanBeNull = false)]
        public string Title { get { return _title; } set { SetField(ref _title, value, "Title"); } }

        #endregion

        #region short Type

        private short _type;

        [DebuggerNonUserCode]
        [Column(Storage = "_type", Name = "type", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Type { get { return _type; } set { SetField(ref _type, value, "Type"); } }

        #endregion

        #region short Weight

        private short _weight;

        [DebuggerNonUserCode]
        [Column(Storage = "_weight", Name = "weight", DbType = "smallint(16,0)", CanBeNull = false)]
        public short Weight { get { return _weight; } set { SetField(ref _weight, value, "Weight"); } }

        #endregion

        #region Children

        [Association(Storage = null, OtherKey = "ID", Name = "objects_details_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ObjectDetail> ObjectDetails { get; set; }

        [Association(Storage = null, OtherKey = "ParentID", Name = "fk_obj_id")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterInventory> ParentInventory { get; set; }

        [Association(Storage = null, OtherKey = "ObjectID", Name = "fk_inv_obj_id")]
        [DebuggerNonUserCode]
        public EntitySet<CharacterInventory> ObjectInventory { get; set; }

        [Association(Storage = null, OtherKey = "ObjectID", Name = "objects_affects_id_fkey")]
        [DebuggerNonUserCode]
        public EntitySet<ObjectAffect> ObjectAffect { get; set; }

        #endregion
    }
}