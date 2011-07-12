using System;
using System.Linq;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Cache
{
    public class RoomDbCache : LRUCache<Room>
    {
        private const string SelectExitsFormat = "SELECT * FROM room_exit_details WHERE roomid = {0}";
        private const string SelectKeywordsFormat = "SELECT * FROM exit_detail_keywords WHERE exitdetaild = {0}";
        private const string SelectRoomFormat = "SELECT * FROM rooms AS r WHERE r.id = {0};";
        private const string SelectZoneIDFormat = "SELECT z.zoneid FROM zone_details AS z WHERE z.roomid = {0}";

        private const string ValidateFormat = "SELECT updates FROM updates WHERE id = {0}";
        private readonly IIndex<int> _findByRoomID;
        private readonly ShadowDb _database;
        private long _tableVersion;

        /// <summary>constructor creates cache and multiple indexes</summary>
        public RoomDbCache(ShadowDb db)
            : base(20, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(5), null)
        {
            _database = db;
            IsValid = IsDataValid;
            _findByRoomID = AddIndex("RoomID", room => room.ID, LoadFromRoomID);
            IsDataValid();
        }

        /// <summary>retrieve items by userid</summary>
        public Room FindByRoomID(int roomID)
        {
            return _findByRoomID[roomID];
        }

        /// <summary>check to see if users table has changed, if so dump cache and reload.</summary>
        internal bool IsDataValid()
        {
            var query = string.Format(ValidateFormat, (int)TableID.Rooms);
            var oldVersion = _tableVersion;
            _tableVersion = _database.ExecuteCommand(query);
            return (oldVersion == _tableVersion);
        }

        /// <summary>when FindByUserID can't find a user, this method loads the data from the db</summary>
        private Room LoadFromRoomID(int roomID)
        {
            var query = string.Format(SelectRoomFormat, roomID);

            var rooms = _database.ExecuteQuery<Room>(query);

            if (rooms.Count() == 0)
                return null;

            var room = rooms.First();

            query = string.Format(SelectZoneIDFormat, room.ID);
            room.ZoneID = _database.ExecuteCommand(query);

            query = string.Format(SelectExitsFormat, room.ID);

            var roomExits = _database.ExecuteQuery<ExitDetail>(query);

            if (roomExits.Count() == 0)
                return room;

            foreach (var exit in roomExits)
                room.AddExitDetail(exit);

            foreach (var exit in room.ExitDetails)
            {
                query = string.Format(SelectZoneIDFormat, exit.TargetRoom);
                exit.ZoneID = _database.ExecuteCommand(query);

                query = string.Format(SelectKeywordsFormat, exit.ID);

                var exitKeywords = _database.ExecuteQuery<ExitKeyword>(query);

                if (exitKeywords.Count() == 0)
                    continue;

                foreach (var keyword in exitKeywords)
                    exit.AddExitKeyword(keyword);
            }

            return room;
        }
    }
}