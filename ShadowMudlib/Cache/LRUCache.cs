using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ShadowMud.Cache
{
    /// <summary>LRUCache is a thread safe cache that automatically removes the items that have not been accessed for a long time.
    /// an object will never be removed if it has been accessed within the minAge timeSpan, else it will be removed if it
    /// is older than maxAge or the cache is beyond it's desired size capacity.  A periodic check is made when accessing nodes that determines
    /// if the cache is out of date, and clears the cache (allowing new objects to be loaded upon next request). </summary>
    /// 
    /// <remarks>Each Index provides dictionary key / value access to any object in cache, and has the ability to load any object that is
    /// not found. The Indexes use Weak References allowing objects in index to be garbage collected if no other objects are using them.
    /// The objects are not directly stored in indexes, rather, indexes hold Nodes which are linked list nodes. The LifespanMgr maintains
    /// a list of Nodes in each AgeBag which hold the objects and prevents them from being garbage collected.  Any time an object is retrieved 
    /// through a Index it is marked to belong to the current AgeBag.  When the cache gets too full/old the oldest age bag is emptied moving any 
    /// nodes that have been touched to the correct AgeBag and removing the rest of the nodes in the bag. Once a node is removed from the 
    /// LifespanMgr it becomes elegible for garbage collection.  The Node is not removed from the Indexes immediately.  If a Index retrieves the 
    /// node prior to garbage collection it is reinserted into the current AgeBag's Node list.  If it has already been garbage collected a new  
    /// object gets loaded.  If the Index size exceeds twice the capacity the index is cleared and rebuilt.  
    /// 
    /// !!!!! THERE ARE 2 DIFFERENT LOCKS USED BY CACHE - so care is required when altering code or you may introduce deadlocks !!!!!
    ///        order of lock nesting is LifespanMgr (Monitor) / Index (ReaderWriterLock)
    /// </remarks>
    public class LRUCache<TItemType> where TItemType : class
    {
        #region delegates

        public delegate TKeyType GetKeyFunc<out TKeyType>(TItemType item);

        public delegate bool IsCacheValid();

        public delegate TItemType LoadItemFunc<in TKeyType>(TKeyType key);

        #endregion

        #region interfaces

        #region Nested type: IIndex

        /// <summary>The public wrapper for a Index</summary>
        public interface IIndex<in TKeyType>
        {
            /// <summary>Getter for index</summary>
            /// <param name="key">key to find (or load if needed)</param>
            /// <returns>the object value associated with the cache</returns>
            TItemType this[TKeyType key] { get; }

            /// <summary>Delete object that matches key from cache</summary>
            /// <param name="key">key to find</param>
            void Remove(TKeyType key);
        }

        /// <summary>Because there is no auto inheritance between generic types, this interface is used to send messages to Index objects</summary>
        protected interface IIndex
        {
            void ClearIndex();
            bool AddItem(INode item);
            INode FindItem(TItemType item);
            int RebuildIndex();
        }

        #endregion

        #region Nested type: INode

        /// <summary>This interface exposes the public part of a LifespanMgr.Node</summary>
        protected interface INode
        {
            TItemType Value { get; }
            void Touch();
            void Remove();
        }

        #endregion

        #endregion

        #region private nested classes

        #region Nested type: Index

        /// <summary>Index provides dictionary key / value access to any object in cache</summary>
        private class Index<TKeyType> : IIndex<TKeyType>, IIndex
        {
            private readonly GetKeyFunc<TKeyType> _getKey;
            private readonly Dictionary<TKeyType, WeakReference> _index;
            private readonly LoadItemFunc<TKeyType> _loadItem;
            private readonly ReaderWriterLock _lock = new ReaderWriterLock();
            private readonly LRUCache<TItemType> _owner;

            /// <summary>constructor</summary>
            /// <param name="owner">parent of index</param>
            /// <param name="getKey">delegate to get key from object</param>
            /// <param name="loadItem">delegate to load object if it is not found in index</param>
            public Index(LRUCache<TItemType> owner, GetKeyFunc<TKeyType> getKey, LoadItemFunc<TKeyType> loadItem)
            {
                Debug.Assert(owner != null, "owner argument required");
                Debug.Assert(getKey != null, "GetKey delegate required");
                _owner = owner;
                _index = new Dictionary<TKeyType, WeakReference>(_owner._capacity*2);
                _getKey = getKey;
                _loadItem = loadItem;
                RebuildIndex();
            }

            #region IIndex Members

            /// <summary>try to find this item in the index and return Node</summary>
            public INode FindItem(TItemType item)
            {
                return GetNode(_getKey(item));
            }

            /// <summary>Remove all items from index</summary>
            public void ClearIndex()
            {
                RWLock.GetWriteLock(_lock, LockTimeout, delegate
                                                            {
                                                                _index.Clear();
                                                                return true;
                                                            });
            }

            /// <summary>Add new item to index</summary>
            /// <param name="item">item to add</param>
            /// <returns>was item key previously contained in index</returns>
            public bool AddItem(INode item)
            {
                TKeyType key = _getKey(item.Value);
                return RWLock.GetWriteLock(_lock, LockTimeout,
                                           delegate
                                               {
                                                   bool isDup = _index.ContainsKey(key);
                                                   _index[key] = new WeakReference(item, false);
                                                   return isDup;
                                               });
            }

            /// <summary>removes all items from index and reloads each item (this gets rid of dead nodes)</summary>
            public int RebuildIndex()
            {
                lock (_owner._lifeSpan)
                    return RWLock.GetWriteLock(_lock, LockTimeout,
                                               delegate
                                                   {
                                                       _index.Clear();
                                                       foreach (INode item in _owner._lifeSpan)
                                                           AddItem(item);
                                                       return _index.Count;
                                                   });
            }

            #endregion

            #region IIndex<TKeyType> Members

            /// <summary>Getter for index</summary>
            /// <param name="key">key to find (or load if needed)</param>
            /// <returns>the object value associated with key, or null if not found & could not be loaded</returns>
            public TItemType this[TKeyType key]
            {
                get
                {
                    INode node = GetNode(key);
                    if (node != null)
                        node.Touch();
                    if ((node == null || node.Value == null) && _loadItem != null)
                        node = _owner.Add(_loadItem(key));
                    return (node == null ? null : node.Value);
                }
            }

            /// <summary>Delete object that matches key from cache</summary>
            /// <param name="key"></param>
            public void Remove(TKeyType key)
            {
                INode node = GetNode(key);
                if (node != null)
                    node.Remove();
                _owner._lifeSpan.CheckValid();
            }

            #endregion

            private INode GetNode(TKeyType key)
            {
                return RWLock.GetReadLock(_lock, LockTimeout,
                                          delegate
                                              {
                                                  WeakReference value;
                                                  return
                                                      (INode) (_index.TryGetValue(key, out value) ? value.Target : null);
                                              });
            }
        }

        #endregion

        #region Nested type: LifespanMgr

        private class LifespanMgr : IEnumerable<INode>
        {
            private const int Size = 265; // based on 240 timeslices + 20 bags for ItemLimit + 5 bags empty buffer
            private readonly int _bagItemLimit;

            private readonly AgeBag[] _bags;
            private readonly TimeSpan _maxAge;
            private readonly TimeSpan _minAge;
            private readonly LRUCache<TItemType> _owner;
            private readonly TimeSpan _timeSlice;
            private int _current;
            private AgeBag _currentBag;
            private int _currentSize;
            private DateTime _nextValidCheck;
            private int _oldest;

            public LifespanMgr(LRUCache<TItemType> owner, TimeSpan minAge, TimeSpan maxAge)
            {
                _owner = owner;
                int maxMs = Math.Min((int) maxAge.TotalMilliseconds, 12*60*60*1000); // max = 12 hours
                _minAge = minAge;
                _maxAge = TimeSpan.FromMilliseconds(maxMs);
                _timeSlice = TimeSpan.FromMilliseconds(maxMs/240.0); // max timeslice = 3 min
                _bagItemLimit = _owner._capacity/20; // max 5% of capacity per bag
                _bags = new AgeBag[Size];
                for (int loop = Size - 1; loop >= 0; --loop)
                    _bags[loop] = new AgeBag();
                OpenCurrentBag(DateTime.Now, 0);
            }

            #region IEnumerable<LRUCache<TItemType>.INode> Members

            /// <summary>Create item enumerator</summary>
            public IEnumerator<INode> GetEnumerator()
            {
                for (int bagNumber = _current; bagNumber >= _oldest; --bagNumber)
                {
                    AgeBag bag = _bags[bagNumber];
                    // if bag.first == null then bag is empty or being cleaned up, so skip it!
                    for (Node node = bag.First; node != null && bag.First != null; node = node.Next)
                        if (node.Value != null)
                            yield return node;
                }
            }

            /// <summary>Create item enumerator</summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            public INode Add(TItemType value)
            {
                return new Node(this, value);
            }

            /// <summary>checks to see if cache is still valid and if LifespanMgr needs to do maintenance</summary>
            public void CheckValid()
            {
                DateTime now = DateTime.Now;
                // Note: Monitor.Enter(this) / Monitor.Exit(this) is the same as lock(this)... We are using Monitor.TryEnter() because it
                // Note: does not wait for a lock, if lock is currently held then skip and let next Touch perform cleanup.
                if ((_currentSize > _bagItemLimit || now > _nextValidCheck) && Monitor.TryEnter(this))
                    try
                    {
                        if ((_currentSize > _bagItemLimit || now > _nextValidCheck))
                            // if cache is no longer valid throw contents away and start over, else cleanup old items
                            if (_current > 1000000 || (_owner.IsValid != null && !_owner.IsValid()))
                                _owner.Clear();
                            else
                                CleanUp(now);
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
            }

            /// <summary>remove old items or items beyond capacity from LifespanMgr allowing them to be garbage collected</summary>
            /// <remarks>since we do not physically move items when touched we must check items in bag to determine if they should be deleted 
            /// or moved.  Also items that were removed by setting value to null get removed now.  Rremoving an item from LifespanMgr allows 
            /// it to be garbage collected.  If removed item is retrieved by index prior to GC then it will be readded to LifespanMgr.</remarks>
            private void CleanUp(DateTime now)
            {
                lock (this)
                {
                    //calculate how many items should be removed
                    DateTime maxAge = now.Subtract(_maxAge);
                    DateTime minAge = now.Subtract(_minAge);
                    int itemsToRemove = _owner._curCount - _owner._capacity;
                    AgeBag bag = _bags[_oldest%Size];
                    while (_current != _oldest &&
                           (_current - _oldest > Size - 5 || bag.StartTime < maxAge ||
                            (itemsToRemove > 0 && bag.StopTime > minAge)))
                    {
                        // cache is still too big / old so remove oldest bag
                        Node node = bag.First;
                        bag.First = null;
                        while (node != null)
                        {
                            Node next = node.Next;
                            node.Next = null;
                            if (node.Value != null && node.AgeBag != null)
                                if (node.AgeBag == bag)
                                {
                                    // item has not been touched since bag was closed, so remove it from LifespanMgr
                                    ++itemsToRemove;
                                    node.AgeBag = null;
                                    Interlocked.Decrement(ref _owner._curCount);
                                }
                                else
                                {
                                    // item has been touched and should be moved to correct age bag now
                                    node.Next = node.AgeBag.First;
                                    node.AgeBag.First = node;
                                }
                            node = next;
                        }
                        // increment oldest bag
                        bag = _bags[(++_oldest)%Size];
                    }
                    OpenCurrentBag(now, ++_current);
                    CheckIndexValid();
                }
            }

            private void CheckIndexValid()
            {
                // if indexes are getting too big its time to rebuild them
                if ((_owner._totalCount - _owner._curCount) <= _owner._capacity) return;

                foreach (var keyValue in _owner.IndexList)
                {
                    _owner._curCount = keyValue.Value.RebuildIndex();
                    _owner._totalCount = _owner._curCount;
                }
            }

            /// <summary>Remove all items from LifespanMgr and reset</summary>
            public void Clear()
            {
                lock (this)
                {
                    foreach (AgeBag bag in _bags)
                    {
                        Node node = bag.First;
                        bag.First = null;
                        while (node != null)
                        {
                            Node next = node.Next;
                            node.Next = null;
                            node.AgeBag = null;
                            node = next;
                        }
                    }
                    // reset item counters 
                    _owner._curCount = _owner._totalCount = 0;
                    // reset age bags
                    OpenCurrentBag(DateTime.Now, _oldest = 0);
                }
            }

            /// <summary>ready a new current AgeBag for use and close the previous one</summary>
            private void OpenCurrentBag(DateTime now, int bagNumber)
            {
                lock (this)
                {
                    // close last age bag
                    if (_currentBag != null)
                        _currentBag.StopTime = now;
                    // open new age bag for next time slice
                    AgeBag currentBag = _bags[(_current = bagNumber)%Size];
                    currentBag.StartTime = now;
                    currentBag.First = null;
                    _currentBag = currentBag;
                    // reset counters for CheckValid()
                    _nextValidCheck = now.Add(_timeSlice);
                    _currentSize = 0;
                }
            }

            #region Nested type: AgeBag

            /// <summary>container class used to hold nodes added within a descrete timeframe</summary>
            private class AgeBag
            {
                public Node First;
                public DateTime StartTime;
                public DateTime StopTime;
            }

            #endregion

            #region Nested type: Node

            /// <summary>LRUNodes is a linked list of items</summary>
            private class Node : INode
            {
                private readonly LifespanMgr _mgr;
// ReSharper disable MemberHidesStaticFromOuterClass
                public AgeBag AgeBag;
// ReSharper restore MemberHidesStaticFromOuterClass
                public Node Next;

                /// <summary>constructor</summary>
                public Node(LifespanMgr mgr, TItemType value)
                {
                    _mgr = mgr;
                    Value = value;
                    Interlocked.Increment(ref _mgr._owner._curCount);
                    Touch();
                }

                #region INode Members

                /// <summary>returns the object</summary>
                public TItemType Value { get; private set; }

                /// <summary>Updates the status of the node to prevent it from being dropped from cache</summary>
                public void Touch()
                {
                    if (Value != null && AgeBag != _mgr._currentBag)
                    {
                        if (AgeBag == null)
                            lock (_mgr)
                                if (AgeBag == null)
                                {
                                    // if node.AgeBag==null then the object is not currently managed by LifespanMgr so add it
                                    Next = _mgr._currentBag.First;
                                    _mgr._currentBag.First = this;
                                    Interlocked.Increment(ref _mgr._owner._curCount);
                                }
                        AgeBag = _mgr._currentBag;
                        Interlocked.Increment(ref _mgr._currentSize);
                    }
                    _mgr.CheckValid();
                }

                /// <summary>Removes the object from node, thereby removing it from all indexes and allows it to be garbage collected</summary>
                public void Remove()
                {
                    if (AgeBag != null && Value != null)
                        Interlocked.Decrement(ref _mgr._owner._curCount);
                    Value = null;
                    AgeBag = null;
                }

                #endregion
            }

            #endregion
        } ;

        #endregion

        #endregion

        #region private data

        private const int LockTimeout = 30000;

        protected readonly Dictionary<string, IIndex> IndexList = new Dictionary<string, IIndex>();
        protected readonly IsCacheValid IsValid;
        private readonly int _capacity;
        private readonly LifespanMgr _lifeSpan;

        private int _curCount;
        private int _totalCount;

        #endregion

        /// <summary>Constructor</summary>
        /// <param name="capacity">the normal item limit for cache (Count may exeed capacity due to minAge)</param>
        /// <param name="minAge">the minimium time after an access before an item becomes eligible for removal, during this time
        /// the item is protected and will not be removed from cache even if over capacity</param>
        /// <param name="maxAge">the max time that an object will sit in the cache without being accessed, before being removed</param>
        /// <param name="isValid">delegate used to determine if cache is out of date.  Called before index access not more than once per 10 seconds</param>
        public LRUCache(int capacity, TimeSpan minAge, TimeSpan maxAge, IsCacheValid isValid)
        {
            _capacity = capacity;
            IsValid = isValid;
            _lifeSpan = new LifespanMgr(this, minAge, maxAge);
        }

        /// <summary>Retrieve a index by name</summary>
        public IIndex<TKeyType> GetIndex<TKeyType>(String indexName)
        {
            IIndex index;
            return (IndexList.TryGetValue(indexName, out index) ? index as IIndex<TKeyType> : null);
        }

        /// <summary>Retrieve a object by index name / key</summary>
        public TItemType GetValue<TKeyType>(String indexName, TKeyType key)
        {
            IIndex<TKeyType> index = GetIndex<TKeyType>(indexName);
            return (index == null ? null : index[key]);
        }

        /// <summary>Add a new index to the cache</summary>
        /// <typeparam name="TKeyType">the type of the key value</typeparam>
        /// <param name="indexName">the name to be associated with this list</param>
        /// <param name="getKey">delegate to get key from object</param>
        /// <param name="loadItem">delegate to load object if it is not found in index</param>
        /// <returns>the newly created index</returns>
        public IIndex<TKeyType> AddIndex<TKeyType>(String indexName, GetKeyFunc<TKeyType> getKey,
                                                   LoadItemFunc<TKeyType> loadItem)
        {
            var index = new Index<TKeyType>(this, getKey, loadItem);
            IndexList[indexName] = index;
            return index;
        }

        /// <summary>Add an item to the cache (not needed if accessed by index)</summary>
        public void AddItem(TItemType item)
        {
            Add(item);
        }

        /// <summary>Add an item to the cache</summary>
        private INode Add(TItemType item)
        {
            if (item == null)
                return null;

            // see if item is already in index
            INode node = null;

            foreach (var keyValue in IndexList)
                if ((node = keyValue.Value.FindItem(item)) != null)
                    break;

            // dupl is used to prevent total count from growing when item is already in indexes (only new Nodes)
            bool isDupl = (node != null && node.Value == item);

            if (!isDupl)
                node = _lifeSpan.Add(item);

            // make sure node gets inserted into all indexes
            foreach (var keyValue in IndexList)
                if (!keyValue.Value.AddItem(node))
                    isDupl = true;

            if (!isDupl)
                Interlocked.Increment(ref _totalCount);

            return node;
        }

        /// <summary>Remove all items from cache</summary>
        public void Clear()
        {
            foreach (var keyValue in IndexList)
                keyValue.Value.ClearIndex();
            _lifeSpan.Clear();
        }
    }
}