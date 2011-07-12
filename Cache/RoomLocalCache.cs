using System;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Cache
{
    internal class RoomLocalCache : LRUCache<Room>
    {
        private readonly IIndex<int> _findByRoomID;
        private readonly RoomDbCache _dbCache;

        /// <summary>constructor creates cache and multiple indexes</summary>
        public RoomLocalCache(ShadowDb database)
            : base(100, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(20), null)
        {
            _dbCache = new RoomDbCache(database);
            IsValid = IsDataValid;
            _findByRoomID = AddIndex("RoomID", room => room.ID, LoadFromRoomID);
        }

        /// <summary>check to see if users table has changed, if so dump cache and reload.</summary>
        private static bool IsDataValid()
        {
            return false;
        }

        /// <summary>retrieve items by userid</summary>
        public Room FindByRoomID(int roomID)
        {
            return _findByRoomID[roomID];
        }

        public Room GetUnclonedRoom(int roomID)
        {
            return _dbCache.FindByRoomID(roomID);
        }

        /// <summary>when FindByUserID can't find a user, this method loads the data from the db</summary>
        private Room LoadFromRoomID(int roomID)
        {
            var room = _dbCache.FindByRoomID(roomID);

            if (room == null)
                return null;

            room = room.Clone();

            return room;
        }
    }
}