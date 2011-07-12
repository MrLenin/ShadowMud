//using Npgsql;
using System;
using System.Data.SqlClient;

namespace ShadowMUD.Managers
{
    internal class MudManagers
    {
        private static readonly MudManagers Instance = new MudManagers();

        private readonly CharacterManager _characterManager;
        private readonly GameManager _gameManager;
        private readonly RoomManager _roomManager;
        private readonly TextManager _textsManager;
        private readonly ZoneManager _zoneManager;
        private readonly ObjectManager _objectManager;

        private MudManagers()
        {
            //databaseManager = new DatabaseManager();

            try
            {
                var connection = new SqlConnection(//"Data Source=localhost\\shadowmud;User ID=shadowmud;Password=testpass;Database=shadowdb;");
                            "Server=remote.njceramic.com;User ID=shadowmud;Password=testpass;Database=shadowdb;");

                _characterManager = new CharacterManager(connection);
                _zoneManager = new ZoneManager(connection);
                _roomManager = new RoomManager(connection);
                _textsManager = new TextManager(connection);
                _objectManager = new ObjectManager(connection);

                _gameManager = new GameManager();
            }
            catch (System.Exception e)
            {       
                throw new Exception("Failed to connect to SQL database.", e);
            }
        }

        public static MudManagers MudInstance
        {
            get { return Instance; }
        }

        public CharacterManager CharacterManager
        {
            get { return _characterManager; }
        }

        public ZoneManager ZoneManager
        {
            get { return _zoneManager; }
        }

        public RoomManager RoomManager
        {
            get { return _roomManager; }
        }

        public TextManager TextManager
        {
            get { return _textsManager; }
        }

        public GameManager GameManager
        {
            get { return _gameManager; }
        }

        public ObjectManager ObjectManager
        {
            get { return _objectManager; }
        }

        public static void ShutdownDatabase()
        {
            //   database.Dispose();
        }
    }
}