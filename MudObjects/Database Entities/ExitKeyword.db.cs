using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.exit_detail_keywords")]
    public partial class ExitKeyword : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public ExitKeyword()
        {
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
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true,
            CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region int ExitDetailID

        private int _exitDetailID;

        [DebuggerNonUserCode]
        [Column(Storage = "_exitDetailID", Name = "exitdetailid", DbType = "integer(32,0)", CanBeNull = false)]
        public int ExitDetailID { get { return _exitDetailID; } set { SetField(ref _exitDetailID, value, "ExitDetailID"); } }

        #endregion

        #region Parents

        private EntityRef<Room> _roomExitDetail;

        [Association(Storage = "_roomExitDetail", ThisKey = "ExitDetailID", Name = "exit_details_keywords_id_fkey", IsForeignKey = true)]
        [DebuggerNonUserCode]
        public Room Source { get { return _roomExitDetail.Entity; } set { _roomExitDetail.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public ExitKeyword Clone()
        {
            var exitKeyword = MemberwiseClone() as ExitKeyword;

            if (exitKeyword != null)
                exitKeyword.IsCopy = true;

            return exitKeyword;
        }
    }
}