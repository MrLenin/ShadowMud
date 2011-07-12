using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.update_table")]
    public class UpdateTable : INotifyPropertyChanged
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

        //#region  Blah

        //private int[] _blah;

        //[DebuggerNonUserCode]
        //[Column(Storage = "_blah", Name = "Blah", DbType = "ARRAY")]
        //public int[] Blah
        //{
        //    get { return _blah; }
        //    set
        //    {
        //        if (value == _blah) return;
        //        _blah = value;
        //        OnPropertyChanged("Blah");
        //    }
        //}

        //#endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region long Updates

        private long _updates;

        [DebuggerNonUserCode]
        [Column(Storage = "_updates", Name = "updates", DbType = "bigint(64,0)", CanBeNull = false)]
        public long Updates { get { return _updates; } set { SetField(ref _updates, value, "Updates"); } }

        #endregion
    }
}