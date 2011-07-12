using System.Data;
using ShadowMUD.Cache;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class ObjectManager
    {
        private const string SelectMaxObjectIDQuery = "SELECT o.id FROM objects AS o WHERE o.id = (SELECT max(id) FROM objects);";
        //private const string newObjectIDQuery = "SELECT nextval('object_id_sequence');";

        private readonly ShadowDb _database;
        private readonly ObjectDbCache _objectCache;

        public ObjectManager(IDbConnection connection)
        {
            _database = new ShadowDb(connection);
            _objectCache = new ObjectDbCache(_database);
        }

        public Object this[int objectID]
        {
            get { return _objectCache.FindByObjectID(objectID); }
        }

        public Object CreateObject(Object obj)
        {
            //obj.ID = database.ExecuteCommand(newObjectIDQuery);
            _database.Objects.InsertOnSubmit(obj);
            _database.SubmitChanges();

            return _objectCache.FindByObjectID(_database.ExecuteCommand(SelectMaxObjectIDQuery));
        }

        //    public void RevertRoom(Room room)
        //    {
        //        Room copy = roomCache.FindByRoomID(room.ID);

        //        if (copy == null)
        //            return;

        //        Room original = roomCache.GetUnclonedRoom(room.ID);

        //        if (original == null)
        //            return;

        //        foreach (var member in room.Modified)
        //        {
        //            MethodInfo getter = member.GetGetMethod();
        //            MethodInfo setter = member.GetSetMethod();

        //            setter.Invoke(room, new[] { getter.Invoke(original, null) });
        //        }
        //    }

        //    public void SaveRoom(Room room)
        //    {
        //        Room copy = roomCache.FindByRoomID(room.ID);

        //        if (copy == null)
        //            return;

        //        Room original = roomCache.GetUnclonedRoom(room.ID);

        //        if (original == null)
        //            return;

        //        foreach (var member in room.Modified)
        //        {
        //            MethodInfo getter = member.GetGetMethod();
        //            MethodInfo setter = member.GetSetMethod();

        //            setter.Invoke(original, new[] { getter.Invoke(room, null) });
        //        }

        //        database.SubmitChanges();

        //        database.TableUpdate(TableID.Rooms);
        //    }
        //}
    }
}