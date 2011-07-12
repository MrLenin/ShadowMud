using System.Collections.Generic;

namespace ShadowMUD.MudObjects
{
    partial class Object
    {
        private readonly List<ObjectKeyword> _keywords;
        private readonly List<Object> _contains; /**< List of objects being carried, or NULL */
// ReSharper disable UnaccessedField.Local
        private readonly List<Character> _sittingHere; /**< For furniture, who is sitting in it */
// ReSharper restore UnaccessedField.Local

        private readonly List<ObjectAffect> _affects;

        public ObjectWearFlags Wearflags;
        public ObjectFlags Flags;

        public Object()
        {
            _keywords = new List<ObjectKeyword>();
            _contains = new List<Object>();
            _sittingHere = new List<Character>();
            _affects = new List<ObjectAffect>();
        }

        #region  Keywords

        public ObjectKeyword this[int index]
        {
            get { return _keywords.Count > index ? _keywords[index] : null; }
            set { if (_keywords.Count > index) _keywords[index] = value; }
        }

        internal IEnumerable<ObjectKeyword> Keywords { get { return _keywords; } }

        public void AddExitKeyword(ObjectKeyword keyword) { _keywords.Add(keyword); }

        #endregion

        public void AddAffect(ObjectAffect affect) { _affects.Add(affect); }

        public void AddObjectTo(Object obj) { _contains.Add(obj); }

        public ObjectDetail Detail { get; set; }
    }
}