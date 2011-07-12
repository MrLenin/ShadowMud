using System.Data;
using ShadowMUD.Cache;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class RoomManager
    {
        private const string SelectMaxRoomIDQuery = "SELECT r.id FROM rooms AS r WHERE r.id = (SELECT max(id) FROM rooms);";
/*
        private const string NewExitIDQuery = "SELECT nextval('exit_details_sequence');";
        private const string NewRoomIDQuery = "SELECT nextval('room_id_sequence');";
*/
        private const string SelectZoneIDFormat = "SELECT z.zoneid FROM zone_details AS z WHERE z.roomid = {0}";

        private readonly ShadowDb _database;
        private readonly RoomLocalCache _roomCache;

        public RoomManager(IDbConnection connection)
        {
            _database = new ShadowDb(connection);
            _roomCache = new RoomLocalCache(_database);
        }

        public Room this[int zoneID, int roomID]
        {
            get { return _roomCache.FindByRoomID(roomID); }
        }

        public bool AddCharacterTo(int roomID, Character character)
        {
            var room = _roomCache.FindByRoomID(roomID);

            if (room == null)
                return false;

            room.AddCharacter(character);

            return true;
        }

        public void RemoveCharacterFrom(int roomID, Character character)
        {
            var room = _roomCache.FindByRoomID(roomID);

            if (room == null)
                return;

            room.RemoveCharacter(character);
        }

        public Room CreateRoom(Room room)
        {
            //room.ID = database.ExecuteCommand(newRoomIDQuery);
            _database.Rooms.InsertOnSubmit(room);
            _database.SubmitChanges();

            MudManagers.MudInstance.ZoneManager.AddZoneDetail(room.ZoneID, _database.ExecuteCommand(SelectMaxRoomIDQuery));

            return _roomCache.FindByRoomID(room.ID);
        }

        public void CreateExit(Room room, ExitDetail exitDetail)
        {
            //exitDetail.ID = database.ExecuteCommand(newExitIDQuery);
            _database.ExitDetails.InsertOnSubmit(exitDetail);
            _database.SubmitChanges();

            var original = _roomCache.GetUnclonedRoom(room.ID);

            original.AddExitDetail(exitDetail);

            room.AddExitDetail(exitDetail.Clone());

            var query = string.Format(SelectZoneIDFormat, room.ID);
            room[(Exit)exitDetail.ExitDirection].ZoneID = _database.ExecuteCommand(query);
        }

        public void CreateExitKeyword(ExitDetail exitDetail, string keyword)
        {
            var exitKeyword = new ExitKeyword
            {
                ExitDetailID = exitDetail.ID,
                Keyword = keyword
            };

            _database.ExitKeywords.InsertOnSubmit(exitKeyword);
            _database.SubmitChanges();

            var original = _roomCache.GetUnclonedRoom(exitDetail.Source.ID);

            foreach (var detail in original.ExitDetails)
                if (detail.Equals(exitDetail))
                    detail.AddExitKeyword(exitKeyword);

            exitDetail.AddExitKeyword(exitKeyword.Clone());
        }

        public void RevertRoom(Room room)
        {
            //var copy = _roomCache.FindByRoomID(room.ID);

            //if (copy == null)
            //    return;

            var original = _roomCache.GetUnclonedRoom(room.ID);

            if (original == null)
                return;

            foreach (var member in room.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(room, new[] {getter.Invoke(original, null)});
            }

            // BUG - this wont revert changes to room exits (and now, exit keywords), do we want it thus?
        }

        public void SaveRoom(Room room)
        {
            //var copy = _roomCache.FindByRoomID(room.ID);

            //if (copy == null)
            //    return;

            var original = _roomCache.GetUnclonedRoom(room.ID);

            if (original == null)
                return;

            foreach (var member in room.Modified)
            {
                var getter = member.GetGetMethod();
                var setter = member.GetSetMethod();

                setter.Invoke(original, new[] {getter.Invoke(room, null)});
            }

            _database.SubmitChanges();

            _database.TableUpdate(TableID.Rooms);

            // BUG - this wont save changes to room exits (and now, exit keywords), do we want it thus?
        }
    }
}