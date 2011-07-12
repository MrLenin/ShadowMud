using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq.Mapping;
using System.Diagnostics;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.text_table")]
    public class Text : INotifyPropertyChanged
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

        #region string Data

        private string _data;

        [DebuggerNonUserCode]
        [Column(Storage = "_data", Name = "data", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Data { get { return _data; } set { SetField(ref _data, value, "Data"); } }

        #endregion

        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region string Name

        private string _name;

        [DebuggerNonUserCode]
        [Column(Storage = "_name", Name = "name", DbType = "varchar(MAX)", CanBeNull = false)]
        public string Name { get { return _name; } set { SetField(ref _name, value, "Name"); } }

        #endregion
    }
}