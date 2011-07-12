using System.Collections.Generic;
using System.Data;
using ShadowMUD.MudObjects;

namespace ShadowMUD.Managers
{
    internal class ZoneManager
    {
        //private const string newZoneDetailQuery = "SELECT nextval('zone_details_sequence');";
        private const string SelectZonesQuery = "SELECT * FROM zones ORDER BY id ASC;";
        private readonly ShadowDb _database;

        private readonly Dictionary<int, Zone> _zoneMap;

        public ZoneManager(IDbConnection connection)
        {
            _zoneMap = new Dictionary<int, Zone>();
            _database = new ShadowDb(connection);
        }

        public bool LoadZones()
        {
            var zones = _database.ExecuteQuery<Zone>(SelectZonesQuery);
            //database.Transaction
            foreach (var zone in zones)
            {
                _zoneMap.Add(zone.ID, zone);
            }

            return true;
        }

        public void AddZone(Zone zone)
        {
            _database.Zones.InsertOnSubmit(zone);
            _database.SubmitChanges();
        }

        public void AddZoneDetail(int zoneID, int roomID)
        {
            var zoneDetail = new ZoneDetail
            {
                ZoneID = zoneID,
                RoomID = roomID
            };

            _database.ZoneDetails.InsertOnSubmit(zoneDetail);
            _database.SubmitChanges();
        }
    }
}