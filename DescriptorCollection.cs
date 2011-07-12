using System.Collections;
using System.Collections.Generic;

namespace ShadowMUD
{
    internal class DescriptorCollection : ICollection<PlayerDescriptor>
    {
        private readonly List<PlayerDescriptor> _descriptorList;

        public DescriptorCollection()
        {
            _descriptorList = new List<PlayerDescriptor>();
        }

        #region ICollection<PlayerDescriptor> Members

        public void Add(PlayerDescriptor item)
        {
            _descriptorList.Add(item);
        }

        public void Clear()
        {
            _descriptorList.Clear();
        }

        public bool Contains(PlayerDescriptor item)
        {
            return _descriptorList.Contains(item);
        }

        public void CopyTo(PlayerDescriptor[] array, int arrayIndex)
        {
            _descriptorList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _descriptorList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(PlayerDescriptor item)
        {
            return _descriptorList.Remove(item);
        }

        public IEnumerator<PlayerDescriptor> GetEnumerator()
        {
            return _descriptorList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _descriptorList.GetEnumerator();
        }

        #endregion
    }
}