using System;
using System.Linq;
using ShadowMUD.MudObjects;
using Object=ShadowMUD.MudObjects.Object;

namespace ShadowMUD.Cache
{
    public class ObjectDbCache : LRUCache<Object>
    {
        private const string SelectObjectWearFlagsFormat = "SELECT * FROM object_wearflags AS owf WHERE owf.id = {0}";
        private const string SelectObjectFlagsFormat = "SELECT * FROM object_flags AS of WHERE owf.id = {0}";
        private const string SelectObjectDetailsFormat = "SELECT * FROM object_details AS od WHERE od.id = {0}";
        private const string SelectObjectAffectsFormat = "SELECT * FROM object_affects AS oa WHERE oa.objectid = {0}";
        private const string SelectObjectFormat = "SELECT * FROM objects AS o WHERE o.id = {0};";
        
        private const string ValidateFormat = "SELECT updates FROM updates WHERE id = {0}";

        private readonly IIndex<int> _findByObjectID;
        private readonly ShadowDb _database;
        private long _tableVersion;

        /// <summary>constructor creates cache and multiple indexes</summary>
        public ObjectDbCache(ShadowDb db)
            : base(20, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(20), null)
        {
            _database = db;
            IsValid = IsDataValid;
            _findByObjectID = AddIndex("ObjectID", obj => obj.ID, LoadFromObjectID);
            IsDataValid();
        }

        /// <summary>retrieve items by userid</summary>
        public Object FindByObjectID(int objectid)
        {
            return _findByObjectID[objectid];
        }

        /// <summary>check to see if users table has changed, if so dump cache and reload.</summary>
        internal bool IsDataValid()
        {
            var query = string.Format(ValidateFormat, (int)TableID.Objects);
            var oldVersion = _tableVersion;
            _tableVersion = _database.ExecuteCommand(query);
            return (oldVersion == _tableVersion);
        }

        /// <summary>when FindByUserID can't find a user, this method loads the data from the db</summary>
        private Object LoadFromObjectID(int roomNumber)
        {
            var query = string.Format(SelectObjectFormat, roomNumber);

            var objects = _database.ExecuteQuery<Object>(query);

            if (objects.Count() == 0)
                return null;

            var obj = objects.First();

            query = string.Format(SelectObjectDetailsFormat, obj.ID);
            
            var details = _database.ExecuteQuery<ObjectDetail>(query);

            if (details.Count() != 0)
                obj.Detail = details.First();

            query = string.Format(SelectObjectWearFlagsFormat, obj.ID);

            var wearflags = _database.ExecuteQuery<ObjectWearFlags>(query);

            if (wearflags.Count() != 0)
                obj.Wearflags = wearflags.First();

            query = string.Format(SelectObjectFlagsFormat, obj.ID);

            var flags = _database.ExecuteQuery<ObjectFlags>(query);

            if (flags.Count() != 0)
                obj.Flags = flags.First();
            
            query = string.Format(SelectObjectAffectsFormat, obj.ID);

            var objectAffects = _database.ExecuteQuery<ObjectAffect>(query);

            if (objectAffects.Count() == 0)
                return obj;

            foreach (var affect in objectAffects)
                obj.AddAffect(affect);

            return obj;
        }
    }
}