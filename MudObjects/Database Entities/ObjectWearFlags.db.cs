using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Diagnostics;
using System.Reflection;

namespace ShadowMUD.MudObjects
{
    [Table(Name = "dbo.object_wearflags")]
    public class ObjectWearFlags : INotifyPropertyChanged, ICloneable
    {
        private readonly Dictionary<string, PropertyInfo> _modifiedList;
        public bool IsCopy;

        public ObjectWearFlags() { _modifiedList = new Dictionary<string, PropertyInfo>(); }

        #region INotifyPropertyChanged handling

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

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

        #region bool Head

        private bool _head;

        [DebuggerNonUserCode]
        [Column(Storage = "_head", Name = "head", DbType = "bit", CanBeNull = false)]
        public bool Head { get { return _head; } set { SetField(ref _head, value, "Head"); } }

        #endregion

        #region bool Face

        private bool _face;

        [DebuggerNonUserCode]
        [Column(Storage = "_face", Name = "face", DbType = "bit", CanBeNull = false)]
        public bool Face { get { return _face; } set { SetField(ref _face, value, "Face"); } }

        #endregion

        #region bool Neck

        private bool _neck;

        [DebuggerNonUserCode]
        [Column(Storage = "_neck", Name = "neck", DbType = "bit", CanBeNull = false)]
        public bool Neck { get { return _neck; } set { SetField(ref _neck, value, "Neck"); } }

        #endregion

        #region bool Chest

        private bool _chest;

        [DebuggerNonUserCode]
        [Column(Storage = "_chest", Name = "chest", DbType = "bit", CanBeNull = false)]
        public bool Chest { get { return _chest; } set { SetField(ref _chest, value, "Chest"); } }

        #endregion

        #region bool LeftArm

        private bool _leftArm;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftArm", Name = "leftarm", DbType = "bit", CanBeNull = false)]
        public bool LeftArm { get { return _leftArm; } set { SetField(ref _leftArm, value, "LeftArm"); } }

        #endregion

        #region bool RightArm

        private bool _rightArm;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightArm", Name = "rightarm", DbType = "bit", CanBeNull = false)]
        public bool RightArm { get { return _rightArm; } set { SetField(ref _rightArm, value, "RightArm"); } }

        #endregion

        #region bool LeftForearm

        private bool _leftForearm;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftForearm", Name = "leftforearm", DbType = "bit", CanBeNull = false)]
        public bool LeftForearm { get { return _leftForearm; } set { SetField(ref _leftForearm, value, "LeftForearm"); } }

        #endregion

        #region bool RightForearm

        private bool _rightForearm;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightForearm", Name = "rightforearm", DbType = "bit", CanBeNull = false)]
        public bool RightForearm { get { return _rightForearm; } set { SetField(ref _rightForearm, value, "RightForearm"); } }

        #endregion

        #region bool LeftHand

        private bool _leftHand;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftHand", Name = "lefthand", DbType = "bit", CanBeNull = false)]
        public bool LeftHand { get { return _leftHand; } set { SetField(ref _leftHand, value, "LeftHand"); } }

        #endregion

        #region bool RightHand

        private bool _rightHand;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightHand", Name = "righthand", DbType = "bit", CanBeNull = false)]
        public bool RightHand { get { return _rightHand; } set { SetField(ref _rightHand, value, "RightHand"); } }

        #endregion

        #region bool Abdomen

        private bool _abdomen;

        [DebuggerNonUserCode]
        [Column(Storage = "_abdomen", Name = "abdomen", DbType = "bit", CanBeNull = false)]
        public bool Abdomen { get { return _abdomen; } set { SetField(ref _abdomen, value, "Abdomen"); } }

        #endregion

        #region bool LeftThigh

        private bool _leftThigh;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftThigh", Name = "leftthigh", DbType = "bit", CanBeNull = false)]
        public bool LeftThigh { set { SetField(ref _leftThigh, value, "LeftThigh"); } }

        #endregion

        #region bool RightThigh

        private bool _rightThigh;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightThigh", Name = "rightthigh", DbType = "bit", CanBeNull = false)]
        public bool RightThigh { get { return _rightThigh; } set { SetField(ref _rightThigh, value, "RightThigh"); } }

        #endregion

        #region bool LeftLeg

        private bool _leftLeg;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftLeg", Name = "leftleg", DbType = "bit", CanBeNull = false)]
        public bool LeftLeg { get { return _leftLeg; } set { SetField(ref _leftLeg, value, "LeftLeg"); } }

        #endregion

        #region bool RightLeg

        private bool _rightLeg;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightLeg", Name = "rightleg", DbType = "bit", CanBeNull = false)]
        public bool RightLeg { get { return _rightLeg; } set { SetField(ref _rightLeg, value, "RightLeg"); } }

        #endregion

        #region bool LeftFoot

        private bool _leftFoot;

        [DebuggerNonUserCode]
        [Column(Storage = "_leftFoot", Name = "leftfoot", DbType = "bit", CanBeNull = false)]
        public bool LeftFoot { get { return _leftFoot; } set { SetField(ref _leftFoot, value, "LeftFoot"); } }

        #endregion

        #region bool RightFoot

        private bool _rightFoot;

        [DebuggerNonUserCode]
        [Column(Storage = "_rightFoot", Name = "rightfoot", DbType = "bit", CanBeNull = false)]
        public bool RightFoot { get { return _rightFoot; } set { SetField(ref _rightFoot, value, "RightFoot"); } }

        #endregion
        
        #region int ID

        private int _id;

        [DebuggerNonUserCode]
        [Column(Storage = "_id", Name = "id", DbType = "integer(32,0)", IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
        public int ID { get { return _id; } set { SetField(ref _id, value, "ID"); } }

        #endregion

        #region Parents

        private EntityRef<Object> _object;

        [DebuggerNonUserCode]
        [Association(Storage = "_object", ThisKey = "ID", Name = "objects_wearflags_id_fkey", IsForeignKey = true)]
        public Object Source { get { return _object.Entity; } set { _object.Entity = value; } }

        #endregion

        public IEnumerable<PropertyInfo> Modified { get { return _modifiedList.Values; } }

        #region ICloneable Members

        object ICloneable.Clone() { return Clone(); }

        #endregion

        public ObjectWearFlags Clone()
        {
            var objectWearFlags = MemberwiseClone() as ObjectWearFlags;

            if (objectWearFlags != null)
                objectWearFlags.IsCopy = true;

            return objectWearFlags;
        }
    }
}